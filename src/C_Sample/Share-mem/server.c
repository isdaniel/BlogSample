#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>
#include <signal.h>
typedef struct bufferModel
{
    int pid;
    char buf[128]
} mybuf;

void myfunc(int num){
    printf("signal rewrite....!!\r\n");
    return;
}
int main()
{
    key_t key = ftok("./ps1",1);
    int shmid = shmget(key, sizeof(mybuf), 0666| IPC_CREAT); // 獲取一個新的共享記憶 體段，大小為 512 bytes
    int client_pid;
    mybuf* p;
    if (shmid < 0)
    {
        printf("create share mem failed\r\n");
        return -1;
    }
    printf("create share mem successful\r\n");
    p = (mybuf*) shmat(shmid,NULL,0);
    if (p == NULL)
    {
        printf("server process:shmat function failed!!\r\n");
        return -1;
    }
    signal(SIGUSR1,myfunc);

    p->pid = getpid();
    printf("shmid = %d ,server sent pid = %d \r\n",shmid,p->pid);
    pause();
    client_pid = p->pid;
    printf("server received client pid = %d \r\n",p->pid);
    while (1)
    {
        printf("server process write share mem\r\n");
        fgets(p->buf,sizeof(p->buf),stdin);
        //notify child process read data.
        kill(client_pid,SIGUSR1);
        //wait for child proecss res.
        pause();
    }
}