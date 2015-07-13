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
%token <Literal> SKIP
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

translation_unit : /* Empty */
       | pattern_definitions
       ;

pattern_definitions 
    : pattern_definition_or_comment 
    | pattern_definitions CRLF pattern_definition_or_comment
    ;
	
pattern_definition_or_comment 
    : PATTERN_DEFINITION WS parse
    | COMMENT
    ;

parse  : /* Empty */
       | groks
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
    : SKIP { OnLiteral($1); } 
    ;

definition
	: PATTERN semantic { OnPattern($1); }
	;

semantic
	: /* Empty */
    | property_ref { this.OnSemantic($1.Property); }
    | property_ref casting { this.OnSemantic($1.Property); }
	;
    
property_ref
	: COLON PROPERTY { $$.Property = $2; }
	;

casting
	: COLON type_cast { OnRule($2.Parser, GrokRule.DefaultPattern); }
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
	: CASTING_PATTERN ARROW target { OnRule($3.Parser, $1, $3.Level); }
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