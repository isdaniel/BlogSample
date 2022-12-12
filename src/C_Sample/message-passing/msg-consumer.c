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
    char quit[124] = "quit";
    int dataLen;
    key_t key = ftok("./key",0);
    int msgid = msgget(key, 0666 | IPC_CREAT);
    struct msgModel recvbuf;
    printf("begin received data!\r\n");

    do
    {
        memset(recvbuf.buffer,0,sizeof(recvbuf.buffer));
        dataLen = msgrcv(msgid,(void*)&recvbuf,sizeof(recvbuf.buffer),100,0);
        printf("dataLen = %d, data = %s\r\n",dataLen - 1,recvbuf.buffer);
    } while(strncmp(recvbuf.buffer, quit, 4) != 0);
    printf("release queue!!\r\n");
    //del message queue.
    msgctl(msgid,IPC_RMID,NULL);

    return 0;
}