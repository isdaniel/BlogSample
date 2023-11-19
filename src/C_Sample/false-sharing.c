#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#define MAX_SIZE 10000000

typedef struct {
  int first;
  int last;
  int id;
} MY_ARGS;

int* arr;
int results[2];

void* myfunc(void* args){
  MY_ARGS* my_args = (MY_ARGS*) args;
  //int i;
  for (size_t i = 0; i < my_args->last; i++)
  {
    results[my_args->id] = results[my_args->id]+arr[i];
  }
  
  return NULL;
}

int main(void) {
    arr = malloc(sizeof(int) * MAX_SIZE);
    for (size_t i = 0; i < MAX_SIZE; i++)
    {
      arr[i] = rand() % 5;
    }
    results[0] = 0;
    results[0] = 1;

    pthread_t th1,th2;
    
    int mid = MAX_SIZE /2;
    MY_ARGS args1 = {0,mid,0};
    MY_ARGS args2 = {mid,MAX_SIZE,1};

    pthread_create(&th1,NULL,myfunc,&args1);
    pthread_create(&th2,NULL,myfunc,&args2);

    pthread_join(th1,NULL);
    pthread_join(th1,NULL);

    printf("s1 = %d\n",results[0]);
    printf("s2 = %d\n",results[1]);
    printf("s1 + s2 = %d\n",results[0] + results[1]);

    return 0;
}