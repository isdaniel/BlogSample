#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

int main(void) {
    int fd[2];
    int ret;
    char writebuf[] = "Hello process";
    char readbuf[128]={0};
    ret = pipe(fd);

    if(ret<0){
      printf("create pipe failed!\r\n");
      return 1;
    }
    printf("create pipe sucess fd[0] = %d, fd[1] = %d\r\n",fd[0],fd[1]);


    write(fd[1],writebuf,sizeof(writebuf));
    read(fd[0],readbuf,sizeof(writebuf));
    //read block
    //read(fd[1],readbuf,128);
    printf("result = %s \r\n",readbuf);

    close(fd[0]);
    close(fd[1]);
    return 1;
}