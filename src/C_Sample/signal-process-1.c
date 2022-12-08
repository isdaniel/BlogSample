#include <stdio.h>
#include <stdlib.h>
#include <string.h>

void myfunc(int signum){
    for (int i = 0; i < 5; i++)
    {
        printf("myfunc releaseZombie! %d\r\n",i);
        sleep(1);
    }
    return;
}

void releaseZombie(int signum){
    wait(NULL);
    return;
}

int main(int argc, char *argv[]) {
    int pid = fork();

    if(pid > 0){
        int i = 0;
        signal(10,myfunc);
        signal(17,releaseZombie);
        while(1){
            printf("parent Process Complete! %d\r\n",i);
            sleep(1);
            i++;
        }
    }

    if (pid == 0){
        printf("create child process\r\n");
        sleep(10);
        kill(getppid(),10);
        exit(0);
    } 

    return 0;
}