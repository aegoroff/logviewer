grammar Grok;

parse: grok (literal? grok)* literal? # Build
;       

grok
	: OPEN ID CLOSE # Find
	; 


literal
	: STRING # Paste
	;

STRING : ~[%}{]+? ;

ID : [A-Z]+ ;
OPEN : '%{' ;
CLOSE : '}' ;