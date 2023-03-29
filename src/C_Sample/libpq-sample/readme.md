
Please copy dynamic link library "libpq.so" to root path.

```c
gcc libpq-sample.c -I ./include/ -o libpq-sample -L. -lpq
```

Result as below:

Postgresql version usage 150001
Query returned 1 records with 1 fields:
Hello world!!