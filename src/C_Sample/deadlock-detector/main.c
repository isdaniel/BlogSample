#include "deadlock-detection.h"
#include <stdio.h>
#include <unistd.h>

pthread_mutex_t mutex_1 = PTHREAD_MUTEX_INITIALIZER;
pthread_mutex_t mutex_2 = PTHREAD_MUTEX_INITIALIZER;
pthread_mutex_t mutex_3 = PTHREAD_MUTEX_INITIALIZER;
pthread_mutex_t mutex_4 = PTHREAD_MUTEX_INITIALIZER;

void *thread_rountine_1(void *args)
{
	pthread_t selfid = pthread_self(); //
	printf("thread_routine 1 : %ld \n", selfid);
	pthread_mutex_lock(&mutex_1);
	sleep(1);
	pthread_mutex_lock(&mutex_2);
	pthread_mutex_unlock(&mutex_2);
	pthread_mutex_unlock(&mutex_1);
	return (void *)(0);
}

void *thread_rountine_2(void *args)
{
	pthread_t selfid = pthread_self(); //
	printf("thread_routine 2 : %ld \n", selfid);

	pthread_mutex_lock(&mutex_2);
	sleep(1);
	pthread_mutex_lock(&mutex_3);
	pthread_mutex_unlock(&mutex_3);
	pthread_mutex_unlock(&mutex_2);
	return (void *)(0);
}

void *thread_rountine_3(void *args)
{
	pthread_t selfid = pthread_self(); //
	printf("thread_routine 3 : %ld \n", selfid);
	pthread_mutex_lock(&mutex_3);
	sleep(1);
	pthread_mutex_lock(&mutex_4);
	pthread_mutex_unlock(&mutex_4);
	pthread_mutex_unlock(&mutex_3);
	return (void *)(0);
}

void *thread_rountine_4(void *args)
{
	pthread_t selfid = pthread_self(); //
	printf("thread_routine 4 : %ld \n", selfid);

	pthread_mutex_lock(&mutex_4);
	sleep(1);
	pthread_mutex_lock(&mutex_1);
	pthread_mutex_unlock(&mutex_1);
	pthread_mutex_unlock(&mutex_4);
	return (void *)(0);
}

int main()
{
	// pthread_kill
	init_hook();
	start_check();
	printf("start_check\n");
	pthread_t tid1, tid2, tid3, tid4;
	pthread_create(&tid1, NULL, thread_rountine_1, NULL);
	pthread_create(&tid2, NULL, thread_rountine_2, NULL);
	pthread_create(&tid3, NULL, thread_rountine_3, NULL);
	pthread_create(&tid4, NULL, thread_rountine_4, NULL);
	pthread_join(tid1, NULL);
	pthread_join(tid2, NULL);
	pthread_join(tid3, NULL);
	pthread_join(tid4, NULL);

	//getchar();
	return 0;
}