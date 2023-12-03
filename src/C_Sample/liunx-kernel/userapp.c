#include <stdio.h>
#include <stdlib.h>
#include <fcntl.h>

#define DEVICE "/dev/dainel_device"

int main(){
        int i,fd;
        char ch,write_buf[100],read_buf[100];

        fd = open(DEVICE,O_RDWR);

        if(fd < 0){
                printf("file %s either doesn't exist or has been locked.\n",DEVICE);
                exit(-1);
        }

        printf("r = read from device \nw = write to device\n enter command:");

        scanf("%c",&ch);

        if(ch == 'w'){
                printf("enter data:");
                scanf(" %[^\n]",write_buf);
                write(fd,write_buf,sizeof(write_buf));
        } else if (ch == 'r'){
                read(fd,read_buf,sizeof(read_buf));
                printf("message from device:%s\n",read_buf);
        }
        close(fd);
        return 0;
}