#include <linux/module.h>
#include <linux/kernel.h>
#include <linux/fs.h>
#include <linux/init.h>
#include <linux/cdev.h>
#include <linux/device.h>
#include <linux/semaphore.h>
#include <linux/uaccess.h>
#include <linux/slab.h>
#include <linux/timer.h>
#include <linux/atomic.h>

#include <linux/fcntl.h>
#include <linux/poll.h>
#include <linux/fs.h>


#define TIMER_FREQ(period) jiffies + msecs_to_jiffies(period)

int signal_fasync(int fd,struct file* file, int on);

typedef struct
{
    dev_t devid;
    int major;
    int minor;
    char *dev_name;
    unsigned dev_cnt;
    struct cdev cdev;
    struct device *device;
    struct class *class;
    struct module *owner;
    struct timer_list timer; /* timer */
    int period;
    atomic_t flag;
    struct fasync_struct* async_queue;
    atomic_t cnt;
} signal_device;

void GetIntrTimerCallback(struct timer_list *t)
{
    signal_device *dev_context = from_timer(dev_context, t, timer);
    mod_timer(t, TIMER_FREQ(dev_context->period));

    //sent signal to application
    kill_fasync(&dev_context->async_queue,SIGIO,POLL_IN);
}

static int register_chrdevice(signal_device *dev_context)
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
        ret = alloc_chrdev_region(&dev_context->devid, 0, dev_context->dev_cnt, dev_context->dev_name);
        dev_context->major = MAJOR(dev_context->devid);
        dev_context->minor = MINOR(dev_context->devid);
    }
    dev_context->cdev.owner = dev_context->owner;
    return ret;
}

static int init_character_driver(signal_device *dev_context, struct file_operations *fops)
{
    int ret = 0;

    cdev_init(&dev_context->cdev, fops);
    // 初始化完畢 character driver 需要透過 cdev_add 添加到 liunx kernel 中.
    ret = cdev_add(&dev_context->cdev, dev_context->devid, dev_context->dev_cnt);
    printk("device majorId =%d, minorId =%d \r\n", dev_context->major,
           dev_context->minor);

    return ret;
}

static int init_device_node(signal_device *timerdev)
{
    timerdev->class = class_create(timerdev->owner, timerdev->dev_name);
    if (IS_ERR(timerdev->class))
    {
        return PTR_ERR(timerdev->class);
    }

    timerdev->device = device_create(timerdev->class, NULL, timerdev->devid, NULL, timerdev->dev_name);
    if (IS_ERR(timerdev->device))
    {
        return PTR_ERR(timerdev->device);
    }

    return 0;
}

static void realse_chardriver_resource(signal_device *dev_context)
{
    del_timer(&dev_context->timer);

    cdev_del(&dev_context->cdev);

    // unregister_chrdev(DEV_MAJOR, DEV_NAME);
    unregister_chrdev_region(dev_context->devid, dev_context->dev_cnt);

    /* firstly release device */
    device_destroy(dev_context->class, dev_context->devid);
    /* secondly release class */
    class_destroy(dev_context->class);
}

signal_device dev_context;
int ret;
//char data[100];

ssize_t
timer_read(struct file *filp, char *buffer, size_t count, loff_t *ppos)
{
    signal_device* dev = filp->private_data;    
    atomic_add(1,&dev->cnt);
    printk("lab01_read\r\n");
    unsigned char cnt = atomic_read(&dev->cnt);
    ret = copy_to_user(buffer, &cnt, sizeof(cnt));
    return 0;
}

ssize_t
timer_write(struct file *filp, const char *buffer, size_t count, loff_t *ppos)
{
    printk("lab01_write\r\n");
    //ret = copy_from_user(data, buffer, count);
    return 0;
}

int signal_release(struct inode *inode, struct file *filp)
{
    printk("lab01_rlease\r\n");
    //release signal
    return signal_fasync(-1,filp,0);    
}

int timer_open(struct inode *inode, struct file *filp)
{
    filp->private_data = &dev_context;
    printk("lab01_open\r\n");
    return 0;
}

int 
signal_fasync(int fd,struct file* file, int on)
{
    signal_device* dev = file->private_data;    
    printk("signal_fasync processing!\r\n");
    return fasync_helper(fd,file,on,&dev->async_queue);
}

struct file_operations fops = {
    .owner = THIS_MODULE,
    .open = timer_open,
    .release = signal_release,
    .write = timer_write,
    .read = timer_read,
    .fasync = signal_fasync

};

static int driver_entry(void)
{
    // 手動指定 major 驅動編號和 file_operations
    // int ret = register_chrdev(DEV_MAJOR, DEV_NAME,&fops);

    printk(KERN_INFO "signal-lab: inin module, dev_instance pos:%p\n", &dev_context);
    // 有指定 majorid
    dev_context.dev_name = "signal-lab";
    dev_context.dev_cnt = 1;
    dev_context.owner = THIS_MODULE;
    ret = register_chrdevice(&dev_context);

    if (ret < 0)
    {
        printk("signal-lab init failed\r\n");
        return -1;
    }
    ret = init_character_driver(&dev_context, &fops);

    /*自動建立設備節點 mknod*/
    ret = init_device_node(&dev_context);
    if (ret != 0)
    {
        printk(KERN_ERR "init_device_node failed\r\n");
        return ret;
    }

    // 初始化 timer
    timer_setup(&dev_context.timer, GetIntrTimerCallback, 0);
    dev_context.period = 1000;
    dev_context.timer.expires = TIMER_FREQ(dev_context.period);
    add_timer(&dev_context.timer);
    atomic_set(&dev_context.cnt,0);
    return 0;
}

static void driver_exit(void)
{
    realse_chardriver_resource(&dev_context);
    printk(KERN_INFO "signal-lab: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);
MODULE_AUTHOR("danielshih");
MODULE_LICENSE("GPL");