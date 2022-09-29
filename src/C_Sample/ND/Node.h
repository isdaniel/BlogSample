typedef struct vMethods {
    int (*add)(int val1,int valu2);
    int (*sub)(int val1,int valu2);
    int (*multiplied)(int val1,int valu2);
} VMethods;

typedef struct node
{
    struct Node* next;
    long val;
    short flags;	
    const VMethods* methods;
} Node,*pNode;