#include <stdio.h>
#include <pthread.h>
#include <unistd.h>

int main(void)
{
  int pid;
  int fd[2];
  int ret;
  int process_inter = 0;
  ret = pipe(fd);
  if(ret < 0){
    printf("create pipe failed!\r\n");
    return 1;
  }
  pid = fork();
  char str[] = "Hello";
  if (pid < 0)
  {
    printf("Fork Failed");
    exit(-1);
  }
  // child process
  else if (pid == 0)
  {
    read(fd[0],&process_inter,1);
    //printf("process_inter:%d\r\n",process_inter);
    while(process_inter == 0);

    for (int i = 0; i < 10; i++)
    {
      printf("child : %d \r\n", i);
    }
    //execlp("/bin/ls", "ls", NULL);
  }
  // parent process
  else
  {
    for (int i = 0; i < 10; i++)
    {
      printf("parent : %d \r\n", i);
      usleep(100);
    }
    process_inter = 1;

    write(fd[1],&process_inter,1);
    //process_inter = 1;
    wait(NULL);

    exit(0);
  }
  close(fd[0]);
  close(fd[1]);
  return 0;
}