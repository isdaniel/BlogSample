#include <linux/module.h>
#include <linux/kernel.h>
#include <linux/fs.h>
#include <linux/cdev.h>
#include <linux/semaphore.h>
#include <linux/uaccess.h>


struct fake_device{
        char data[100];
        struct semaphore sem;
} virtual_device;

struct cdev *mcdev;
int major_number;
int ret;
dev_t dev_num;

#define DEVICE_NAME "dainel_device"

int device_open(struct inode *inode,struct file *filp){
        if(down_interruptible(&virtual_device.sem)!=0){


                return -1;
        }

        printk(KERN_INFO "device: opened device\n");

        return 0;
}

ssize_t device_read(struct file *filp,char* bufStoreData,size_t bufCount,loff_t* curOffset){

        printk(KERN_INFO "device: Reading from device.\n");
        ret = copy_to_user(bufStoreData,virtual_device.data,bufCount);
        return ret;
}


ssize_t device_write(struct file *filp,const char* bufSourceData,size_t bufCount,loff_t* curOffset){
        printk(KERN_INFO "device: writing to device.\n");
        ret = copy_from_user(virtual_device.data,bufSourceData,bufCount);
        return ret;
}


int device_close(struct inode *inode,struct file *filp){

        up(&virtual_device.sem);
        printk(KERN_INFO "device: closed device.\n");
        return 0;
}

struct file_operations fops ={
        .owner = THIS_MODULE,
        .open = device_open,
        .release = device_close,
        .write = device_write,
        .read = device_read
};


static int driver_entry(void){

        ret = alloc_chrdev_region(&dev_num,0,1,DEVICE_NAME);
        if(ret < 0){
                printk(KERN_ALERT "fake device failed to allocate a major number.\n");
                return ret;
        }

        major_number = MAJOR(dev_num);
        printk(KERN_INFO "device major number is %d\n",major_number);
        printk(KERN_INFO "\t use mknod /dev/%s c %d 0 command for device file\n",DEVICE_NAME,major_number);

        //create our cdev structure initialized our cdev
        mcdev = cdev_alloc();
        mcdev->ops = &fops;
        mcdev->owner = THIS_MODULE;

        ret = cdev_add(mcdev,dev_num,1);
        if(ret < 0){
                printk(KERN_ALERT "device unable to add cdev to kernel\n");
                return ret;
        }

        sema_init(&virtual_device.sem,1);
        return 0;

}


static void driver_exit(void){
        cdev_del(mcdev);

        unregister_chrdev_region(dev_num,1);

        printk(KERN_INFO " device: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);