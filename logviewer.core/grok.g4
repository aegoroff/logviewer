grammar Grok;

parse: grok (literal? grok)* literal? ;       

grok
	: OPEN SYNTAX (SEPARATOR SEMANTIC)? CLOSE # Find
	; 

literal
	: STRING+ # Paste
	;

// string MUST be non greedy
STRING : ~[%}{]+? ;

SYNTAX : [0-9A-Z_]+ ;
SEMANTIC : [a-zA-Z] [0-9a-zA-Z_]* ;
OPEN : '%{' ;
CLOSE : '}' ;
SEPARATOR : ':' ;