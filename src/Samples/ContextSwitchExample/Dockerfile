FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
COPY ./ /sourceCode
WORKDIR /sourceCode
RUN dotnet build --configuration Release
RUN mkdir -p /app && \
    cp ./WorkerProcess/* /app -R

FROM mcr.microsoft.com/dotnet/aspnet:6.0 as server
RUN apt-get update && \
    apt-get install procps -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app .
CMD ["sh", "-c", " exec dotnet ProcessorLab.dll"]
