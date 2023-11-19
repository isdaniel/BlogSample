#include <stdio.h>
#include <pthread.h>

int lock = 0;

void *thread_func(void* arg)
{
    printf("start thread_func %d lock val:%d\n",*((int*)arg),lock);
    
    // Try to acquire the lock
    while (__sync_lock_test_and_set(&lock, 1)){
        printf("arg : %d, wait for spinlock: %d \r\n",*((int*)arg),lock);
    }

    // Critical section
    printf("Inside critical section %d, get lock %d \n",*((int*)arg),lock);

    // Release the lock
    __sync_lock_release(&lock);
}

int main(void)
{
    printf("spin lock start\r\n");
    pthread_t thread1, thread2;
    int* ptr1= malloc(sizeof(int)); 
    int* ptr2= malloc(sizeof(int));
    *ptr1 = 1;
    *ptr2 = 2;
    pthread_create(&thread1, NULL, thread_func, ptr1);
    pthread_create(&thread2, NULL, thread_func, ptr2);
    pthread_join(thread1, NULL);
    pthread_join(thread2, NULL);
    return 0;
}