#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/ioctl.h>

#include <signal.h>
#include <fcntl.h>

#define BUFFER_SIZE 100

int fd;
static void singio_func(int num)
{
    int err;
    unsigned int keyvalue = 0;
    err = read(fd,&keyvalue,sizeof(keyvalue));    

    if (err < 0)
    {
        printf("read error \n");
    }else {
        printf("signal read! value = %d\n",keyvalue);
    }
}

int main(int argc, char *argv[])
{
    char *filename = argv[1];
    fd = open(filename, O_RDWR);
    int cmd,period;
    int flags = 0;
    char readbuf[BUFFER_SIZE], writebuf[BUFFER_SIZE];

    if (fd < 0)
    {
        printf("open filename %s failed \n", filename);
        return -1;
    }

    //設定 signal 處理函數

    signal(SIGIO, singio_func);

    //設定當前 process 接收 singal
    fcntl(fd,F_SETOWN,getpid());
    flags = fcntl(fd,F_GETFL);
    fcntl(fd,F_SETFL,flags | FASYNC);

    while (1) { sleep(2); }
    

    close(fd);
    return 0;
}