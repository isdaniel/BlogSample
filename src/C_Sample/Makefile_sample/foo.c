#include "foo.h"
#define NELEMS(x)  (sizeof(x) / sizeof((x)[0]))

int find_max(int arr[]){
    if (NELEMS(arr) <= 0)
    {
        printf("unexpected array!\r\n");
        return -1;
    }
    
    int max_val = arr[0];
    for (int i = 0; i < NELEMS(arr); i++)
    {
        if(arr[i] > max_val){
            max_val = arr[i];
        }
    }
    
    return max_val;
}