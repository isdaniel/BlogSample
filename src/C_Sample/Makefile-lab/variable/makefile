TARGET = 1

all: Non scope-modify bear-zoo elephant-zoo

%-zoo: TARGET=50

bear-zoo:
	@echo "bear-zoo TARGET : $(TARGET)"

elephant-zoo: TARGET += 1
elephant-zoo: 
	@echo "elephant-zoo TARGET : $(TARGET)"

Non:
	@echo "Non TARGET : $(TARGET)"

scope-modify: TARGET = 100
scope-modify:
	@echo "scope-modify TARGET : $(TARGET)"


