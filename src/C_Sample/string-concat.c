#include <stdio.h>
#include <stdlib.h>
#include <string.h>

char* concatStr(char* str1,char* str2){
    int len = strlen(str1) + strlen(str2) + 1;
    char* str = (char*)malloc((len+1)*sizeof(char));
    strcat(str, str1);
    strcat(str, str2);
    return str;
}

int main(int argc, char *argv[]) {
    char* str = concatStr(argv[1],argv[2]);
    printf("arg1= %s,arg2=%s, str = %s\r\n",argv[1],argv[2],str);
    free(str);
    return 0;
}