#define _GNU_SOURCE 
#include <stdio.h>
#include <pthread.h>
#include <time.h>
#include <sched.h> 

#define CACHELINE_SIZE 64 

//sudo perf stat -e cache-misses,cache-references ./main
struct shared_data {
    int data1 ;
    //int data2; //false sharing 
    int data2  __attribute__((aligned(CACHELINE_SIZE))); //no false sharing  
} shared;

void set_cpu(int cpu_id,const char* func_name){
    cpu_set_t cpuset;
    CPU_ZERO(&cpuset);
    CPU_SET(cpu_id, &cpuset);
    sched_setaffinity(0, sizeof(cpuset), &cpuset);
    printf("sched_setaffinity cpu%d\n", cpu_id);
    printf("%s thread sched_getcpu = %d\n",func_name, sched_getcpu());
}

void *update_data1() {
    //set_cpu(0,"update_data1");
    for (int i = 0; i < 1000000000; ++i) {
        shared.data1 ++;
    }
    return NULL;
}

void *update_data2() {
    //set_cpu(1,"update_data2");
    for (int i = 0; i < 100000000; ++i) {
        shared.data2+=2;
    }
    return NULL;
}

double get_time_diff(struct timespec start, struct timespec end) {
    return (end.tv_sec - start.tv_sec) + (end.tv_nsec - start.tv_nsec) / 1e9;
}

int main() {
    pthread_t threads[2];
    struct timespec start, end;

    // Start timer
    clock_gettime(CLOCK_MONOTONIC, &start);

    pthread_create(&threads[0], NULL, update_data1, NULL);
    pthread_create(&threads[1], NULL, update_data2, NULL);

    // Wait for threads to finish
    pthread_join(threads[0], NULL);
    pthread_join(threads[1], NULL);
    
    // End timer
    clock_gettime(CLOCK_MONOTONIC, &end);

    double time_taken = get_time_diff(start, end);
    printf("Without cacheline alignment: data1 = %d, data2 = %d\n", shared.data1, shared.data2);
    printf("Time taken: %f seconds\n", time_taken);
    
    return 0;
}
