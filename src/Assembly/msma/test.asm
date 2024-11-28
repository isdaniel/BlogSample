; section .data           ; Data section
;     ;msg db "Hello, world!", 0 ; Null-terminated string

; section .text           ; Code section
;     global _start       ; Entry point for the program

.MODEL SMALL
.STACK 100H
.DATA
.CODE
MAIN PROC
    xor bx, bx
    mov bl, 255
    add bx, 1
    nop
    nop
    ; ; Print the message using Linux syscalls
    ; mov eax, 4          ; Syscall number for write
    ; mov ebx, 1          ; File descriptor 1 (stdout)
    ; mov ecx, msg        ; Address of the message
    ; mov edx, 13         ; Length of the message
    ; int 0x80            ; Invoke the syscall

    ; ; Exit the program
    ; xor eax, eax        ; Clear EAX
    ; mov al, 1           ; Syscall number for exit
    ; xor ebx, ebx        ; Exit code 0
    ; int 0x80            ; Invoke the syscall
MAIN ENDP
END MAIN