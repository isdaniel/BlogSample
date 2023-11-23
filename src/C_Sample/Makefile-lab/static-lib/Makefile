.PHONY: clean

hello:main.o
# lib{name}.a => staticlib
	gcc -o $@  $^ -L ./ -l staticlib
main.o:main.c libstaticlib.a
	gcc -o $@ -c -fPIC $^
libstaticlib.a:libstaticlib.o
	ar rcs $@ $^

clean:
	rm -f libstaticlib.a main.o libstaticlib.o hello
