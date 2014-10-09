// Created by: egr
// Created at: 02.10.2014
// Grok templating syntax
// © 2012-2014 Alexander Egorov

grammar Grok;

parse
	: (literal? grok)* literal? 
	;

literal
	: STR+ # Passthrough
	;

grok
	: OPEN definition CLOSE
	;

definition
	: syntax semantic?  # Replace
	;

syntax
	: PATTERN
	;

semantic
	: SEMANTIC
	;

PATTERN 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | '_')* 
	;

SEMANTIC
	: SEPARATOR PROPERTY CASTING?
	;

CASTING 
	: SEPARATOR TYPE_NAME
	;

OPEN : '%{' ;
CLOSE : '}' ;

// string MUST be non greedy!
STR : ~[}{%]+? ;


fragment PROPERTY
		: (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT)*
		;
fragment TYPE_NAME
		: (LOWER_LETTER | UPPER_LETTER)+
		;
fragment SEPARATOR : ':' ;
fragment UPPER_LETTER : 'A' .. 'Z' ;
fragment LOWER_LETTER : 'a' .. 'z' ;
fragment DIGIT : '0' .. '9' ;

