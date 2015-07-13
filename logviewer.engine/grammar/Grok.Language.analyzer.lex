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

NOT_QUOTED_STR  ([^%\r\n]|%[^\{]|%\{\})+
STR_ESCAPE_SEQ "\\".

QUOTED_STR "'"({STR_ESCAPE_SEQ}|[^\\\r\n'])*"'"|"\""({STR_ESCAPE_SEQ}|[^\\\r\n"])*"\""

COMMENT ^#[^\r\n]*
WS [ \t]+ // whitespaces
CRLF (\r)?(\n)

PATTERN_DEFINITION {PATTERN}
LITERAL {NOT_QUOTED_STR}|{QUOTED_STR}

TYPE_NAME {INT}|{INT32}|{INT64}|{LONG}|{LOG_LEVEL}|{DATE_TIME}|{STRING_TYPE}

LEVEL {LEVEL_TRACE}|{LEVEL_DEBUG}|{LEVEL_INFO}|{LEVEL_WARN}|{LEVEL_ERROR}|{LEVEL_FATAL}
CASTING_PATTERN {QUOTED_STR}

%x INPATTERN
%x INDEFINITION
%x INGROK
%x INCOMMENT

%%

/* Scanner body */

{PATTERN_DEFINITION} { yy_push_state (INDEFINITION); yylval.Pattern = yytext; return (int)Token.PATTERN_DEFINITION; }

{COMMENT} { yy_push_state (INCOMMENT); return (int)Token.COMMENT; }

<INDEFINITION>{
	{WS} { yy_push_state (INGROK); return (int)Token.WS; }
}

<INGROK>{
    {CRLF} { 
		yy_pop_state(); // INGROK
		yy_pop_state(); // INDEFINITION
		return (int)Token.CRLF; 
	}
}

<INCOMMENT>{
    {CRLF} { 
		yy_pop_state(); // INCOMMENT
		return (int)Token.CRLF; 
	}
}

<INGROK> {
	{OPEN} {  yy_push_state (INPATTERN); return (int)Token.OPEN; }
	{LITERAL} { yylval.Literal = yytext; return (int)Token.LITERAL; }
}

<INPATTERN>{
    {COMMA} { return (int)Token.COMMA; }
    {DOT} { return (int)Token.DOT; }
    {ARROW} { return (int)Token.ARROW; }
    {COLON} { return (int)Token.COLON; }
    
	{UNDERSCORE} { throw new Exception("Unexpected underscore inside"); }
	{DIGIT} { throw new Exception("Unexpected digit inside pattern"); }
    
    {INT} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {INT32} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {INT64} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {LONG} { yylval.Parser = ParserType.Interger; return (int)Token.TYPE_NAME; }
    {DATE_TIME} { yylval.Parser = ParserType.Datetime; return (int)Token.TYPE_NAME; }
    {LOG_LEVEL} { yylval.Parser = ParserType.LogLevel; return (int)Token.TYPE_NAME; }
    {STRING_TYPE} { yylval.Parser = ParserType.String; return (int)Token.TYPE_NAME; }

    {LEVEL_TRACE} { yylval.Parser = ParserType.LogLevel; yylval.Level = LogLevel.Trace; return (int)Token.LEVEL; }
    {LEVEL_DEBUG} { yylval.Parser = ParserType.LogLevel; yylval.Level = LogLevel.Debug; return (int)Token.LEVEL; }
    {LEVEL_INFO} { yylval.Parser = ParserType.LogLevel; yylval.Level = LogLevel.Info; return (int)Token.LEVEL; }
    {LEVEL_WARN} { yylval.Parser = ParserType.LogLevel; yylval.Level = LogLevel.Warn; return (int)Token.LEVEL; }
    {LEVEL_ERROR} { yylval.Parser = ParserType.LogLevel; yylval.Level = LogLevel.Error; return (int)Token.LEVEL; }
    {LEVEL_FATAL} { yylval.Parser = ParserType.LogLevel; yylval.Level = LogLevel.Fatal; return (int)Token.LEVEL; }

    {PATTERN} { yylval.Pattern = yytext; return (int)Token.PATTERN; }
    {PROPERTY} { yylval.Property = yytext; return (int)Token.PROPERTY; }

    {CASTING_PATTERN} { yylval.CastingPattern = yytext; return (int)Token.CASTING_PATTERN; }

    {CLOSE} {  yy_pop_state(); return (int)Token.CLOSE; }
}

%%