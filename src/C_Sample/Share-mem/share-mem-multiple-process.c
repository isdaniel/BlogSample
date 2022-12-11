#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>
#include <signal.h>

void myfunc(int num){
    return;
}
int main()
{
    int shmid = shmget(IPC_PRIVATE, 128, 0666 | IPC_CREAT); // 獲取一個新的共享記憶 體段，大小為 512 bytes
    char* p;
    if (shmid < 0)
    {
        printf("create share mem failed\r\n");
        return -1;
    }
    printf("create share mem successful\r\n");

    int pid = fork();

    //parent
    if (pid > 0 )
    {
        p = (char*) shmat(shmid,NULL,0);
        signal(SIGUSR1,myfunc);
        while (1)
        {
            printf("parent process write share mem\r\n");
            fgets(p,128,stdin);
            //notify child process read data.
            kill(pid,SIGUSR1);
            //wait for child proecss res.
            pause();
        }
    }
    if(pid == 0)
    {
        signal(SIGUSR1,myfunc);
        p = (char*) shmat(shmid,NULL,0);
        while (1)
        {
            pause();
            printf("child process received message = %s!!",p);
            kill(getppid(),SIGUSR1);
        }
    }
}