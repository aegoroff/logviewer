grammar Grok;

parse: grok+ # Build
;       

grok: OPEN ID CLOSE # Find
;  

ID : [A-Z]+ ;
OPEN : '%{' ;
CLOSE : '}' ;

WS :   [ \t\n\r]+ -> skip ;