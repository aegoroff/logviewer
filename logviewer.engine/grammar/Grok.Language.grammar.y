%namespace logviewer.engine.grammar
%partial
%parsertype GrokParser
%visibility internal
%tokentype Token

%union { 
			public string Pattern;
			public string Property;
			public string Literal;
			public string CastLValue;
			public string Parser;
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
%token <CastLValue> L_CAST
%token <Parser> TYPE_NAME
%token <TypeMember> TYPE_MEMBER
%token <Property> PROPERTY

%{
    private readonly Composer composer = new Composer();
    private readonly IDictionary<string, string> templates;
    private readonly List<Semantic> schema = new List<Semantic>();
    private IPattern compiledPattern;
    private string property;
%}


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
    : SKIP { 
        var literal = new StringLiteral($1);
        this.composer.Add(literal);
     } 
    ;

definition
	: PATTERN semantic  { this.AddSemantic($2.Property); OnRule($1); }
	;

semantic
	: 
    | property_ref { AddRule("string", "*"); $$ = $1; }
    | property_ref casting { $$ = $1; }
	;
    
property_ref
	: COLON PROPERTY { $$.Property = $2; }
	;

casting
	: COMMA type_cast { AddRule($2.Parser, "*"); }
    | COMMA casting_list
	;
    
type_cast
	: TYPE_NAME { $$.Parser = $1; }
	;
    
casting_list
	: cast
	| cast COMMA casting_list
	;

cast
	: L_CAST ARROW target { AddRule($3.Parser, $1); }
	;

target
	: TYPE_NAME opt_members { $$.Parser = $1; }
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
	: DOT TYPE_MEMBER
	;

%%