PHONY: all

A = 1 3 4 5 6 7 8 9
B = $(foreach i,$(A),$(addprefix 0.,$(i)))
C = $(foreach i,$(A),$(addsuffix .0,$(i)))

all:
	@echo "A = ${A}"
	@echo "B = ${B}"
	@echo "C = ${C}"