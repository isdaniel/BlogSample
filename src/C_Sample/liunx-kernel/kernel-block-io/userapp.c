#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <unistd.h>

#define RUNNING 1
#define STOP 0

int main(int argc,char* argv[]){
    char* filename = argv[1];
    int fd = open(filename ,O_RDWR);
    int ret;
    unsigned char data;

    if(fd < 0){
        printf("open filename %s failed \n",filename);
        return -1;
    }
    printf("into while(1)\n");
    while (1)
    {
        ret = read(fd, &data, sizeof(data));
		if (ret < 0) {  
			//printf("errrrrrrr\r\n");
		} else {		
			if (data){
                printf("RUNNING!!.....Get key value = %d\r\n", data);
            } else {
                //printf("stoppppppppppppppppp\r\n");
            }
		}
    }
    
    close(fd);
    return 0;
}