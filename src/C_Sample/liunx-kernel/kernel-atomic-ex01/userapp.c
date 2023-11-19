#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>

#define BUFFER_SIZE 100
#define DEVICE "/dev/dainel_device"

int main(int argc,char* argv[]){
    char* filename = argv[1];
    int fd = open(filename ,O_RDWR);
    int ret;
    char readbuf[BUFFER_SIZE],writebuf[BUFFER_SIZE]; 

    if(fd < 0){
        printf("open filename %s failed \n",filename);
        return -1;
    }
    printf("enter data:");
    scanf(" %[^\n]",writebuf);
    ret = write(fd,writebuf , sizeof(writebuf));
    if(ret < 0){
        printf("write file %s failed \n",filename);
        return -1;
    }
    
    ret = read(fd, readbuf, sizeof(readbuf));
    if(ret < 0){
        printf("read file %s failed \n",filename);
        return -1;
    }

    printf("read file content:%s\r\n",readbuf);

    close(fd);
    return 0;
}