#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#define N 10

typedef struct 
{
    int id;
    pthread_t tid;
} http_Context;


void* myfunc(void* args){
  http_Context* context = (http_Context*)args;
  
  printf("in myfunc : %d, thread info %ld\r\n",context->id,context->tid);

  return NULL;
}

int main(void) {
    http_Context* context_ptr = malloc(N * sizeof(http_Context));
    for (size_t i = 0; i < N; i++)
    {
        (context_ptr + i)->id = i;
        int ret = pthread_create(&((context_ptr + i)->tid),NULL,myfunc,(context_ptr + i));
        if (ret != 0) {
            fprintf(stderr, "Error creating thread: %d\n", ret);
        }
    }

    for (size_t i = 0; i < N; i++)
    {
        pthread_join((context_ptr + i)->tid,NULL);
    }
    
    free(context_ptr);
    return 0;
}