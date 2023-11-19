#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>

int main(void) {
    int fd;
    fd = open("./pipe2",O_RDONLY);
    int process_inter=0;
    if(fd < 0){
      printf("open failed \r\n");
      return -1;
    }
    read(fd, &process_inter, sizeof(int));
    printf("process_inter = %d\r\n",process_inter);
    return 0;
}