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


parse  : { Console.WriteLine("Start parse"); }  groks literal
       ;

literal 
    :
    | SKIP { Console.WriteLine("- Literal: {0}", $1); } // TODO: OnLiteral
    ;

groks 
    : literal grok
    | literal grok groks
    ;

grok
	: OPEN definition CLOSE
	;

definition
	: { Console.WriteLine("start definition"); } PATTERN semantic  { Console.WriteLine("- Pattern: {0}", $1); } // TODO: OnRule
	;

semantic
	: 
    | COLON PROPERTY casting { Console.WriteLine("-- Property: {0}", $2); } // TODO: OnSemantic
	;

casting
	: COMMA type_cast {} // TODO: OnCasting
    | COMMA casting_list {} // TODO: OnCasting
	;
    
type_cast
	: TYPE_NAME { Console.WriteLine("--- Parser: {0}", $1); }
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
	: DOT TYPE_MEMBER { Console.WriteLine("-- Type member: {0}", $2); }
	;

%%