#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>

int main(void) {
    int fd;
    fd = open("./pipe1",O_WRONLY);
    int process_inter=0;
    if(fd < 0){
      printf("open failed \r\n");
      return -1;
    }
      printf("open pipe file success \r\n");
    for (int i = 0; i < 5; i++)
    {
      printf("this is first process i = %d \r\n",i);
      usleep(100);
    }
    process_inter = 1;
    write(fd,&process_inter,1 );
    return 0;
}