#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <fcntl.h>
#include <unistd.h>
#include <string.h>

char* concatStr(char* str1,char* str2){
    int len = strlen(str1) + strlen(str2);
    char* str = (char*)malloc((len+1)*sizeof(char));
    strcat(str, str1);
    strcat(str, str2);
    return str;
}

int main(int argc, char *argv[])
{
  int process_inter = 1;
  char* p_read = concatStr("./",argv[1]);
  char* p_write = concatStr("./",argv[2]);

  int fd_write = open(p_write, O_WRONLY);
  write(fd_write, &process_inter, sizeof(int));
  printf("wirte fin!\r\n");
  int fd_read = open(p_read , O_RDONLY);
  printf("read: %s, write %s \r\n",p_read,p_write);
  int i = 0;

  do
  {
    printf("pid = %d , i = %d \r\n", getpid(),i);
    i++;
    sleep(1);
    write(fd_write, &process_inter, sizeof(int));
  } while (read(fd_read, &process_inter, sizeof(int)) > 0);
  

  return 0;
}