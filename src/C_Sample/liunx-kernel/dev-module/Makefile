obj-m += dev-lab01.o
KDIR = /usr/src/linux-headers-$(shell uname -r)

all:
	$(MAKE) -C $(KDIR) M=$(shell pwd) modules
clean:
	rm -rf *.o *.ko *.mod.* *.symvers *.order 