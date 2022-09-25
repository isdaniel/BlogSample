#include <stdio.h>
#include <stdlib.h>
#include "ND/Node.h"
#define MAXIMUM_ALIGNOF 8
#define TYPEALIGN(ALIGNVAL,LEN)  \
	(((uintptr_t) (LEN) + ((ALIGNVAL) - 1)) & ~((uintptr_t) ((ALIGNVAL) - 1)))

// #define SHORTALIGN(LEN)			TYPEALIGN(ALIGNOF_SHORT, (LEN))
// #define INTALIGN(LEN)			TYPEALIGN(ALIGNOF_INT, (LEN))
// #define LONGALIGN(LEN)			TYPEALIGN(ALIGNOF_LONG, (LEN))
// #define DOUBLEALIGN(LEN)		TYPEALIGN(ALIGNOF_DOUBLE, (LEN))
#define MAXALIGN(LEN)			TYPEALIGN(MAXIMUM_ALIGNOF, (LEN))


#define BTP_LEAF		(1 << 0)	/* leaf page, i.e. not internal page */
#define BTP_ROOT		(1 << 1)	/* root page (has no parent) */
#define BTP_DELETED		(1 << 2)	/* page has been deleted from tree */
#define BTP_META		(1 << 3)	/* meta-page */
#define BTP_HALF_DEAD	(1 << 4)	/* empty, but still in tree */
#define BTP_SPLIT_END	(1 << 5)	/* rightmost page of split group */
#define BTP_HAS_GARBAGE (1 << 6)	/* page has LP_DEAD tuples */
#define BTP_INCOMPLETE_SPLIT (1 << 7)	/* right sibling's downlink is missing */

#define P_ISLEAF(node)		(((node)->flags & BTP_LEAF) != 0)

// #define UTILS_H 100

// #ifndef UTILS_H
// #define UTILS_H 1 
// #endif  

int main(void) {


    //printf("%d  %d",MAXALIGN(sizeof(Node)),sizeof(pNode));
    // int src = 1;
    // int "dst;   
    // printf("%d\r\n",UTILS_H);
    pNode ptr = malloc(sizeof(Node));
    ptr->flags = 1;

    printf("flags : %d\r\n",P_ISLEAF(ptr));
    // asm ("mov %1, %0\n\t"
    //     "add $1, %0"
    //     : "=r" (dst) 
    //     : "r" (src));

    // printf("%d\n", dst);
    // Size s1 = 100;
    // printf("%d",s1);
    // int val;
    // pNode head = malloc(sizeof(Node));
    // pNode nptr = head;
    
    // while (val != -1)
    // {
    //     scanf("%d", &val);
    //     nptr->val = val;
    //     nptr->next = malloc(sizeof(Node));
    //     nptr = nptr->next;
    // }

    // nptr->next = NULL;

    // while(head->next != NULL){
    //     printf("this pos %x, next pos %x, node val %d\r\n",head ,head->next,head->val);
    //     head = head->next;
    // }
    
    // int** ptr;
    // int* a = 0;
    // int b = 100;
    // a = &b;
    // ptr = &a;
    // *a = 10;
    // printf("%d \r\n",b);
    // **ptr = 100;
    // printf("%d \r\n",b);
    return 0;
}