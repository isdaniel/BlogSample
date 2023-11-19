CC = gcc
obj-m += dev-lab01.o
MY_CFLAGS += -g -DDEBUG
ccflags-y += ${MY_CFLAGS}
# dev-lab01-objs := dev-lab01.o device-library.o
KDIR = /usr/src/linux-headers-$(shell uname -r)

all: 
	$(MAKE) -C $(KDIR) M=$(shell pwd) modules
# device-library.o:
# 	$(CC) -c ./lib/device-library.c -o device-library
debug:
	make -C $(KDIR) M=$(PWD) modules 
	EXTRA_CFLAGS="$(MY_CFLAGS)"
clean:
	make -C $(KDIR) M=$(PWD) clean 