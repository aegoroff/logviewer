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


NOT_QUOTED_STR  [^%]+
STR_ESCAPE_SEQ "\\".

QUOTED_STR "'"({STR_ESCAPE_SEQ}|[^\\\r\n'])*"'"|"\""({STR_ESCAPE_SEQ}|[^\\\r\n"])*"\""

PATTERN {UPPER_LETTER}({UPPER_LETTER}|{DIGIT}|{UNDERSCORE})*

PROPERTY ({LOWER_LETTER}|{UPPER_LETTER})({LOWER_LETTER}|{UPPER_LETTER}|{DIGIT}|{UNDERSCORE})*


SKIP {NOT_QUOTED_STR}|{QUOTED_STR}

TYPE_NAME {INT}|{INT32}|{INT64}|{LONG}|{LOG_LEVEL}|{DATE_TIME}|{STRING_TYPE}

TYPE_MEMBER {LEVEL_TRACE}|{LEVEL_DEBUG}|{LEVEL_INFO}|{LEVEL_WARN}|{LEVEL_ERROR}|{LEVEL_FATAL}
L_CAST {QUOTED_STR}

%{

%}

%x INPATTERN

%%

/* Scanner body */

{OPEN} {  yy_push_state (INPATTERN); return (int)Token.OPEN; }

<INPATTERN>{
    {COMMA} { return (int)Token.COMMA; }
    {DOT} { return (int)Token.DOT; }
    {ARROW} { return (int)Token.ARROW; }
    {COLON} { return (int)Token.COLON; }
    
    {INT} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {INT32} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {INT64} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {LONG} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {DATE_TIME} { yylval.Parser = ParserType.Datetime; return (int)Token.TYPE_NAME; }
    {LOG_LEVEL} { yylval.Parser = ParserType.LogLevel; return (int)Token.TYPE_NAME; }
    {STRING_TYPE} { yylval.Parser = ParserType.String; return (int)Token.TYPE_NAME; }

    {LEVEL_TRACE} { yylval.TypeMember = LogLevel.Trace; return (int)Token.TYPE_MEMBER; }
    {LEVEL_DEBUG} { yylval.TypeMember = LogLevel.Debug; return (int)Token.TYPE_MEMBER; }
    {LEVEL_INFO} { yylval.TypeMember = LogLevel.Info; return (int)Token.TYPE_MEMBER; }
    {LEVEL_WARN} { yylval.TypeMember = LogLevel.Warn; return (int)Token.TYPE_MEMBER; }
    {LEVEL_ERROR} { yylval.TypeMember = LogLevel.Error; return (int)Token.TYPE_MEMBER; }
    {LEVEL_FATAL} { yylval.TypeMember = LogLevel.Fatal; return (int)Token.TYPE_MEMBER; }

    {PATTERN} { yylval.Pattern = yytext; return (int)Token.PATTERN; }
    {PROPERTY} { yylval.Property = yytext; return (int)Token.PROPERTY; }

    {L_CAST} { return (int)Token.L_CAST; }

    {CLOSE} {  yy_pop_state(); return (int)Token.CLOSE; }
}

{SKIP} { yylval.Literal = yytext; return (int)Token.SKIP; }

%%