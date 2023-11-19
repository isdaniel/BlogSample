#include <sys/types.h>
#include <sys/ipc.h>
#include <sys/shm.h>
#include <stdio.h>

int main()
{
    key_t key = 1234;                                // 這裡需要指定一個用於唯一標識共享記憶體段的鍵值
    int shmid = shmget(key, 512, 0666 | IPC_CREAT); // 獲取一個新的共享記憶體段，大小為 512 bytes
    if (shmid < 0)
    {
        perror("shmget failed"); // 如果獲取失敗，顯示錯誤信息
        return -1;
    }

    char *shmaddr = shmat(shmid, NULL, 0); // 將共享記憶體段附加到進程的地址空間中
    if (shmaddr == (char *)-1)
    {
        perror("shmat failed"); // 如果附加失敗，顯示錯誤信息
        return -1;
    }

    printf("shmat returned %p\n", shmaddr); // 如果成功，顯示共享記憶體段附加到進程中的地址
    system("ipcs -m");
    fgets(shmaddr,512,stdin);

    printf("shmaddr val %s\n", shmaddr );
    // del user process mem that map from kernel mem
    shmdt(shmaddr);
    
    return 0;
}