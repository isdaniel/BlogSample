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
    mov ax, 8         ; Load 10 into ax
    mov [a], ax        ; Store the value of ax into `a`

    ; Compare `a` with 10
    mov ax, [a]        ; Load the value of `a` into ax
    cmp ax, 10         ; Compare ax with 10
    jl skip_assignment  ; Jump if ax < 10 (less than)

    ; If a >= 10, set `a` to 100
    mov ax, 100        ; Load 100 into ax
    mov [a], ax        ; Store the value of ax into `a`
skip_assignment:
    jmp $               ; Infinite loop
    ; Exit program
    mov ax, 4C00h      ; DOS function to terminate program
    int 21h            ; DOS interrupt
MAIN ENDP
END MAIN