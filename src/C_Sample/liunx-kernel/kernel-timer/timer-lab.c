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

#define TIMER_FREQ(period) jiffies + msecs_to_jiffies(period)
#define OPEN_CMD _IO(0XEF, 1)
#define CLOSE_CMD _IO(0XEF, 2)
#define SET_PERIOD_CMD _IOW(0XEF, 3, int)

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

    int cnt; /* for test */
} timer_device;

void GetIntrTimerCallback(struct timer_list *t)
{
    timer_device *dev_context = from_timer(dev_context, t, timer);
    dev_context->cnt++;
    printk("dev_context->cnt is %d, time period is %d\n", dev_context->cnt,dev_context->period);
    mod_timer(t, TIMER_FREQ(dev_context->period));
}

static int register_chrdevice(timer_device *dev_context)
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

static int init_character_driver(timer_device *dev_context, struct file_operations *fops)
{
    int ret = 0;

    cdev_init(&dev_context->cdev, fops);
    // 初始化完畢 character driver 需要透過 cdev_add 添加到 liunx kernel 中.
    ret = cdev_add(&dev_context->cdev, dev_context->devid, dev_context->dev_cnt);
    printk("device majorId =%d, minorId =%d \r\n", dev_context->major,
           dev_context->minor);

    return ret;
}

static int init_device_node(timer_device *timerdev)
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

static void realse_chardriver_resource(timer_device *dev_context)
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

timer_device dev_context;
int ret;
char data[100];

ssize_t
timer_read(struct file *filp, char *buffer, size_t count, loff_t *ppos)
{

    printk("lab01_read\r\n");
    ret = copy_to_user(buffer, data, count);
    return 0;
}

ssize_t
timer_write(struct file *filp, const char *buffer, size_t count, loff_t *ppos)
{
    printk("lab01_write\r\n");
    ret = copy_from_user(data, buffer, count);
    return ret;
}

int timer_release(struct inode *inode, struct file *filp)
{
    printk("lab01_rlease\r\n");
    return 0;
}

int timer_open(struct inode *inode, struct file *filp)
{
    filp->private_data = &dev_context;
    printk("lab01_open\r\n");
    return 0;
}

long timer_iotcl(struct file *filp, unsigned int cmd, unsigned long arg)
{
    int ret = 0;
    unsigned long cp_ret;
    timer_device* dev_context = (timer_device*)filp->private_data;
    switch (cmd)
    {
        case OPEN_CMD:
            add_timer(&dev_context->timer);
            break;
        case CLOSE_CMD:
            del_timer_sync(&dev_context->timer);
            break;
        case SET_PERIOD_CMD:    
            //todo use spin lock
            cp_ret = copy_from_user(&dev_context->period,(int*)arg,sizeof(int));
            if(cp_ret < 0){
                printk(KERN_ERR "Kernel copy_from_user function error\r\n");
                return -EFAULT;
            }
            mod_timer(&dev_context->timer,TIMER_FREQ(dev_context->period));
            break;
        default:
            ret = -EFAULT;
            break;
    }

    return ret;
}

struct file_operations fops = {
    .owner = THIS_MODULE,
    .open = timer_open,
    .release = timer_release,
    .write = timer_write,
    .read = timer_read,
    .unlocked_ioctl = timer_iotcl};

static int driver_entry(void)
{
    // 手動指定 major 驅動編號和 file_operations
    // int ret = register_chrdev(DEV_MAJOR, DEV_NAME,&fops);

    printk(KERN_INFO "timer-lab: inin module, dev_instance pos:%p\n", &dev_context);
    // 有指定 majorid
    dev_context.dev_name = "timer-lab";
    dev_context.dev_cnt = 1;
    dev_context.owner = THIS_MODULE;
    ret = register_chrdevice(&dev_context);

    if (ret < 0)
    {
        printk("timer-lab init failed\r\n");
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
    
    // init_timer(&timerdev->timer);
    // timerdev->timer.function = timer_func;
    // timerdev->timer.expires = jiffies + msecs_to_jiffies(1000);

    return 0;
}

static void driver_exit(void)
{
    realse_chardriver_resource(&dev_context);
    printk(KERN_INFO "timer-lab: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);
MODULE_AUTHOR("danielshih");
MODULE_LICENSE("GPL");