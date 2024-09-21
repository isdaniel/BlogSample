# Compiler and flags
CC = gcc
CFLAGS = -g -Wall -pthread -ldl

# Source files and object files
SRCS = main.c deadlock-detection.c
OBJS = $(SRCS:.c=.o)

# Output executable
TARGET = main

# Default target: build the executable
all: $(TARGET)

# Link object files to create the executable
$(TARGET): $(OBJS)
	$(CC) $(CFLAGS) -o $(TARGET) $(OBJS)

# # Compile source files into object files
%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

# Clean the project (remove object files and the executable)
clean:
	rm -f $(OBJS) $(TARGET)

# Phony targets
.PHONY: all clean
