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
	: PATTERN semantic?  # OnDefinition
	;
	
semantic
	: property          # OnProperty
	| propertyWithCast  # OnPropertyWithCast
	;
	
property
	: PROPERTY_REFERENCE
	;

propertyWithCast
	: PROPERTY_REFERENCE_WITH_CAST
	;

PATTERN 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | '_')* 
	;

PROPERTY_REFERENCE
	: SEPARATOR PROPERTY
	;
	
PROPERTY_REFERENCE_WITH_CAST
	: PROPERTY_REFERENCE SEPARATOR TYPE_NAME
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

