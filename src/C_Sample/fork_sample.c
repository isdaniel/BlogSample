#include <stdio.h>
#include <stdlib.h>


int main(void) {
    int pid;
    pid = fork();
    if(pid < 0){
      printf("Fork Failed");
      exit(-1);
    } else if (pid == 0){
      execlp("/bin/ls","ls",NULL);
    } else{
        printf("Child Process Complete! %d\r\n",pid);
        while(1){
        }
        wait(NULL);

        exit(0);
    }
    return 0;
}