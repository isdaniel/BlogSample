; section .data           ; Data section
;     ;msg db "Hello, world!", 0 ; Null-terminated string

; section .text           ; Code section
;     global _start       ; Entry point for the program
.MODEL SMALL
.STACK 100H
.DATA
    a dw 0              ; Declare a word variable `a`
.CODE
MAIN PROC
    mov [a], 10 
    xor ax, ax
loop_start:
    cmp ax,10
    jge loop_end

    mov cx,[a]
    add cx, 1
    mov [a],cx
    inc ax
    jmp loop_start

loop_end: 
    ; Exit program
    mov ax, 4C00h      ; DOS function to terminate program
    int 21h            ; DOS interrupt
MAIN ENDP
END MAIN