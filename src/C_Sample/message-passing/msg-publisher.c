#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>
#include <signal.h>
#include <string.h>

struct msgModel
{
    long type;
    char buffer[124];
    char ID[4];
};


int main()
{
    int msgid = msgget(IPC_PRIVATE,0777);
    struct msgModel sendbuf;
    if (msgid < 0)
    {
        printf("create message queue failed\r\n");
        return -1;
    }
    sendbuf.type = 100;
    printf("please input message:\r\n");
    while (1)
    {
        fgets(sendbuf.buffer,sizeof(sendbuf.buffer),stdin);
        msgsnd(msgid,(void*)&sendbuf,strlen(sendbuf.buffer),0);
    }
    
    return 0;
}