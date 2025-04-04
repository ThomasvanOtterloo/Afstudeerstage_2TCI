import pyodbc
from Load.DatabaseConnection import IDatabaseConnection


class MicrosoftSQLServer(IDatabaseConnection):
    def __init__(self, server='localhost', database='EonWatches'):
        self.server = server
        self.database = database
        self.conn = None
        self.cursor = None

    def connect(self):
        try:
            self.conn = pyodbc.connect(
                f'DRIVER={{ODBC Driver 17 for SQL Server}};'
                f'SERVER={self.server};'
                f'DATABASE={self.database};'
                f'Trusted_Connection=yes;'
            )
            self.cursor = self.conn.cursor()
            print("✅ Connected to SQL Server")
        except Exception as e:
            print(f"❌ Connection failed: {e}")

    def dummy_write(self):
        try:
            # Insert a dummy row
            self.cursor.execute("INSERT INTO DummyTable (Name) VALUES (?)", ('TestName',))
            self.conn.commit()
            print("✅ Dummy row inserted")
        except Exception as e:
            print(f"❌ Failed to write: {e}")

    def close(self):
        if self.cursor:
            self.cursor.close()
        if self.conn:
            self.conn.close()
            print("🔌 Connection closed")
