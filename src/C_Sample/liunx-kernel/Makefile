obj-m += device-module.o
KDIR = /usr/src/linux-headers-$(shell uname -r)

all:
	$(MAKE) -C $(KDIR) M=$(shell pwd) modules
clean:
	rm -rf *.o *.ko *.mod.* *.symvers *.order