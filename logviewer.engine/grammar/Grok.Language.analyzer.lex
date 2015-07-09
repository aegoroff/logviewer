%namespace logviewer.engine.grammar
%scannertype GrokScanner
%visibility internal
%tokentype Token

%option stack, minimize, parser, verbose, persistbuffer, noembedbuffers 


COMMA   ","
DOT     "."
ARROW   "->"
COLON   ":"
UNDERSCORE "_"
INT   "int"
INT32   "Int32"
INT64   "Int64"
LONG   "long"
DATE_TIME   "DateTime"
LOG_LEVEL   "LogLevel"
STRING_TYPE  [Ss]tring
LEVEL_TRACE   "Trace"
LEVEL_DEBUG   "Debug"
LEVEL_INFO   "Info"
LEVEL_WARN   "Warn"
LEVEL_ERROR   "Error"
LEVEL_FATAL   "Fatal"

UPPER_LETTER [A-Z]
LOWER_LETTER [a-z]
DIGIT [0-9]


OPEN "%{"
CLOSE "}" 

PATTERN {UPPER_LETTER}({UPPER_LETTER}|{DIGIT}|{UNDERSCORE})*
PROPERTY ({LOWER_LETTER}|{UPPER_LETTER})({LOWER_LETTER}|{UPPER_LETTER}|{DIGIT}|{UNDERSCORE})*

NOT_QUOTED_STR  ([^%]|%[^\{]|%\{\})+
STR_ESCAPE_SEQ "\\".

QUOTED_STR "'"({STR_ESCAPE_SEQ}|[^\\\r\n'])*"'"|"\""({STR_ESCAPE_SEQ}|[^\\\r\n"])*"\""

SKIP {NOT_QUOTED_STR}|{QUOTED_STR}

TYPE_NAME {INT}|{INT32}|{INT64}|{LONG}|{LOG_LEVEL}|{DATE_TIME}|{STRING_TYPE}

TYPE_MEMBER {LEVEL_TRACE}|{LEVEL_DEBUG}|{LEVEL_INFO}|{LEVEL_WARN}|{LEVEL_ERROR}|{LEVEL_FATAL}
CASTING_PATTERN {QUOTED_STR}

%x INPATTERN

%%

/* Scanner body */

{OPEN} {  yy_push_state (INPATTERN); return (int)Token.OPEN; }

<INPATTERN>{
    {COMMA} { return (int)Token.COMMA; }
    {DOT} { return (int)Token.DOT; }
    {ARROW} { return (int)Token.ARROW; }
    {COLON} { return (int)Token.COLON; }
    
	{UNDERSCORE} { throw new Exception("Unexpected underscore inside"); }
	{DIGIT} { throw new Exception("Unexpected digit inside pattern"); }
    
    {INT} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }
    {INT32} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }
    {INT64} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }
    {LONG} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }
    {DATE_TIME} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }
    {LOG_LEVEL} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }
    {STRING_TYPE} { yylval.Parser = yytext; return (int)Token.TYPE_NAME; }

    {LEVEL_TRACE} { yylval.Parser = yytext; yylval.TypeMember = LogLevel.Trace; return (int)Token.TYPE_MEMBER; }
    {LEVEL_DEBUG} { yylval.Parser = yytext; yylval.TypeMember = LogLevel.Debug; return (int)Token.TYPE_MEMBER; }
    {LEVEL_INFO} { yylval.Parser = yytext; yylval.TypeMember = LogLevel.Info; return (int)Token.TYPE_MEMBER; }
    {LEVEL_WARN} { yylval.Parser = yytext; yylval.TypeMember = LogLevel.Warn; return (int)Token.TYPE_MEMBER; }
    {LEVEL_ERROR} { yylval.Parser = yytext; yylval.TypeMember = LogLevel.Error; return (int)Token.TYPE_MEMBER; }
    {LEVEL_FATAL} { yylval.Parser = yytext; yylval.TypeMember = LogLevel.Fatal; return (int)Token.TYPE_MEMBER; }

    {PATTERN} { yylval.Pattern = yytext; return (int)Token.PATTERN; }
    {PROPERTY} { yylval.Property = yytext; return (int)Token.PROPERTY; }

    {CASTING_PATTERN} { yylval.CastingPattern = yytext; return (int)Token.CASTING_PATTERN; }

    {CLOSE} {  yy_pop_state(); return (int)Token.CLOSE; }
}

{SKIP} { yylval.Literal = yytext; return (int)Token.SKIP; }

%%