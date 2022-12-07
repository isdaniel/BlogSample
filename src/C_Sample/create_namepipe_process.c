#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <string.h>

char* concatStr(char* str1,char* str2){
    int len = strlen(str1) + strlen(str2) + 1;
    char* str = (char*)malloc((len+1)*sizeof(char));
    strcat(str, str1);
    strcat(str, str2);
    return str;
}

int main(int argc, char *argv[]) {
    int ret;
    char* path = concatStr("./",argv[1]);
    ret = mkfifo(path,0777);
    if(ret < 0){
      return -1;
    }

    printf("create fifo sucess %s\r\n",path);
    free(path);
    return 0;
}