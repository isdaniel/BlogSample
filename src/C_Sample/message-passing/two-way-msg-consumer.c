#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <string.h>
#include <sys/msg.h>
#include <unistd.h>

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
    struct msgModel sendbuf,recvbuf;
    long sendId = atol(argv[1]);
    long recvId = atol(argv[2]);
    int dataLen;
    printf("begin received data! sendId =%ld ,msgid = %d, recvId =%ld , key = %d \r\n",sendId,msgid,recvId,key);
    sendbuf.type = sendId;
    int pid = fork();

    //child process
    if(pid == 0){
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
        while (1)
        {
            //sleep(1000);
            memset(sendbuf.buffer,0,sizeof(sendbuf.buffer));
            printf("please input message:\r\n");
            fgets(sendbuf.buffer,sizeof(sendbuf.buffer),stdin);
            msgsnd(msgid,(void*)&sendbuf,strlen(sendbuf.buffer),0);
        }
    }
    

    printf("release queue!!\r\n");
    //del message queue.
    msgctl(msgid,IPC_RMID,NULL);

    return 0;
}