( Programming language: GNU Forth/gforth in Ubuntu on WSL for Windows 10, but probably works in other vt100/xterm and OS. )
( It depends on font in terminal: teletext2 or teletext4 from https://github.com/peterkvt80/Muttlee/tree/master/public/assets . )
( Paste this in running gforth console. )

decimal

( To print answer: DECXCPR type )
: DECXCPR ( -- addr count )
  \ BEGIN  stdin key?-file
  \ WHILE  stdin key-file drop
  \ REPEAT
  ESC[ ." 6n"
  9 0 DO
  stdin key-file dup 
  pad i + c!
  [char] R =
  IF pad 2 + i 1- LEAVE THEN
  LOOP ;

: str2coords ( addr count -- r1 c1 )
  drop 0. rot 5 >number drop 1+ 0. rot 5 >number 2drop drop nip ;

( screensize is similar to form in gforth. )
: screensize ( -- r c )
  999 999 at-xy DECXCPR str2coords ;

: gemit ( gch -- )
  60960 + dup 60992 >
  IF     32 +
  THEN
  xemit ;

: semigraphics3x2 ( -- )
  64 0
  DO     i gemit 61103 xemit
  LOOP
  ;

: cur ( r c -- )
  swap at-xy ;

: string
  Create allot does> + ;

( In future _cols and _rows will be set using DECXCPR. )
variable _cols
40 _cols !
variable _rows
24 _rows !
_rows @ _cols @ * string screen

: gpage ( -- )
  _rows @ _cols @ * 0
  DO     0 i screen c!
  LOOP
  ;

: grefresh ( -- )
  _rows @ _cols @ * 0
  DO     i screen c@ ?dup
         IF     i _cols @ /mod at-xy gemit
         THEN
  LOOP
  0 0 at-xy ;

: dotcommon1 ( y x -- dx dy r c )
  2 /mod rot 3 /mod ( dx c dy r )
  rot ( dx dy r c ) ;

: dotcommon2 ( dx dy r c -- r c gch )
  2swap ( r c dx dy )
  2* + 1 swap lshift ( r c gch ) ;

: dotindex ( r c -- index )
  swap _cols @ * + ( index ) ;

: dotcommon4 ( gch pos -- pos gch old )
  tuck ( pos gch pos )
  screen c@ ( pos gch old ) ;

: dotcommon5 ( pos new -- )
  tuck gemit ( new pos )
  screen c! ( ) ;

: setdot ( y x -- )
  dotcommon1
  2dup cur
  dotcommon2
  -rot ( gch r c )
  dotindex
  dotcommon4
  or ( pos new )
  dotcommon5 ;

: clrdot ( y x -- )
  dotcommon1
  2dup cur
  dotcommon2
  invert ( r c igch )
  -rot ( igch r c )
  dotindex
  dotcommon4
  and ( pos new )
  dotcommon5 ;

: dot ( y x -- f )
  dotcommon1
  dotcommon2
  -rot ( gch r c )
  dotindex
  screen c@ ( gch old )
  and 0> ( f ) ;

( Examples )
variable iR
variable iK
: exgramod ( -- ) \ Translated from ABC 80 BASIC "Bruksanvisning ABC80" p. 30.
  58 5
  DO     41 i 2 / - iR ! 
         36 i 2 / - iK ! 
         i 1+ 1
         DO     iR @ i + iK @ setdot 
                iR @ j + iK @ i + setdot 
                iR @ j + i - iK @ j + setdot 
                iR @ iK @ j + i - setdot
         LOOP
         4
  +LOOP
  ;
\ Original
\ 10 I = 1 TO 24:;:;CHR¤(151);:NEXT I
\ 20 FOR J = 5 TO 57 STEP 4
\ 30 R = 41-J/2
\ 40 K = 36-J/2
\ 50 FOR I = 1 TO J
\ 60 SETDOT R+I,K
\ 70 SETDOT R+J,K+I
\ 80 SETDOT R+J-I,K+J
\ 90 SETDOT R,K+J-I
\ 100 NEXT I
\ 110 NEXT J

: invscreen
  3 _rows @ * 0
  DO     2 _cols @ * 0
         DO     j i dot
                IF     j i clrdot
                ELSE   j i setdot
                THEN
         LOOP

  LOOP
  0 0 cur ;
  
gpage page exgramod 0 0 cur invscreen invscreen
gpage page
15 4 setdot 16 5 setdot 17 6 setdot 18 7 setdot
16 5 clrdot 18 7 clrdot
page grefresh
15 4 dot . 16 5 dot . 17 6 dot . 18 7 dot .
3 24 cur _rows ? _cols ?
screensize _cols ! _rows !
