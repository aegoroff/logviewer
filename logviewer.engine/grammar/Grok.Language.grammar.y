%namespace logviewer.engine.grammar
%partial
%parsertype GrokParser
%visibility internal
%tokentype Token

%union { 
			public string Pattern;
			public string Property;
			public string Literal;
			public ParserType Parser;
			public LogLevel TypeMember;
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
%token L_CAST
%token <Parser> TYPE_NAME
%token <TypeMember> TYPE_MEMBER
%token <Property> PROPERTY

%%


parse  : opt_groks
       ;

opt_groks :
    | groks
    ;

groks 
    : grok
    | grok groks
    ;

grok
	: OPEN definition CLOSE
	| literal
	;

literal 
    : SKIP { customErrorOutputMethod(string.Format("- Literal: {0}", $1)); } // TODO: OnLiteral
    ;

definition
	: PATTERN semantic  { customErrorOutputMethod(string.Format("- Pattern: {0}", $1)); } // TODO: OnRule
	;

semantic
	: 
    | property_ref 
    | property_ref casting
	;
    
property_ref
	: COLON PROPERTY { customErrorOutputMethod(string.Format("-- Property: {0}", $2)); } // TODO: OnSemantic
	;

casting
	: COMMA type_cast {} // TODO: OnCasting
    | COMMA casting_list {} // TODO: OnCasting
	;
    
type_cast
	: TYPE_NAME { customErrorOutputMethod(string.Format("--- Parser: {0}", $1)); }
	;
    
casting_list
	: cast
	| cast COMMA casting_list
	;

cast
	: L_CAST ARROW target {} // TODO: OnCastingCustomRule
	;

target
	: TYPE_NAME opt_members
	;
    
opt_members
	: 
    | members 
	;

members
    : member
	| member members
	;
    
member
	: DOT TYPE_MEMBER { customErrorOutputMethod(string.Format("-- Type member: {0}", $2)); }
	;

%%