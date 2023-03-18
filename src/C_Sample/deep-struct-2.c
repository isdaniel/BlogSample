#include <stdio.h>
#include <stdlib.h>
#define N 10


typedef struct {
    int id;
    char* str;
} StructBase;

typedef struct 
{
    StructBase header;
    int val;
} Structchild;


int main(void) {
    printf("Hello \r\n");
    StructBase* base1 = malloc(sizeof(Structchild));
    base1->id = 1000;
    base1->str = "Hello StructBase!!";
    Structchild* child = (Structchild*)base1;
    child->val = 111;
    printf("base1->id : %d,  child->val %d ,  base1->str %s  \r\n",child->header.id,child->val, base1->str);
    return 0;
}