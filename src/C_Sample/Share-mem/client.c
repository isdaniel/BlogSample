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
    return;
}
int main()
{
    key_t key = ftok("./ps1",1);
    int shmid = shmget(key, sizeof(mybuf), 0666 | IPC_CREAT); // 獲取一個新的共享記憶 體段，大小為 512 bytes
    int server_pid;
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
        printf("client process:shmat function failed!!\r\n");
        return -1;
    }    
    signal(SIGUSR1,myfunc);
    // sent pid to server process 
    server_pid = p->pid;
    printf("shmid = %d , client get server pid = %d , send client pid = %d \r\n",shmid,server_pid,getpid());
    p->pid = getpid();
    //notify server
    kill(server_pid,SIGUSR1);
    //pause();

    while (1)
    {
        //wait for server proecss res.
        pause();

        printf("client process received data = %s\r\n",p->.buf);

        //notify child process read data.
        kill(server_pid,SIGUSR1);
    }
}