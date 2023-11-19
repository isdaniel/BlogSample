
# declare var (cmd)
CC = gcc

# target:dependencies
#	command
test: test.c foo.o
	$(CC) test.c foo.o -o test
foo.o: foo.c
	$(CC) -c foo.c 
clean:
	rm *.o test