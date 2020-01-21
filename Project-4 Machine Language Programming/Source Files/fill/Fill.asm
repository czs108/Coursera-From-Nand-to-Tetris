// This file is part of www.nand2tetris.org
// and the book "The Elements of Computing Systems"
// by Nisan and Schocken, MIT Press.
// File name: projects/04/Fill.asm

// Runs an infinite loop that listens to the keyboard input.
// When a key is pressed (any key), the program blackens the screen,
// i.e. writes "black" in every pixel;
// the screen should remain fully black as long as the key is pressed. 
// When no key is pressed, the program clears the screen, i.e. writes
// "white" in every pixel;
// the screen should remain fully clear as long as no key is pressed.

// Put your code here.
    // R0 = the address of the first white pixel
    @SCREEN
    D = A
    @R0
    M = D

    // R1 = the end address of screen memory
    @R0
    D = M
    @R1
    M = D
    @8192
    D = A
    @R1
    M = M + D

(CHECK)
    // Check the keyboard
    @KBD
    D = M
    // A key is pressed
    @FILL
    D; JNE
    // No key is pressed
    @CLEAR
    0; JMP

(FILL)
    // Check if all pixels are black
    @R1
    D = M
    @R0
    D = D - M
    @CHECK
    D; JLT

    // Move the pointer from the beginning to the end
    // Set the next pixel to black
    @R0
    D = M
    A = M
    M = -1
    @R0
    M = D + 1

    @CHECK
    0; JMP

(CLEAR)
    // Check if all pixels are white
    @SCREEN
    D = A
    @R0
    D = M - D
    @CHECK
    D; JEQ

    // Move the pointer from the end to the beginning
    // Set the next pixel to white
    @R0
    M = M - 1
    @R0
    A = M
    M = 0

    @CHECK
    0; JMP