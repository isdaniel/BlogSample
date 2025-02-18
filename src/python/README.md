# MySQL Slow Query Log Parser

This Python script parses MySQL slow query logs and extracts relevant details into a structured CSV file. It helps analyze slow queries by capturing metadata such as execution time, user, host, and query details.

## Features

- Extracts timestamps, user and host information, query execution times, and affected rows.
- Parses multiple slow query log files.
- Saves the extracted data to a CSV file for further analysis.

## Requirements

Ensure you have Python installed (Python 3.x recommended).

## Installation

No additional libraries are required beyond Python's standard library.

## Usage

Run the script from the command line with the path to the MySQL slow query log file(s) as an argument:

```sh
python slow_query_parser.py /path/to/slow_query.log /path/to/slow_query1.log /path/to/slow_query2.log
```

### Options:

- `log_file_path` (required): Path(s) to one or more MySQL slow query log files.
- `--output_csv` (optional): Output CSV file name (default: `parsed_slow_queries.csv`).

Example:

```sh
python slow_query_parser.py /path/to/slow_query.log /path/to/slow_query1.log /path/to/slow_query2.log --output_csv results.csv
```

## Output

The script generates a CSV file containing the following columns:

- **Time**: The timestamp of the query.
- **User**: The MySQL user executing the query.
- **Host**: The client host from which the query originated.
- **Id**: The MySQL thread ID associated with the query.
- **Query\_Time**: The total execution time of the query.
- **Lock\_Time**: The time the query was waiting for locks.
- **Rows\_Sent**: The number of rows sent as a result.
- **Rows\_Examined**: The number of rows examined during execution.
- **Query**: The actual SQL query executed.

## Example Output

If the log contains the following entry:

```
# Time: 2025-02-18T12:34:56
# User@Host: db_user[db_user] @ [192.168.1.10] Id: 12345
# Query_time: 1.234  Lock_time: 0.001  Rows_sent: 10  Rows_examined: 100
SELECT * FROM users WHERE id = 1;
```

The resulting CSV (`parsed_slow_queries.csv`) will contain:

```csv
Time,User,Host,Id,Query_Time,Lock_Time,Rows_Sent,Rows_Examined,Query
2025-02-18T12:34:56,db_user,192.168.1.10,12345,1.234,0.001,10,100,"SELECT * FROM users WHERE id = 1;"
```

## Notes

- The script processes each log file sequentially.
- The output CSV file will be overwritten if it already exists.
- Queries spanning multiple lines are merged into a single entry.

## Author

Daniel Shih

