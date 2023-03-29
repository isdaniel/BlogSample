#include <stdio.h>
#include <stdlib.h>
#include "libpq-fe.h"

int main() {
    printf("Postgresql version usage %d\n",PQlibVersion());
    // Connect to the database
    PGconn *conn = PQconnectdb("user=postgres password=test.123 dbname=postgres host=dd-pg-fs-13.postgres.database.azure.com port=6432");
    if (PQstatus(conn) != CONNECTION_OK) {
        fprintf(stderr, "Connection to database failed: %s\n", PQerrorMessage(conn));
        PQfinish(conn);
        exit(1);
    }

    // Execute a simple query
    PGresult *res = PQexec(conn, "SELECT 'Hello world!!';");
    if (PQresultStatus(res) != PGRES_TUPLES_OK) {
        fprintf(stderr, "Query execution failed: %s\n", PQresultErrorMessage(res));
        PQclear(res);
        PQfinish(conn);
        exit(1);
    }

    // Print the query results
    int nfields = PQnfields(res);
    int nrecords = PQntuples(res);
    printf("Query returned %d records with %d fields:\n", nrecords, nfields);
    for (int i = 0; i < nrecords; i++) {
        for (int j = 0; j < nfields; j++) {
            printf("%s\t", PQgetvalue(res, i, j));
        }
        printf("\n");
    }

    PQclear(res);
    PQfinish(conn);

    return 0;
}
