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

    def load_data(self, ad: list):
        try:
            self.database.write_ad(ad)
            self.end_transaction()  # commit all changes
        except Exception as e:
            self.database.conn.rollback()  # rollback everything
            raise

    def end_transaction(self):
        self.database.commit()

    def close_connection(self):
        self.database.close()
