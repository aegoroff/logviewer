%namespace logviewer.engine.grammar
%partial
%parsertype GrokParser
%visibility internal
%tokentype Token

%union { 
			public string Pattern;
			public string Property;
			public string Literal;
			public string CastingPattern;
			public ParserType Parser;
			public LogLevel Level;
	   }

%start parse

%token COMMA
%token DOT
%token ARROW
%token COLON
%token OPEN
%token CLOSE
%token <Literal> SKIP
%token <Pattern> PATTERN
%token <CastingPattern> CASTING_PATTERN
%token <Parser> TYPE_NAME
%token <Level> LEVEL
%token <Property> PROPERTY


%%


parse  : /* Empty */
       | groks
       ;

groks 
    : grok
    | groks grok
    ;

grok
	: OPEN definition CLOSE
	| literal
	;

literal 
    : SKIP { 
        var literal = new StringLiteral($1);
        this.composer.Add(literal);
     } 
    ;

definition
	: PATTERN semantic  { 
        OnPattern($1); 
      }
	;

semantic
	: /* Empty */
    | property_ref { 
        AddRule(ParserType.String, "*"); // No schema case. Add generic string rule
        this.OnSemantic($1.Property); 
      }
    | property_ref casting { this.OnSemantic($2.Property); }
	;
    
property_ref
	: COLON PROPERTY { $$.Property = $2; }
	;

casting
	: COLON type_cast { AddRule($2.Parser, "*"); }
    | COLON casting_list
	;
    
type_cast
	: TYPE_NAME { $$.Parser = $1; }
	;
    
casting_list
	: cast
	| casting_list COMMA cast
	;

cast
	: CASTING_PATTERN ARROW target { AddRule($3.Parser, $1, $3.Level); }
	;

target
	: TYPE_NAME opt_member { $$.Parser = $1; $$.Level = $2.Level; }
	;
    
opt_member
	: /* Empty */
    | member
	;
    
member
	: DOT LEVEL { $$.Level = $2; }
	;

%%