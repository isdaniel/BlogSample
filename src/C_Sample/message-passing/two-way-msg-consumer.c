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

int main(int argc, char *argv[])
{
    key_t key = ftok("./key",0);
    int msgid = msgget(key,0666 | IPC_CREAT);
    long sendId = atol(argv[1]);
    long recvId = atol(argv[2]);
    int dataLen;
    printf("begin received data! sendId =%d , recvId =%d , key = %d \r\n",sendId,recvId,key);
    
    int pid = fork();

    //child process
    if(pid == 0){
        struct msgModel recvbuf;
        do
        {
            memset(recvbuf.buffer,0,sizeof(recvbuf.buffer));
            dataLen = msgrcv(msgid,(void*)&recvbuf,sizeof(recvbuf.buffer),(int)recvId,0);
            printf("dataLen = %d, data = %s\r\n",dataLen - 1,recvbuf.buffer);
        } while(strncmp(recvbuf.buffer, "quit", 4) != 0);
    }

    //parent process
    if (pid > 0)
    {
        struct msgModel sendbuf;
        sendbuf.type = sendId;
        while (1)
        {
            fgets(sendbuf.buffer,sizeof(sendbuf.buffer),stdin);
            msgsnd(msgid,(void*)&sendbuf,strlen(sendbuf.buffer),0);
            printf("send message %s\r\n",sendbuf.buffer);
        }
    }
    

    printf("release queue!!\r\n");
    //del message queue.
    msgctl(msgid,IPC_RMID,NULL);

    return 0;
}