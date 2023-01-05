#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netinet/ip.h>

int main(int argc, char *argv[])
{
    int server_fd, client_fd;
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
        // 接受客戶端連接
        client_addr_len = sizeof(client_addr);
        if ((client_fd = accept(server_fd, (struct sockaddr *)&client_addr, &client_addr_len)) < 0)
        {
            perror("accept error");
            continue;
        }

        // 接收客戶端請求並回傳回應
        char request[1024];
        int recv_len = recv(client_fd, request, sizeof(request), 0);
        if (recv_len < 0)
        {
            perror("recv error");
            close(client_fd);
            continue;
        }

        // 解析 HTTP 請求並發送回應
        char *response = "HTTP/1.1 200 OK\nContent-Type: text/html\n\n<html><body><h1>Hello, World!</h1></body></html>\n";
        send(client_fd, response, strlen(response), 0);
        close(client_fd);
    }
}