#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>
#include <sys/ioctl.h>

#define BUFFER_SIZE 100
#define DEVICE "/dev/dainel_device"

#define OPEN_CMD _IO(0XEF, 1)
#define CLOSE_CMD _IO(0XEF, 2)
#define SET_PERIOD_CMD _IOW(0XEF, 3, int)

int main(int argc, char *argv[])
{
    char *filename = argv[1];
    int fd = open(filename, O_RDWR);
    int cmd,period;
    char readbuf[BUFFER_SIZE], writebuf[BUFFER_SIZE];

    if (fd < 0)
    {
        printf("open filename %s failed \n", filename);
        return -1;
    }

    while (1)
    {
        printf("please type your command: ");
        scanf("%d", &cmd);
        if (cmd == 1)
        {
            ioctl(fd,OPEN_CMD);
        }
        else if (cmd == 2)
        {
            ioctl(fd,CLOSE_CMD);
        }
        else if (cmd == 3)
        {
            printf("please write your expect change time period: ");
            scanf("%d",&period);
            ioctl(fd,SET_PERIOD_CMD,&period);
        }
    }

    printf("finished process!! \r\n");

    close(fd);
    return 0;
}