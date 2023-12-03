#include <linux/module.h>
#include <linux/kernel.h>
#include <linux/fs.h>
#include <linux/init.h>
#include <linux/cdev.h>
#include <linux/device.h>
#include <linux/semaphore.h>
#include <linux/uaccess.h>
#include <linux/slab.h>
// #include "device-library.h"

typedef struct device_context
{
    dev_t devid;
    int major;
    int minor;
    char* dev_name;
    unsigned dev_cnt;
    struct cdev cdev;
    struct device *device;
    struct class *class;
    struct module *owner;
    int sn;
} device_context;

static int register_chrdevice(device_context* dev_context)
{
    int ret = 0;
    if (dev_context->major)
    {
        // 向系統申請一個設備號,產生 device Id
        dev_context->devid = MKDEV(dev_context->major, 0);
        ret = register_chrdev_region(dev_context->devid, dev_context->dev_cnt, dev_context->dev_name);
    }
    else
    {
        ret = alloc_chrdev_region(&dev_context->devid, 0,dev_context->dev_cnt, dev_context->dev_name);
        dev_context->major = MAJOR(dev_context->devid);
        dev_context->minor = MINOR(dev_context->devid);
    }
    dev_context->cdev.owner = dev_context->owner;
    return ret;
}

static int init_character_driver(device_context* dev_context,struct file_operations* fops){
    int ret = 0;

    cdev_init(&dev_context->cdev,fops);
    //初始化完畢 character driver 需要透過 cdev_add 添加到 liunx kernel 中.
    ret = cdev_add(&dev_context->cdev,dev_context->devid ,dev_context->dev_cnt);
    printk("device majorId =%d, minorId =%d \r\n",dev_context->major,
    dev_context->minor);

    return ret;
}

static int init_device_node(device_context* dev_context)
{
    dev_context->class = class_create(dev_context->owner,dev_context->dev_name);
    if(IS_ERR(dev_context->class)){
        return PTR_ERR(dev_context->class);
    }

    dev_context->device = device_create(dev_context->class,NULL,dev_context->devid,NULL,dev_context->dev_name);
    if(IS_ERR(dev_context->device)){
        return PTR_ERR(dev_context->device);
    }

    return 0;
}

static void realse_chardriver_resource(device_context* dev_context){
    cdev_del(&dev_context->cdev);

    //unregister_chrdev(DEV_MAJOR, DEV_NAME);
    unregister_chrdev_region(dev_context->devid,dev_context->dev_cnt);

    /* firstly release device */
    device_destroy(dev_context->class,dev_context->devid);
    /* secondly release class */
    class_destroy(dev_context->class);
}

device_context dev_context;
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

    printk(KERN_INFO "dev-lab01: inin module, dev_instance pos:%p\n",&dev_context);
    //有指定 majorid
    dev_context.dev_name = "dev-lab01";
    dev_context.dev_cnt = 1;
    dev_context.owner = THIS_MODULE;
    ret = register_chrdevice(&dev_context);
    
    if(ret < 0){
        printk("dev-lab01 init failed\r\n");
        return -1;
    }
    ret = init_character_driver(&dev_context,&fops);
    
    /*自動建立設備節點 mknod*/
    ret = init_device_node(&dev_context);
    if(ret != 0){
         printk(KERN_ERR "init_device_node failed\r\n");
         return ret;
    }

    return 0;
}

static void driver_exit(void)
{
    realse_chardriver_resource(&dev_context);
    printk(KERN_INFO "dev-lab01: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);
MODULE_AUTHOR("danielshih");
MODULE_LICENSE("GPL");