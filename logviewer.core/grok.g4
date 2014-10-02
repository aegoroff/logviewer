grammar Grok;

parse
	: grok (literal? grok)* literal? 
	;

grok
	: OPEN SYNTAX CLOSE                   # Replace
	| OPEN SYNTAX SEMANTIC CASTING? CLOSE # Replace
	;

literal
	: STR+ # Paste
	;

SYNTAX 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | '_')* 
	;

SEMANTIC
	: SEPARATOR (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT)* 
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

