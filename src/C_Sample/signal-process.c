#include <stdio.h>
#include <stdlib.h>
#include <string.h>

void myfunc(int signum){
    for (int i = 0; i < 5; i++)
    {
        printf("signum = %d,cnt = %d\r\n",signum,i);
        sleep(1);
    }
    return;
}

void termfunc(int signum){
    printf("signum = %d, termfunc\r\n",signum);
    return;
}

int main(int argc, char *argv[]) {
    int i =0;
    //signal(intercept singal num, func)
    signal(14,myfunc);
    signal(15,termfunc);
    printf("before alarm\r\n");
    alarm(3);
    printf("after alarm\r\n");

    for (int i = 0; i < 20; i++)
    {
        printf("main process cnt = %d\r\n",i);
        sleep(1);
    }
    //exit(0) kill(getpid(),17)
    return 0;
}