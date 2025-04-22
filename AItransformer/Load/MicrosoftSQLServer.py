import pyodbc
from Load.DatabaseConnection import IDatabaseConnection
from sqlalchemy import create_engine
import json


class MicrosoftSQLServer(IDatabaseConnection):
    def __init__(self, server='localhost', database='EonWatches'):
        self.server = server
        self.database = database
        self.conn = None
        self.cursor = None
        self.table_name = "Ads"
        self.table_columns = None

    def get_table_columns(self):
        result = self.cursor.execute(
            f"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{self.table_name}'"
        )
        columns = [row[0] for row in result]
        print("Columns:" + columns.__str__())
        return columns


    def connect(self):
        try:
            self.conn = pyodbc.connect(
                f'DRIVER=/opt/homebrew/Cellar/msodbcsql18/18.5.1.1/lib/libmsodbcsql.18.dylib;;'
                f'SERVER={self.server},1433;'
                f'DATABASE={self.database};'
                f'UID=sa;'
                f'PWD=MyPass@word;'
                f'TrustServerCertificate=yes;'
            )
            self.cursor = self.conn.cursor()
            self.table_columns = self.get_table_columns()
            print("✅ Connected to SQL Server")
        except Exception as e:
            print(f"❌ Connection failed: {e}")

    def write(self, json_data):
        try:
            print(json_data)

            # Handle single dict or list of dicts
            rows = json_data if isinstance(json_data, list) else [json_data]

            for row in rows:
                known = {}
                unknown = {}

                for k, v in row.items():
                    if k in self.table_columns and k != "Other":
                        known[k] = json.dumps(v) if isinstance(v, dict) else v
                    elif k not in self.table_columns:
                        unknown[k] = v

                # Handle 'Other' field
                if "Other" in row:
                    try:
                        # Merge existing 'Other' with unknown keys
                        existing_other = row["Other"] if isinstance(row["Other"], dict) else json.loads(row["Other"])
                        unknown = {**existing_other, **unknown}
                    except Exception as e:
                        print(f"⚠️ Failed to parse 'Other' JSON: {e}")

                if "Other" in self.table_columns:
                    known["Other"] = json.dumps(unknown)

                columns = ", ".join(known.keys())
                placeholders = ", ".join(["?"] * len(known))
                values = list(known.values())

                query = f"INSERT INTO {self.table_name} ({columns}) VALUES ({placeholders})"
                self.cursor.execute(query, values)

            self.conn.commit()
            print(f"✅ {len(rows)} row(s) inserted")

        except Exception as e:
            print(f"❌ Failed to write: {e}")

    def close(self):
        if self.cursor:
            self.cursor.close()
        if self.conn:
            self.conn.close()
            print("🔌 Connection closed")
