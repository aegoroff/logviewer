grammar Grok;

parse: grok* # Build
;       

grok
	: OPEN ID CLOSE # Find
	| literal #Paste
	; 

literal
    : ~( OPEN | CLOSE )+
    ;

ID : [A-Z]+ ;
OPEN : '%{' ;
CLOSE : '}' ;