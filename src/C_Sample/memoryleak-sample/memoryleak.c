#include <stdio.h>
#include <stdlib.h>
#ifdef DEBUG
#include <unistd.h>
#endif

#ifdef DEBUG
#include <unistd.h>
//write file in system for detecting memory leak
void* _malloc(size_t size,const char* filename,int line,const char* datetime){
    void* ptr = malloc(size);
    //printf("[%s]_malloc [%p]:%s, %d\n",datetime, ptr,filename,line);
    char buffer[64] = {0};
    sprintf(buffer,"./leak-checker/%p.mem",ptr);
    FILE *fp = fopen(buffer,"w");
    fprintf(fp,"[%s]_malloc [%p]:%s, %d\n",datetime, ptr,filename,line);
    //fflust(fp);
    fclose(fp);

    return ptr;
}

void* _free(void* ptr,const char* filename,int line,const char* datetime){
    char buffer[64] = {0};
    sprintf(buffer,"./leak-checker/%p.mem",ptr);
    if(unlink(buffer) < 0){
        //double free
    }
    free(ptr);
    //printf("[%s]_free [%p]:%s, %d\n",datetime, ptr,filename,line);
}

#define malloc(size) _malloc(size,__FILE__,__LINE__,__TIMESTAMP__)
#define free(ptr) _free(ptr,__FILE__,__LINE__,__TIMESTAMP__)

#endif

int main(void) {
    int* p1 = (int*)malloc(1);
    int* p2 = (int*)malloc(2);
    int* p3 = (int*)malloc(sizeof(int));
    *p3 = 123;
    printf("%d\r\n",*p3);
    free(p1);
    free(p2);
    return 0;
}