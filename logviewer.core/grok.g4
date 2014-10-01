grammar Grok;

parse: grok+ ;

grok: '%{' ID '}' ;

ID : [A-Z]+ ;