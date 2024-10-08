#include "deadlock-detection.h"

enum Type
{
	PROCESS,
	RESOURCE
};

struct source_node
{
	uint64 id;
	enum Type type;
	uint64 lock_id; //mutex address
	int degress;
};

struct vertex
{
	struct source_node node;
	struct vertex *next;
};

struct task_graph
{
	struct vertex list[MAX];
	struct source_node locklist[MAX];
	int num;
	int lockidx;
};

struct task_graph *tg = NULL;
int path[MAX + 1];
int visited[MAX];
int k = 0;
int deadlock = 0;


int inc_atomic(int *value, int add)
{
	int old;
	__asm__ volatile(
		"lock;xaddl %2, %1;"
		: "=a"(old)
		: "m"(*value), "a"(add)
		: "cc", "memory");
	return old;
}

struct vertex *create_vertex(struct source_node node)
{
	struct vertex *tex = (struct vertex *)malloc(sizeof(struct vertex));
	tex->node = node;
	tex->next = NULL;
	return tex;
}

int search_vertex(struct source_node node)
{
	for (int i = 0; i < tg->num; i++)
	{
		if (tg->list[i].node.type == node.type && tg->list[i].node.id == node.id)
		{
			return i;
		}
	}
	return -1;
}
void add_vertex(struct source_node type)
{
	if (search_vertex(type) == -1)
	{
		tg->list[tg->num].node = type;
		tg->list[tg->num].next = NULL;
		inc_atomic(&tg->num, 1);
	}
}
void add_edge(struct source_node from, struct source_node to)
{
	add_vertex(from);
	add_vertex(to);
	struct vertex *v = &(tg->list[search_vertex(from)]);
	while (v->next != NULL)
	{
		v = v->next;
	}
	v->next = create_vertex(to);
}

int verify_edge(struct source_node i, struct source_node j)
{
	if (tg->num == 0)
		return 0;
	int idx = search_vertex(i);
	if (idx == -1)
	{
		return 0;
	}
	struct vertex *v = &(tg->list[idx]);
	while (v != NULL)
	{
		if (v->node.id == j.id)
			return 1;
		v = v->next;
	}
	return 0;
}
void remove_edge(struct source_node from, struct source_node to)
{
	int idxi = search_vertex(from);
	int idxj = search_vertex(to);
	if (idxi != -1 && idxj != -1)
	{
		struct vertex *v = &tg->list[idxi];
		struct vertex *remove;
		while (v->next != NULL)
		{
			if (v->next->node.id == to.id)
			{
				remove = v->next;
				v->next = v->next->next;
				free(remove);
				break;
			}
			v = v->next;
		}
	}
}

void print_deadlock(void)
{
	int i = 0;
	printf("deadlock : ");
	for (i = 0; i < k - 1; i++)
	{
		printf("%ld --> ", tg->list[path[i]].node.id);
	}
	printf("%ld\n", tg->list[path[i]].node.id);
}

int DFS(int idx)
{
	struct vertex *ver = &tg->list[idx];
	if (visited[idx] == 1)
	{
		path[k++] = idx;
		print_deadlock();
		deadlock = 1;
		return 0;
	}
	visited[idx] = 1;
	path[k++] = idx;
	while (ver->next != NULL)
	{
		DFS(search_vertex(ver->next->node));
		k--;
		ver = ver->next;
	}
	return 1;
}

void search_for_cycle(int idx)
{
	struct vertex *ver = &tg->list[idx];
	visited[idx] = 1;
	k = 0;
	path[k++] = idx;
	while (ver->next != NULL)
	{
		int i = 0;
		for (i = 0; i < tg->num; i++)
		{
			if (i == idx)
				continue;
			visited[i] = 0;
		}
		for (i = 1; i <= MAX; i++)
		{
			path[i] = -1;
		}
		k = 1;
		DFS(search_vertex(ver->next->node));
		ver = ver->next;
	}
}

void check_dead_lock(void)
{
	int i = 0;
	deadlock = 0;
	for (i = 0; i < tg->num; i++)
	{
		if (deadlock == 1)
			break;
		search_for_cycle(i);
	}
	if (deadlock == 0)
	{
		printf("no deadlock\n");
	}
}

static void *thread_routine(void *args)
{
	while (1)
	{
		sleep(1);
		check_dead_lock();
		// break deadlock
		if (deadlock == 1)
		{
			for (int i = 0; i < MAX; i++)
			{
				if (path[i] == 1)
				{
					pthread_t tid = tg->locklist[i].id;
					printf("deadlock detected, kill tid = %ld\r\n", tid);
					pthread_kill(tid, SIGINT);
				}
			}
		}
	}

	return NULL;
}



void start_check(void)
{
	tg = (struct task_graph *)malloc(sizeof(struct task_graph));
	tg->num = 0;
	tg->lockidx = 0;
	pthread_t tid;
	pthread_create(&tid, NULL, thread_routine, NULL);
}

int search_lock(uint64 lock)
{
	for (int i = 0; i < tg->lockidx; i++)
	{
		if (tg->locklist[i].lock_id == lock)
		{
			return i;
		}
	}
	return -1;
}

int search_empty_lock(uint64 lock)
{
	for (int i = 0; i < tg->lockidx; i++)
	{
		if (tg->locklist[i].lock_id == 0)
		{
			return i;
		}
	}
	return tg->lockidx;
}

void print_locklist(void)
{
	printf("print_locklist: \n");
	printf("---------------------\n");
	for (int i = 0; i < tg->lockidx; i++)
	{
		printf("threadid : %ld, lockid: %ld\n", tg->locklist[i].id, tg->locklist[i].lock_id);
	}
	printf("---------------------\n\n\n");
}

void lock_before(uint64 thread_id, uint64 lockaddr)
{
	// list<threadid, toThreadid>
	for (int idx = 0; idx < tg->lockidx; idx++)
	{
		if ((tg->locklist[idx].lock_id == lockaddr))
		{
			struct source_node from;
			from.id = thread_id;
			from.type = PROCESS;
			add_vertex(from);
			struct source_node to;
			to.id = tg->locklist[idx].id;
			tg->locklist[idx].degress++;
			to.type = PROCESS;
			add_vertex(to);

			if (!verify_edge(from, to))
			{
				add_edge(from, to); //
			}
		}
	}
}

void lock_after(uint64 thread_id, uint64 lockaddr)
{
	int idx = 0;
	if (-1 == (idx = search_lock(lockaddr)))
	{ 
		int eidx = search_empty_lock(lockaddr);

		tg->locklist[eidx].id = thread_id;
		tg->locklist[eidx].lock_id = lockaddr;

		inc_atomic(&tg->lockidx, 1);
	}
	else
	{
		struct source_node from;
		from.id = thread_id;
		from.type = PROCESS;
		struct source_node to;
		to.id = tg->locklist[idx].id;
		tg->locklist[idx].degress--;
		to.type = PROCESS;
		if (verify_edge(from, to))
			remove_edge(from, to);
		tg->locklist[idx].id = thread_id;
	}
}
void unlock_after(uint64 thread_id, uint64 lockaddr)
{
	int idx = search_lock(lockaddr);
	if (tg->locklist[idx].degress == 0)
	{
		tg->locklist[idx].id = 0;
		tg->locklist[idx].lock_id = 0;
		inc_atomic(&tg->lockidx, -1);
		inc_atomic(&tg->num, -1);
	}
}

typedef int (*pthread_mutex_lock_t)(pthread_mutex_t *mutex);
pthread_mutex_lock_t pthread_mutex_lock_f;
typedef int (*pthread_mutex_unlock_t)(pthread_mutex_t *mutex);
pthread_mutex_unlock_t pthread_mutex_unlock_f;

int pthread_mutex_lock(pthread_mutex_t *mutex)
{
	pthread_t selfid = pthread_self(); //

	lock_before(selfid, (uint64)mutex);
	pthread_mutex_lock_f(mutex);
	lock_after(selfid, (uint64)mutex);
	return 0;
}

int pthread_mutex_unlock(pthread_mutex_t *mutex)
{
	pthread_t selfid = pthread_self();
	pthread_mutex_unlock_f(mutex);
	unlock_after(selfid, (uint64)mutex);
	return 0;
}

void init_hook()
{
	pthread_mutex_lock_f = dlsym(RTLD_NEXT, "pthread_mutex_lock");
	pthread_mutex_unlock_f = dlsym(RTLD_NEXT, "pthread_mutex_unlock");
}
