ifeq ($(DEBUG),"true")
CC = gcc -g
else
CC = gcc
endif
all:run_player
run_player:lcd.o player.o
	$(CC) -o run_player lcd.o player.o
player.o:player.c
	$(CC) -o player.o -c player.c
lcd.o:lcd.c
	$(CC) -o lcd.o -c lcd.c
clean:
	rm lcd.o run_player player.o