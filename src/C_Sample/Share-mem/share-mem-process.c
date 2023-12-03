#include <stdio.h>
#include <stdlib.h>
#include <sys/shm.h>
#include <string.h>

int main(int argc, char *argv[]) {
    /*
    int shmget(key_t key,int size,int shflg);
        key: IPC_PRIVATE or ftok return value
        size: mem allocat size
        shmflg: open function auth ex:0777
    we can use (ipcs -m) to take a look IPC object
    */
    int shmid = shmget(IPC_PRIVATE,64,0777);

    /*
    int key =ftok("./a.c","test");
    int shmid = shmget(key,64,IPC_CREAT|0777);
    */
    if(shmid < 0){
        printf("create share mem failed!\r\n");
        return -1;
    }

    printf("create share mem sucess shmid =%d!\r\n",shmid);
    system("ipcs -m");
    return 0;
}