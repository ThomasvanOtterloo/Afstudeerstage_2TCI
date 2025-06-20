import pyodbc

from Dtos.Group_Dto import GroupDto
from Dtos.Trader_Dto import TraderDto
from Load.DatabaseConnection import IDatabaseConnection
from sqlalchemy import create_engine
import json


class MicrosoftSQLServer(IDatabaseConnection):
    def __init__(self, server='localhost', database='EonWatches'):
        self.server = server
        self.database = database
        self.conn = None
        self.cursor = None
        self.table_name_ads = "Ads"
        self.table_name_whitelisted_groups = "WhitelistedGroups"
        self.table_columns = None

    def get_table_columns(self):
        result = self.cursor.execute(
            f"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{self.table_name_ads}'"
        )
        columns = [row[0] for row in result]
        print("Columns:" + columns.__str__())
        return columns

    def connect(self):
        try:
            self.conn = pyodbc.connect(
                f"DRIVER={{ODBC Driver 18 for SQL Server}};"
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

    def write_ad(self, json_data: list):
        print(f"Attempt to Write ad(s): {json_data}")

        # Make sure json_data is a list
        rows = json_data if isinstance(json_data, list) else [json_data]

        for row in rows:
            known = {}
            unknown = {}

            # Step 1: Separate known and unknown columns
            for k, v in row.items():
                if k in self.table_columns and k != "Other":
                    known[k] = json.dumps(v) if isinstance(v, dict) else v
                elif k not in self.table_columns:
                    unknown[k] = v

            # Step 2: Merge unknown fields into 'Other'
            if "Other" in self.table_columns:
                # Try to preserve existing 'Other' field if present
                existing_other = {}
                if "Other" in row:
                    try:
                        existing_other = row["Other"] if isinstance(row["Other"], dict) else json.loads(row["Other"])
                    except Exception as e:
                        print(f"⚠️ Failed to parse 'Other' JSON: {e}")
                combined_other = {**existing_other, **unknown}
                known["Other"] = json.dumps(combined_other)

            # Step 3: Prepare SQL Insert
            columns = ", ".join(known.keys())
            placeholders = ", ".join(["?"] * len(known))
            values = list(known.values())

            query = f"INSERT INTO {self.table_name_ads} ({columns}) VALUES ({placeholders})"

            try:
                self.cursor.execute(query, values)
                print(f"✅ {len(rows)} row(s) queued for insertion.")

            except Exception as e:
                print(f"❌ Failed to insert row: {e}\nRow data: {row}")


    def get_ad_by_message_id(self, message_id) -> bool:
        if self.cursor is None:
            raise RuntimeError("DB cursor not initialized; did you call connect()?")
        query = (
            f"SELECT TOP 1 1 "
            f"FROM {self.table_name_ads} "
            f"WHERE MessageId = ?"
        )
        self.cursor.execute(query, (message_id,))
        return self.cursor.fetchone() is not None

    def get_group_by_id(self, group_id) -> bool:
        if self.cursor is None:
            raise RuntimeError("DB cursor not initialized; did you call connect()?")
        query = (
            f"SELECT TOP 1 1 "
            f"FROM {self.table_name_whitelisted_groups} "
            f"WHERE Id = ?"
        )
        self.cursor.execute(query, (group_id,))
        return self.cursor.fetchone() is not None

    def getChannelId(self, query):
        pass

    def close(self):
        if self.cursor:
            self.cursor.close()
        if self.conn:
            self.conn.close()
            print("🔌 Connection closed")

    def commit(self):
        if self.conn:
            self.conn.commit()
            print("✅ Changes committed")
