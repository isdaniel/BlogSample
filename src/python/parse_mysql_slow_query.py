import re
import csv
import argparse

# Set up argument parsing
parser = argparse.ArgumentParser(description="Parse MySQL slow query log to CSV.")
parser.add_argument("log_file_path", type=str, nargs="+", help="Path to the MySQL slow query log file.")
parser.add_argument("--output_csv", type=str, default="parsed_slow_queries.csv", help="Output CSV file name (default: parsed_slow_queries.csv).")
args = parser.parse_args()

# Use parsed arguments
log_file_paths = args.log_file_path
csv_file_path = args.output_csv
# User@Host: alfresco[alfresco] @  [10.129.9.135]  Id: 75031
# Regular expressions to capture relevant log details
time_pattern = re.compile(r"#\s*Time\s*:\s*([\d\-TZ:]+)")
user_host_pattern = re.compile(r"#\s*User@Host\s*:\s*(.*?)\s*\[.*?\]\s*@\s*\[(.*?)\]\s*Id\s*:\s*(.*\d)")
query_stats_pattern = re.compile(r"#\s*Query_time\s*:\s*([\d.]+)\s*Lock_time\s*:\s*([\d.]+)\s*Rows_sent\s*:\s*(\d+)\s*Rows_examined\s*:\s*(\d+)")
query_start_pattern = re.compile(r"^(\/\*|SELECT|INSERT|UPDATE|DELETE).*", re.IGNORECASE)

# Open the CSV file for writing
with open(csv_file_path, mode="w", newline="") as csv_file:
    csv_writer = csv.writer(csv_file)
    # Write header
    csv_writer.writerow([
        "Time", "User", "Host","Id", "Query_Time", "Lock_Time", "Rows_Sent", "Rows_Examined", "Query"
    ])
    for log_file_path in log_file_paths:
        # Read and parse the log file
        with open(log_file_path, "r", encoding='utf-8') as log_file:
            current_record = {}
            current_query = []
            for line in log_file:
                # Check if the line is a new timestamp
                time_match = time_pattern.match(line)
                if time_match:
                    # If a query was previously collected, write it to the CSV
                    if current_record and current_query:
                        current_record["Query"] = " ".join(current_query).strip()
                        csv_writer.writerow([
                            current_record.get("Time"),
                            current_record.get("User"),
                            current_record.get("Host"),
                            current_record.get("Id"),
                            current_record.get("Query Time"),
                            current_record.get("Lock Time"),
                            current_record.get("Rows Sent"),
                            current_record.get("Rows Examined"),
                            current_record.get("Query")
                        ])
                        # Clear for the next query
                        current_record = {}
                        current_query = []

                    # Start a new record with the current time
                    current_record["Time"] = time_match.group(1)

                # Match user and host details
                user_host_match = user_host_pattern.match(line)
                if user_host_match:
                    current_record["User"] = user_host_match.group(1)
                    current_record["Host"] = user_host_match.group(2)
                    current_record["Id"] = user_host_match.group(3)

                # Match query stats (Query_time, Lock_time, Rows_sent, Rows_examined)
                query_stats_match = query_stats_pattern.match(line)
                if query_stats_match:
                    current_record["Query Time"] = query_stats_match.group(1)
                    current_record["Lock Time"] = query_stats_match.group(2)
                    current_record["Rows Sent"] = query_stats_match.group(3)
                    current_record["Rows Examined"] = query_stats_match.group(4)

                # Collect query lines until the next timestamp
                if query_start_pattern.match(line) or current_query:
                    current_query.append(line.strip())

            # Write the last collected query to the CSV (if any)
            if current_record and current_query:
                current_record["Query"] = " ".join(current_query).strip()
                csv_writer.writerow([
                    current_record.get("Time"),
                    current_record.get("User"),
                    current_record.get("Host"),
                    current_record.get("Id"),
                    current_record.get("Query Time"),
                    current_record.get("Lock Time"),
                    current_record.get("Rows Sent"),
                    current_record.get("Rows Examined"),
                    current_record.get("Query")
                ])

print(f"Parsed slow queries have been saved to {csv_file_path}")
