#include <linux/module.h>
#include <linux/kernel.h>
#include <linux/fs.h>
#include <linux/init.h>
#include <linux/cdev.h>
#include <linux/device.h>
#include <linux/semaphore.h>
#include <linux/uaccess.h>

#define DEV_NAME "dev-lab01"
#define DEV_COUNT 1

struct lab01_dev{
    struct cdev cdev;
    dev_t devid;
    int major;        
    int minor;       
    struct device* device; 
    struct class* class;
};

struct lab01_dev dev_instance;
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
    //手動指定 major 驅動編號和 file_operations
    //int ret = register_chrdev(DEV_MAJOR, DEV_NAME,&fops);

    printk(KERN_INFO "dev-lab01: inin module\n");
    //有指定 majorid
    if(dev_instance.major){
        //向系統申請一個設備號,產生 device Id
        dev_instance.devid = MKDEV(dev_instance.major,0);
        ret = register_chrdev_region(dev_instance.devid,DEV_COUNT,DEV_NAME);
    } else {
        ret = alloc_chrdev_region(&dev_instance.devid,0,DEV_COUNT,DEV_NAME);
        dev_instance.major = MAJOR(dev_instance.devid);
        dev_instance.minor = MINOR(dev_instance.devid);
    }
    
    if(ret < 0){
        printk("dev-lab01 init failed\r\n");
        return -1;
    }
    
    dev_instance.cdev.owner = THIS_MODULE;
    cdev_init(&dev_instance.cdev,&fops);
    //初始化完畢 cdev 需要透過 cdev_add 添加到 liunx kernel 中.
    ret = cdev_add(&dev_instance.cdev,dev_instance.devid ,DEV_COUNT);
    printk("device majorId =%d, minorId =%d \r\n",dev_instance.major,
    dev_instance.minor);
    //printk(KERN_INFO "\t use mknod /dev/%s c %d %d  command for device file\n",DEV_NAME, dev_instance.major,dev_instance.minor);
    
    /*自動建立設備節點 mknod*/
    dev_instance.class = class_create(THIS_MODULE,DEV_NAME);
    if(IS_ERR(dev_instance.class)){
        return PTR_ERR(dev_instance.class);
    }

    dev_instance.device = device_create(dev_instance.class,NULL,dev_instance.devid,NULL,DEV_NAME);
    if(IS_ERR(dev_instance.device)){
        return PTR_ERR(dev_instance.device);
    }

    return 0;
}

static void driver_exit(void)
{
    cdev_del(&dev_instance.cdev);

    //unregister_chrdev(DEV_MAJOR, DEV_NAME);
    unregister_chrdev_region(dev_instance.devid,DEV_COUNT);

    /* firstly release device */
    device_destroy(dev_instance.class,dev_instance.devid);
    /* secondly release class */
    class_destroy(dev_instance.class);
    printk(KERN_INFO "dev-lab01: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);
MODULE_AUTHOR("danielshih");
MODULE_LICENSE("GPL");