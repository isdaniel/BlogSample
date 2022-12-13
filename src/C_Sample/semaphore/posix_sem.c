#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <string.h>
#include <sys/msg.h>
#include <unistd.h>
#include <semaphore.h>

sem_t sem;
void* func(void* var){
    //p wait
    sem_wait(&sem);
    for (int i = 0; i < 10; i++)
    {
        usleep(100);
        printf("%s this is fun i=%d\r\n",var,i);
    }
}

int main(int argc, char *argv[])
{
    pthread_t tid;
    sem_init(&sem,0,0);
    int ret = pthread_create(&tid,NULL,func,(void*)"hello world\r");
    if (ret < 0)
    {
        printf("create thread failed\r\n");
        return -1;
    }

    for (int i = 0; i < 10; i++)
    {
        usleep(100);
        printf("this is main i=%d\r\n",i);
    }
    //v relase
    sem_post(&sem);

    while (1);
    
    return 0;
}