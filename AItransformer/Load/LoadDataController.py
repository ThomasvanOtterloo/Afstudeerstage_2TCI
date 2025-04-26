from Dtos.Group_Dto import GroupDto
from Dtos.Trader_Dto import TraderDto
from Load.DatabaseConnection import IDatabaseConnection
from Load.MicrosoftSQLServer import MicrosoftSQLServer


class LoadDataController:

    def __init__(self):
        self.database = MicrosoftSQLServer()
        self.database.connect()
        pass

    def database_connection(self, database: IDatabaseConnection):
        self.database = database

    def load_data(self, ad: dict, trader: TraderDto, group: GroupDto):
        try:
            self.database.write_ad(ad)
            self.database.write_trader(trader)
            self.database.write_group(group)
            self.end_transaction()  # ← commits once
        except Exception as e:
            print(f"❌ load_data failed, rolling back: {e}")
            self.database.conn.rollback()  # ← undo everything
            raise

    def end_transaction(self):
        self.database.commit()

    def close_connection(self):
        self.database.close()
