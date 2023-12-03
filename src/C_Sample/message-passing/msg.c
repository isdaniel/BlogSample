#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>
#include <signal.h>
int main()
{
    int msgid = msgget(IPC_PRIVATE,0777);

    if (msgid < 0)
    {
        printf("create message queue failed\r\n");
        return -1;
    }
    
    printf("create message queue Ok, msgid = %d \r\n",msgid);
    system("ipcs -q");

    msgctl(msgid,IPC_RMID,NULL);
    system("ipcs -q");
    return 0;
}