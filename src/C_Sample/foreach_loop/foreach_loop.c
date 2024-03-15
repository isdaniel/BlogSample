#include <stdio.h>
#include <stdlib.h>
#include <time.h>   

#define lfirst(lc)				((lc)->ptr_value)
#define lfirst_node(type,lc)	castNode(type, lfirst(lc))
#define castNode(_type_, nodeptr) ((_type_ *) (nodeptr))
#define true	1
#define false	0

#define foreach(cell, lst)	\
	for (ForEachState cell##__state = {(lst), 0}; \
		 (cell##__state.l != NIL && \
		  cell##__state.i < cell##__state.l->length) ? \
		 (cell = &cell##__state.l->elements[cell##__state.i], true) : \
		 (cell = NULL, false); \
		 cell##__state.i++)

#define NIL						((List *) NULL)

typedef union ListCell
{
	void	   *ptr_value;
} ListCell;

typedef struct List
{
	int			length;			/* number of elements currently present */
	ListCell   *elements;		/* re-allocatable array of cells */
	/* We may allocate some cells along with the List header: */
} List;


/*
 * State structs for various looping macros below.
 */
typedef struct ForEachState
{
	const List *l;				/* list we're looping through */
	int			i;				/* current element index */
} ForEachState;

typedef struct Student{
    int age;
    char* name;
} Student;

List* InitialStudents(){
    List *list = (List *)malloc(sizeof(List));
    char* names[] = {"Alice", "Bob", "Charlie", "David", "Emma"};
    int length = sizeof(names) / sizeof(names[0]); 
    list->elements = (ListCell *)malloc(length * sizeof(ListCell));
    list->length = length; // Set the length accordingly
    
    for (int i = 0; i < length; i++) {
        Student *stu = (Student *)malloc(sizeof(Student));
        stu->age = rand() % 10 + 18; 
        char* names[] = {"Alice", "Bob", "Charlie", "David", "Emma"};
        int name_count = sizeof(names) / sizeof(names[0]);
        stu->name = names[i]; 
        list->elements[i].ptr_value = stu;
    }
    return list;
}

int main(void) {
    srand( time(NULL) );    
    ListCell   *item;
    List *list = InitialStudents();

    foreach(item, list) {
        Student *stu = lfirst_node(Student, item);
        printf("student name: %s, age: %d\n", stu->name,stu->age);
    }

    // Free allocated memory
    free(list->elements->ptr_value);
    free(list->elements);
    free(list);
    return 0;
}