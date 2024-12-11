#include <stdio.h>
#include <sys/mman.h>
#include <unistd.h>
#include <memory.h>

int add(int a, int b) {    
    return a + b;
}

int asm_add(int a, int b) {
    // Machine code for the function:
    // mov eax, edi      (0x89, 0xf8)
    // add eax, esi      (0x01, 0xf0)
    // ret               (0xc3)

    /*
    First argument: EDI for 32-bit arguments
    Second argument: ESI for 32-bit arguments
    */
   
    unsigned char code[] = {
        0x89, 0xf8,  // mov eax, edi
        0x01, 0xf0,  // add eax, esi
        0xc3         // ret
    };

    // Allocate memory that is writable and executable
    void *temp = mmap(
        NULL,
        getpagesize(),
        PROT_READ | PROT_WRITE | PROT_EXEC,
        MAP_ANONYMOUS | MAP_PRIVATE,
        -1,
        0
    );

    if (temp == MAP_FAILED) {
        perror("mmap failed");
        return -1;
    }

    // Copy the machine code to the allocated memory
    memcpy(temp, code, sizeof(code));

    // Cast the memory to a function pointer that matches the signature
    typedef int (*p_fun)(int, int);
    p_fun add_func = (p_fun)temp;

    // Call the generated function
    int result = add_func(a, b);

    // Free the allocated memory
    munmap(temp, getpagesize());

    return result;
}

int main() {
    int obj = add(10,5);
    int sum = asm_add(55,5);
     
    printf("%d\n", obj);
    printf("%d\n", sum);

    return 0;
}

