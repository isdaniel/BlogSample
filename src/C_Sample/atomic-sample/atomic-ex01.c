#include <stdio.h>
#include <stdlib.h>
#include <stdatomic.h>

int main(void) {
    atomic_int value = ATOMIC_VAR_INIT(0);
    printf("Original value: %d\n", atomic_load(&value));
    atomic_fetch_add(&value, 10);
    printf("Incremented value : %d\n", atomic_load(&value));
    
    return 0;
}