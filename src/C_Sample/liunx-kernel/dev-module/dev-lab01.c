#include <linux/module.h>
#include <linux/kernel.h>
#include <linux/fs.h>
#include <linux/init.h>
// #include <linux/cdev.h>
// #include <linux/semaphore.h>
// #include <linux/uaccess.h>
#define DEV_NAME "dev-lab01"
#define DEV_MAJOR 200

dev_t major;
int ret;
char data[100];

ssize_t 
lab01_read(struct file* filp, char* buffer, size_t count, loff_t* ppos)
{
    printk("lab01_read\r\n");

    ret = copy_to_user(buffer,data,count);
    return 0;
}

ssize_t 
lab01_write(struct file* filp, const char* buffer, size_t count, loff_t* ppos)
{
    printk("lab01_write\r\n");
    ret = copy_from_user(data,buffer,count);
    return ret;
}

int 
lab01_rlease(struct inode *inode, struct file *filp)
{
    printk("lab01_rlease\r\n");
    return 0;
}

int 
lab01_open(struct inode *inode, struct file *filp)
{
    printk("lab01_open\r\n");
    return 0;
}

struct file_operations fops = {
    .owner = THIS_MODULE,
    .open = lab01_open,
    .release = lab01_rlease,
    .write = lab01_write,
    .read = lab01_read
};

static int driver_entry(void)
{
    int ret = register_chrdev(DEV_MAJOR, DEV_NAME,&fops);
    printk(KERN_INFO "dev-lab01: inin module\n");

    if(ret < 0){
        printk("dev-lab01 init failed\r\n");
    }
    return 0;
}

static void driver_exit(void)
{
    unregister_chrdev(DEV_MAJOR, DEV_NAME);
    printk(KERN_INFO "dev-lab01: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);
MODULE_AUTHOR("danielshih");
MODULE_LICENSE("GPL");