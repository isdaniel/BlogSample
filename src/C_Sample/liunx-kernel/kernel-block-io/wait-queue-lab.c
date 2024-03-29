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
#include <linux/wait.h>

#define TIMER_FREQ(period) jiffies + msecs_to_jiffies(period)
#define RUNNING 1
#define STOP 0

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
    wait_queue_head_t r_wait;
    atomic_t flag;
} io_device;

void GetIntrTimerCallback(struct timer_list *t)
{
    io_device *dev_context = from_timer(dev_context, t, timer);
    int flag = atomic_read(&dev_context->flag);
    if(flag){
        printk(KERN_INFO "wake up dev_context->r_wait !!\r\n");
        //wake_up(&dev_context->r_wait); 
        atomic_set(&dev_context->flag,STOP);
        wake_up_interruptible(&dev_context->r_wait);
    } else {
        atomic_set(&dev_context->flag,RUNNING);
    }
    
    printk("dev_context->flag is %d, time period is %d\n",flag,dev_context->period);
    mod_timer(t, TIMER_FREQ(dev_context->period));
}

static int register_chrdevice(io_device *dev_context)
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

static int init_character_driver(io_device *dev_context, struct file_operations *fops)
{
    int ret = 0;

    cdev_init(&dev_context->cdev, fops);
    // 初始化完畢 character driver 需要透過 cdev_add 添加到 liunx kernel 中.
    ret = cdev_add(&dev_context->cdev, dev_context->devid, dev_context->dev_cnt);
    printk("device majorId =%d, minorId =%d \r\n", dev_context->major,
           dev_context->minor);

    return ret;
}

static int init_device_node(io_device *timerdev)
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

static void realse_chardriver_resource(io_device *dev_context)
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

io_device dev_context;
int ret;
char data[100];

ssize_t
timer_read(struct file *filp, char *buffer, size_t count, loff_t *ppos)
{
    io_device* dev =(io_device*) filp->private_data;
    unsigned char flag = atomic_read(&dev->flag);
    //printk("block-lab_read\r\n");
    #if 0
    ret = wait_event_interruptible(dev->r_wait, atomic_read(&dev->flag));
    if(ret){
        return -ERESTARTSYS;
    }
    #endif
    //printk("block-lab_read\r\n");
    if(atomic_read(&dev->flag) == STOP){
        return -ERESTARTSYS;
    }
    
    DECLARE_WAITQUEUE(wait, current);
    add_wait_queue(&dev->r_wait, &wait);	
    __set_current_state(TASK_INTERRUPTIBLE);
    schedule();							
    if(signal_pending(current))	{			
        ret = -ERESTARTSYS;
        goto wait_error;
    }
    __set_current_state(TASK_RUNNING);      
    remove_wait_queue(&dev->r_wait, &wait);   
    atomic_set(&dev->flag,STOP);
    ret = copy_to_user(buffer, &flag,sizeof(flag));
    
    return 0;

wait_error:
 	set_current_state(TASK_RUNNING);		
 	remove_wait_queue(&dev->r_wait, &wait);	
	return ret;
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

// long timer_iotcl(struct file *filp, unsigned int cmd, unsigned long arg)
// {
//     int ret = 0;
//     unsigned long cp_ret;
//     io_device* dev_context = (io_device*)filp->private_data;
//     switch (cmd)
//     {
//         case OPEN_CMD:
//             add_timer(&dev_context->timer);
//             break;
//         case CLOSE_CMD:
//             del_timer_sync(&dev_context->timer);
//             break;
//         case SET_PERIOD_CMD:    
//             //todo use spin lock
//             cp_ret = copy_from_user(&dev_context->period,(int*)arg,sizeof(int));
//             if(cp_ret < 0){
//                 printk(KERN_ERR "Kernel copy_from_user function error\r\n");
//                 return -EFAULT;
//             }
//             mod_timer(&dev_context->timer,TIMER_FREQ(dev_context->period));
//             break;
//         default:
//             ret = -EFAULT;
//             break;
//     }

//     return ret;
// }

struct file_operations fops = {
    .owner = THIS_MODULE,
    .open = timer_open,
    .release = timer_release,
    .write = timer_write,
    .read = timer_read
    //.unlocked_ioctl = timer_iotcl
};

static int driver_entry(void)
{
    // 手動指定 major 驅動編號和 file_operations
    // int ret = register_chrdev(DEV_MAJOR, DEV_NAME,&fops);

    printk(KERN_INFO "wait-queue-lab: inin module, dev_instance pos:%p\n", &dev_context);
    // 有指定 majorid
    dev_context.dev_name = "wait-queue-lab";
    dev_context.dev_cnt = 1;
    dev_context.owner = THIS_MODULE;
    ret = register_chrdevice(&dev_context);

    if (ret < 0)
    {
        printk("wait-queue-lab init failed\r\n");
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
    printk(KERN_INFO "ready atomic_set \r\n");
    atomic_set(&dev_context.flag, 0);
    
    timer_setup(&dev_context.timer, GetIntrTimerCallback, 0);
    dev_context.period = 1000;
    dev_context.timer.expires = TIMER_FREQ(dev_context.period);
    add_timer(&dev_context.timer);

    init_waitqueue_head(&dev_context.r_wait);
    
    // init_timer(&timerdev->timer);
    // timerdev->timer.function = timer_func;
    // timerdev->timer.expires = jiffies + msecs_to_jiffies(1000);

    return 0;
}

static void driver_exit(void)
{
    realse_chardriver_resource(&dev_context);
    printk(KERN_INFO "wait-queue-lab: unloaded module\n");
}

module_init(driver_entry);
module_exit(driver_exit);
MODULE_AUTHOR("danielshih");
MODULE_LICENSE("GPL");