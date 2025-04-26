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
        self.table_name = "AdsTest"
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
            # self.conn = pyodbc.connect(
            #     f'DRIVER=/opt/homebrew/Cellar/msodbcsql18/18.5.1.1/lib/libmsodbcsql.18.dylib;;'
            #     f'SERVER={self.server},1433;'
            #     f'DATABASE={self.database};'
            #     f'UID=sa;'
            #     f'PWD=MyPass@word;'
            #     f'TrustServerCertificate=yes;'
            # ) macos

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

    def write_ad(self, json_data: dict):
        print(f"Attempt to Write ad: {json_data}")
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
        print(f"✅ {len(rows)} row(s) queued")

    def write_trader(self, trader: TraderDto):
        print(f"Attempt to Write trader: {trader}")
        # assume trader *is* a TraderDto, not a dict
        sql = """
        MERGE Traders WITH (HOLDLOCK) AS tgt
        USING (SELECT ? AS Number, ? AS Name) AS src
          ON tgt.Number = src.Number
        WHEN MATCHED THEN
            UPDATE SET Name = src.Name
        WHEN NOT MATCHED THEN
            INSERT (Number, Name) VALUES (src.Number, src.Name);
        """

        try:
            self.cursor.execute(sql, trader.number, trader.name)
        except Exception as e:
            print(f"❌ Error inserting trader: {e}")
            raise

    def write_group(self, group: GroupDto):
        print(f"Attempt to Write group: {group}")

        sql = """
           MERGE Groups WITH (HOLDLOCK) AS tgt
           USING (SELECT ? AS GroupId, ? AS GroupName) AS src
             ON tgt.GroupId = src.GroupId
           WHEN MATCHED THEN
               UPDATE SET GroupName = src.GroupName
           WHEN NOT MATCHED THEN
               INSERT (GroupId, GroupName)
               VALUES (src.GroupId, src.GroupName);
           """
        try:
            self.cursor.execute(sql, group.groupId, group.groupName)
            print("✅ Group upsert succeeded")
        except Exception as e:
            print(f"❌ Error inserting/updating group: {e}")
            raise

    def read(self, query):
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
