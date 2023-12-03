.PHONY: clean

hello:main.o
	gcc -o $@  $^ -L ./ -l dll
main.o:main.c libdll.so
	gcc -o $@ -c -fPIC $^
libdll.so:dll.o
	gcc -o $@ -shared $^
#move to default dll path for user
	cp $@ /usr/lib/
dll.o:dll.c
	gcc -o $@ -fPIC -c $^
clean:
	rm libdll.so dll.o hello main.o
