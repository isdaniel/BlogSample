#include <stdio.h>
#include <stdlib.h>
#include "foo.h"

int main(void) {
    int arr[] = {1,8,2,3,4};
    int max = find_max(arr);
    printf("Hello World !! max value is %d\r\n",max);
    return 0;
}