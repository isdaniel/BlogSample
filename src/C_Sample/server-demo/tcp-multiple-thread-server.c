#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netinet/ip.h>
#include <pthread.h>

#define size_httpthread 10
#define quit "quit"
typedef struct 
{
    int server_ad;
    pthread_t tid;
} http_Context;

void* http_listen(void* args){
    http_Context* context = (http_Context*)args;
    char request[128];

    while (1)
    {
        memset(request,0,sizeof(request));
        int recv_len = recv(context->server_ad, request, sizeof(request), 0);
        if (recv_len < 0)
        {
            perror("recv error");
            break;
        }

        if (strncmp(request,quit , 4) == 0)
        {
            printf("client leave from this connection!!\n");
            break;
        }

        printf("from client %s \n",request);

        char *response = "HTTP/1.1 200 OK\nContent-Type: text/html\n\n<html><body><h1>Hello, World!</h1></body></html>\n";
        send(context->server_ad, response, strlen(response), 0);
    }
    close(context->server_ad);
    return NULL;
}

int main(void) {
    char buf[100];
    int server_fd, client_fd;
    http_Context http_contexts[size_httpthread];// = malloc(size_httpthread * sizeof(http_Context));
    
    int i = 0;
    struct sockaddr_in server_addr, client_addr;
    socklen_t client_addr_len;

    // 建立網路套接字
    if ((server_fd = socket(AF_INET, SOCK_STREAM, 0)) < 0)
    {
        perror("socket error");
        exit(1);
    }

    // 設定伺服器地址
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sin_family = AF_INET;
    server_addr.sin_addr.s_addr = htonl(INADDR_ANY);
    server_addr.sin_port = htons(8000);

    // 綁定地址和端口
    if (bind(server_fd, (struct sockaddr *)&server_addr, sizeof(server_addr)) < 0)
    {
        perror("bind error");
        exit(1);
    }

    // 設置監聽
    if (listen(server_fd, 5) < 0)
    {
        perror("listen error");
        exit(1);
    }

    while (1)
    {
        //memset(buf,0,sizeof(buf));
        // 接受客戶端連接
        client_addr_len = sizeof(client_addr);
        
        if ((http_contexts[i].server_ad = accept(server_fd, (struct sockaddr *)&client_addr, &client_addr_len)) < 0)
        {
            perror("accept error");
            close(server_fd);
            continue;
        }

        //create a thread to listen 
        pthread_create(&http_contexts[i].tid,NULL,http_listen,&http_contexts[i]);

        i++;
    }

    //free(http_contexts);
}