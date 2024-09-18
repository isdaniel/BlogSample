#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <dlfcn.h>
#ifdef DEBUG
#include <unistd.h>
#endif



#ifdef DEBUG
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

void _free(void* ptr,const char* filename,int line,const char* datetime){
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

// dlsym, dlopen, second way hock
typedef void* (*malloc_t)(size_t size);
typedef void (*free_t)(void* ptr);

malloc_t malloc_f = NULL;
free_t free_f = NULL;

void init_hook(void){
    if(malloc_f == NULL){
        malloc_f = (malloc_t)dlsym(RTLD_NEXT,"malloc");
    }

    if(free_f == NULL){
        free_f = (free_t)dlsym(RTLD_NEXT,"free");
    }
}

bool enable_malloc_hook = 1;
bool enable_free_hook = 1;

void* malloc(size_t size){
    void* ptr = NULL;
    if(enable_malloc_hook){
        enable_malloc_hook = false;
        ptr = malloc_f(size);
        char buffer[64] = {0};
        sprintf(buffer,"./leak-checker/%p.mem",ptr);
        FILE *fp = fopen(buffer,"w");

        //0 previous caller  (SP register)
        //get exact address by addr2line -f -e memoryleak -a {caller address}
        //gcc -rdynamic -no-pie  -g -o memoryleak memoryleak.c -ldl
        fprintf(fp,"[%s]_malloc [%p]\n",__TIMESTAMP__, __builtin_return_address(0));
        fclose(fp);
        enable_malloc_hook = true;
    } else {
        ptr = malloc_f(size);
    }

    return ptr;
}

void free(void* ptr){
    if(enable_free_hook){
        enable_free_hook = 0;
        char buffer[64] = {0};
        sprintf(buffer,"./leak-checker/%p.mem",ptr);
        printf("filename %s: %d\r\n",buffer,unlink(buffer));
        // if(unlink(buffer) < 0){
        //     //double free
        // }
        free_f(ptr);
        enable_free_hook = 1;
    } else {
        free_f(ptr);
    }
}


int main(void) {
    init_hook();
    int* p1 = (int*)malloc(1);
    int* p2 = (int*)malloc(2);
    int* p3 = (int*)malloc(sizeof(int));
    //*p3 = 123;
    //printf("%d\r\n",*p3);
    free(p1);
    free(p2);
    return 0;
}