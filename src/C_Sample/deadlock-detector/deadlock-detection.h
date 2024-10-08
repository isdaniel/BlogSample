#ifndef CYCLE_DETECTION_H
#define CYCLE_DETECTION_H

#define _GNU_SOURCE
#include <stdio.h>
#include <dlfcn.h>
#include <pthread.h>
#include <unistd.h>
#include <stdlib.h>
#include <stdint.h>
#include <signal.h>

#define MAX 64
typedef unsigned long int uint64;

void init_hook();
void start_check(void);
int pthread_mutex_lock(pthread_mutex_t *mutex);
int pthread_mutex_unlock(pthread_mutex_t *mutex);
#endif 