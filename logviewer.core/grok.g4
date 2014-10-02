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

ID : [A-Z]+ ;
OPEN : '%{' ;
CLOSE : '}' ;