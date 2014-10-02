grammar Grok;

parse: grok (literal? grok)* literal? ;       

grok
	: OPEN ID CLOSE # Find
	; 

literal
	: STRING+ # Paste
	;

// string MUST be non greedy
STRING : ~[%}{]+? ;

ID : [0-9A-Z_]+ ;
OPEN : '%{' ;
CLOSE : '}' ;