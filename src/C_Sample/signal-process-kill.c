#include <stdio.h>
#include <stdlib.h>
#include <signal.h>
#include <sys/types.h>

int main(int argc, char *argv[]){
  int pid = atoi(argv[1]);
  int sign = atoi(argv[2]);

  printf("pid = %d,sign = %d \r\n",pid,sign);
  kill(pid,sign);
  return 0;
}


//raise = kill(getpid(),Signal)
// int main() {
//     printf("before raise\r\n");
//     raise(9);
//     printf("after raise\r\n");
//     return 0;
// }