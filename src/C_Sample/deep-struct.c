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
    http_Context context_array[N];
    for (size_t i = 0; i < N; i++)
    {
        context_array[i].id = i;
        pthread_create(&context_array[i].tid,NULL,myfunc,&context_array[i]);
    }

    for (size_t i = 0; i < N; i++)
    {
        pthread_join(context_array[i].tid,NULL);
    }
    


    return 0;
}