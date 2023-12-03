ifeq ($(DEBUG),"true")
CC = gcc -g
else
CC = gcc
endif
BIN = run_player

all:$(BIN)
$(BIN):lcd.o player.o
	$(CC) -o $(BIN) lcd.o player.o
%.o:%.c
	@echo "build code..."
	$(CC) -o $@ -c $^
	@echo "build sucess..."
# player.o:player.c
# 	$(CC) -o player.o -c player.c
# lcd.o:lcd.c
# 	$(CC) -o lcd.o -c lcd.c

#same process execute command.
showpath:
	cd /BlogSample; \
	tree; \
	pwd; 
clean:
	rm -f lcd.o $(BIN) player.o