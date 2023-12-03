obj-m += device-module.o
KDIR = /usr/src/linux-headers-$(shell uname -r)
MY_CFLAGS += -g -DDEBUG
ccflags-y += ${MY_CFLAGS}
CC += ${MY_CFLAGS}

all:
	$(MAKE) -C $(KDIR) M=$(shell pwd) modules
debug:
	make -C $(KDIR) M=$(PWD) modules 
	EXTRA_CFLAGS="$(MY_CFLAGS)"
clean:
	make -C $(KDIR) M=$(PWD) clean 


