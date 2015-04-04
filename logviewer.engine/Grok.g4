// Created by: egr
// Created at: 02.10.2014
// Grok templating syntax
// © 2012-2015 Alexander Egorov

grammar Grok;
options { tokenVocab=GrokLexer; }

parse
	: (literal? grok)* literal? 
	;

literal
	: SKIP # OnLiteral
	;

grok
	: OPEN definition CLOSE
	;

definition
	: PATTERN semantic?  # OnDefinition
	;

semantic
	: COLON PROPERTY casting? # OnSemantic
	;

casting
	: COMMA (TYPE_NAME | cast (COMMA cast)*) # OnCasting
	;

cast
	: QUOTED_STR ARROW target # OnCastingCustomRule
	;

target
	: TYPE_NAME (DOT TYPE_MEMBER)*
	;
