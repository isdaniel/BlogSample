#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>

int main(void) {
    int ret;
    ret = mkfifo("./sample_fifo",0777);
    if(ret < 0){
      return -1;
    }

    printf("create fifo sucess\r\n");
    return 0;
}