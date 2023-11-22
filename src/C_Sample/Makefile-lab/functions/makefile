PHONY: all
FILES = p.c f.h z.c
SRC  = $(wildcard *.c)
HEAD = $(wildcard *.h)
all:
	@echo "SRC = $(SRC)"
	@echo "HEAD = $(HEAD)"
	@echo "FILES = $(subst .c,.o,$(FILES))"
	$(call myfunction, hello dd)


define myfunction
	@echo "pram1 = $(0)"
	@echo "pram2 = $(1)"
endef
