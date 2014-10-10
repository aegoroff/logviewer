// Created by: egr
// Created at: 02.10.2014
// Grok templating syntax
// © 2012-2014 Alexander Egorov

grammar Grok;

@lexer::members {
	public bool inPattern;
	public bool inSemantic;
	public void InPattern() { inPattern = true; }
	public void OutPattern() { inPattern = false; }
	public void InSemantic() { inSemantic = true; }
	public void OutSemantic() { inSemantic = false; }
}

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
	: SEMANTIC casting? # OnSemantic
	;

casting
	: COMMA (TYPE_NAME | cast (COMMA cast)*)
	;

cast
	: QUOTED_STR ARROW target
	;

target
	: TYPE_NAME (DOT TYPE_NAME)*
	;

PATTERN 
	: UPPER_LETTER (UPPER_LETTER | DIGIT | UNDERSCORE)* {!inSemantic}?
	;

SEMANTIC
	: COLON PROPERTY  { InSemantic(); }
	;

OPEN : PERCENT OPEN_BRACE { InPattern(); };
CLOSE : CLOSE_BRACE { OutPattern(); OutSemantic(); };


SKIP : (~[}{%])+ {!inPattern}?;

QUOTED_STR : SHORT_STRING ;

TYPE_NAME
	: (LOWER_LETTER | UPPER_LETTER)+ {inSemantic}?
	;

COMMA : ',' ;
DOT : '.' ;
ARROW : '->' ;

fragment COLON : ':' ;
fragment UNDERSCORE : '_' ;

PROPERTY
	: (LOWER_LETTER | UPPER_LETTER) (LOWER_LETTER | UPPER_LETTER | DIGIT)* {!inSemantic}?
	;

fragment UPPER_LETTER : 'A' .. 'Z' ;
fragment LOWER_LETTER : 'a' .. 'z' ;
fragment DIGIT : '0' .. '9' ;
fragment PERCENT : '%' ;
fragment OPEN_BRACE : '{' ;
fragment CLOSE_BRACE : '}';

/// shortstring     ::=  "'" shortstringitem* "'" | '"' shortstringitem* '"'
/// shortstringitem ::=  shortstringchar | stringescapeseq
/// shortstringchar ::=  <any source character except "\" or newline or the quote>
fragment SHORT_STRING
 : '\'' ( STRING_ESCAPE_SEQ | ~[\\\r\n'] )* '\''
 | '"' ( STRING_ESCAPE_SEQ | ~[\\\r\n"] )* '"'
 ;

/// stringescapeseq ::=  "\" <any source character>
fragment STRING_ESCAPE_SEQ
 : '\\' .
 ;
