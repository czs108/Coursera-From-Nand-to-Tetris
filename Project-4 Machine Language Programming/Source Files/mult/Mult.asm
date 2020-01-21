// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Mult.asm

// Multiplies R0 and R1 and stores the result in R2.
// (R0, R1, R2 refer to RAM[0], RAM[1], and RAM[2], respectively.)

// Put your code here.
    // R2 = 0
    @R2
    M = 0

    // i = 0
    @i
    M = 0

(LOOP)
    // If (i == R1)
    @i
    D = M
    @R1
    D = M - D
    @END
    D; JEQ

    // R2 += R0
    @R0
    D = M
    @R2
    M = M + D

    // i++
    @i
    M = M + 1

    @LOOP
    0; JMP

(END)
    @END
    0; JMP