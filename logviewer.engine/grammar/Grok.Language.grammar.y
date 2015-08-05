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

%start translation_unit

%token COMMA
%token DOT
%token ARROW
%token COLON
%token OPEN
%token CLOSE
%token <Literal> LITERAL
%token <Pattern> PATTERN
%token <Pattern> PATTERN_DEFINITION
%token <CastingPattern> CASTING_PATTERN
%token <Parser> TYPE_NAME
%token <Level> LEVEL
%token <Property> PROPERTY
%token CRLF
%token WS
%token COMMENT

%%

translation_unit : lines ;

lines 
    : line 
    | lines CRLF line
    ;
	
line
    : PATTERN_DEFINITION { OnPatternDefinition($1); } WS groks
    | COMMENT
    | CRLF
    | EOF
    ;

groks 
    : grok
    | groks grok
    ;

grok
	: pattern
	| literal
	;
	
pattern
	: OPEN definition CLOSE
	;

literal 
    : LITERAL { OnLiteral($1); } 
    ;

definition
	: PATTERN { OnPattern($1); } 
	| PATTERN { OnPattern($1); } semantic
	;

semantic
	: property { OnRule(ParserType.String, GrokRule.DefaultPattern); } // No explicit schema case. Add generic string rule
    | property casting
	;
    
property
	: COLON PROPERTY { this.OnSemantic($2); }
	;

casting
	: COLON type { OnRule($2.Parser, GrokRule.DefaultPattern); }
    | COLON castings
	;
    
type
	: TYPE_NAME { $$.Parser = $1; }
	;
    
castings
	: cast
	| castings COMMA cast
	;

cast
	: CASTING_PATTERN ARROW target { OnRule($3.Parser, $1, $3.Level); }
	;

target
	: TYPE_NAME { $$.Parser = $1; }
	| TYPE_NAME member { $$.Parser = $1; $$.Level = $2.Level; }
	;
    
member
	: DOT LEVEL { $$.Level = $2; }
	;

%%