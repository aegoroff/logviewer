// Created by: egr
// Created at: 02.10.2014
// Grok templating syntax
// © 2012-2015 Alexander Egorov

grammar Grok;

@lexer::members {
	public bool inPattern;
	public bool inSemantic;
	public void InPattern() { inPattern = true; }
	public void OutPattern() { inPattern = false; }
	public void InSemantic() { inSemantic = true; }
	public void OutSemantic() { inSemantic = false; }
}

parse
	: (literal? grok)* literal? 
	;

literal
	: SKIP # OnLiteral
	;

grok
	: OPEN definition CLOSE
	;

definition
	: PATTERN semantic?  # OnRule
	;

semantic
	: COLON PROPERTY casting? # OnSemantic
	;

casting
	: COMMA (TYPE_NAME | cast (COMMA cast)*) # OnCasting
	;

cast
	: QUOTED_STR ARROW target # OnCastingCustomRule
	;

target
	: TYPE_NAME (DOT TYPE_MEMBER)*
	;

SKIP : (QUOTED_STR | NOT_QUOTED_STR) {!inPattern}?;

PATTERN 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | UNDERSCORE)* {!inSemantic}?
	;

// MUST follow SKIP rule but not to be before it
PROPERTY
	: (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT | UNDERSCORE)* { InSemantic(); } {!inSemantic}?
	;

OPEN : '%{' { InPattern(); };
CLOSE : '}' { OutPattern(); OutSemantic(); };


fragment NOT_QUOTED_STR : (~[%])+  ;

QUOTED_STR : SHORT_STRING ;

TYPE_NAME
	: (INT | INT32 | INT64 | LONG | LOG_LEVEL | DATE_TIME | STRING_TYPE | STRING_ALT_TYPE) {inSemantic}?
	;

TYPE_MEMBER
	: (LEVEL_TRACE | LEVEL_DEBUG | LEVEL_INFO | LEVEL_WARN | LEVEL_ERROR | LEVEL_FATAL) {inSemantic}?
	;

COMMA : ',' ;
DOT : '.' ;
ARROW : '->' ;
COLON : ':' ;

fragment UNDERSCORE : '_' ;

fragment UPPER_LETTER : 'A' .. 'Z' ;
fragment LOWER_LETTER : 'a' .. 'z' ;
fragment DIGIT : '0' .. '9' ;

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