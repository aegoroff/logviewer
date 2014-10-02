grammar Grok;

parse
	: grok (literal? grok)* literal? 
	;

grok
	: OPEN syntax (semantic casting?)? CLOSE # Replace
	;

literal
	: STR+ # Paste
	;

syntax 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | '_')* 
	;

semantic
	: SEPARATOR (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT)* 
	;

casting 
	: SEPARATOR (LOWER_LETTER | UPPER_LETTER)+ 
	;

OPEN : '%{' ;
CLOSE : '}' ;
SEPARATOR : ':' ;
// string MUST be non greedy!
STR : ~[}{%]+? ;

UPPER_LETTER : 'A' .. 'Z'+ ;
LOWER_LETTER : 'a' .. 'z'+ ;
DIGIT : '0' .. '9'+ ;

