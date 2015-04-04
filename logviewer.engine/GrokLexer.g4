// Created by: egr
// Created at: 02.10.2014
// Grok templating syntax
// © 2012-2015 Alexander Egorov

lexer grammar GrokLexer;

OPEN : PERCENT OPEN_BRACE  -> pushMode(PATTERN_MODE);
CLOSE : CLOSE_BRACE -> popMode;


SKIP : (QUOTED_STR | NOT_QUOTED_STR) ;

fragment NOT_QUOTED_STR : (~[}{%])+  ;

QUOTED_STR : SHORT_STRING ;


COMMA : ',' ;
DOT : '.' ;
ARROW : '->' ;
COLON : ':' ;

fragment UNDERSCORE : '_' ;

PROPERTY
	: (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT | UNDERSCORE)*  -> pushMode(SEMANTIC_MODE)
	;

fragment UPPER_LETTER : 'A' .. 'Z' ;
fragment LOWER_LETTER : 'a' .. 'z' ;
fragment DIGIT : '0' .. '9' ;
fragment PERCENT : '%' ;
fragment OPEN_BRACE : '{' ;
fragment CLOSE_BRACE : '}';

/// shortstring     ::=  "'" shortstringitem* "'" | '"' shortstringitem* '"'
/// shortstringitem ::=  shortstringchar | stringescapeseq
/// shortstringchar ::=  <any source character except "\" or newline or the quote>
fragment SHORT_STRING
 : '\'' ( STRING_ESCAPE_SEQ | ~[\\\r\n'] )* '\''
 | '"' ( STRING_ESCAPE_SEQ | ~[\\\r\n"] )* '"'
 ;

/// stringescapeseq ::=  "\" <any source character>
fragment STRING_ESCAPE_SEQ
 : '\\' .
 ;

fragment INT : 'int' ;
fragment INT32 : 'Int32' ;
fragment INT64 : 'Int64' ;
fragment LONG : 'long' ;
fragment DATE_TIME : 'DateTime' ;
fragment LOG_LEVEL : 'LogLevel' ;
fragment STRING_TYPE : 'string' ;
fragment STRING_ALT_TYPE : 'String' ;

fragment LEVEL_TRACE : 'Trace' ;
fragment LEVEL_DEBUG : 'Debug' ;
fragment LEVEL_INFO : 'Info' ;
fragment LEVEL_WARN : 'Warn' ;
fragment LEVEL_ERROR : 'Error' ;
fragment LEVEL_FATAL : 'Fatal' ;

mode SEMANTIC_MODE ;

TYPE_NAME
	: (INT | INT32 | INT64 | LONG | LOG_LEVEL | DATE_TIME | STRING_TYPE | STRING_ALT_TYPE)
	;

TYPE_MEMBER
	: (LEVEL_TRACE | LEVEL_DEBUG | LEVEL_INFO | LEVEL_WARN | LEVEL_ERROR | LEVEL_FATAL)
	;

mode PATTERN_MODE ;

PATTERN 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | UNDERSCORE)* -> popMode
	;