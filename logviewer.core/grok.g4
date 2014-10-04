grammar Grok;

parse
	: literal? grok (literal? grok)* literal? 
	;

grok
	: OPEN syntax SEMANTIC? CLOSE # Replace
	;

literal
	: STR+ # Passthrough
	;

syntax
	: PREDEFINED 
	| CUSTOM
	;

PREDEFINED 
	: 'LOGLEVEL' 
	| 'TIMESTAMP_ISO8601' 
	| 'WORD' 
	| 'SPACE' 
	| 'DATA' 
	| 'GREEDYDATA' 
	| 'INT' 
	| 'BASE10NUM' 
	| 'BASE16NUM' 
	| 'BASE16FLOAT' 
	| 'POSINT' 
	| 'NONNEGINT' 
	| 'NOTSPACE' 
	| 'QUOTEDSTRING' 
	| 'YEAR' 
	| 'HOUR' 
	| 'MINUTE' 
	| 'SECOND' 
	| 'MONTH' 
	| 'MONTHNUM' 
	| 'MONTHDAY' 
	| 'TIME' 
	| 'DATE_US' 
	| 'DATE_EU' 
	| 'ISO8601_TIMEZONE' 
	| 'ISO8601_SECOND' 
	;

CUSTOM 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | '_')* 
	;

SEMANTIC
	: SEPARATOR (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT)* CASTING?
	;

CASTING 
	: SEPARATOR (LOWER_LETTER | UPPER_LETTER)+ 
	;

OPEN : '%{' ;
CLOSE : '}' ;

// string MUST be non greedy!
STR : ~[}{%]+? ;

fragment SEPARATOR : ':' ;
fragment UPPER_LETTER : 'A' .. 'Z' ;
fragment LOWER_LETTER : 'a' .. 'z' ;
fragment DIGIT : '0' .. '9' ;

