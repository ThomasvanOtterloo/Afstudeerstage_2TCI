from Load.DatabaseConnection import IDatabaseConnection


class LoadDataController:

    @staticmethod
    def load_data(database: IDatabaseConnection):
        database.connect()
        database.dummy_write()
        database.close()
        print("✅ Data loaded successfully")
