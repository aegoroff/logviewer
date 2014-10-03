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

