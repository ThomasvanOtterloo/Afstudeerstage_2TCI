from Load.DatabaseConnection import IDatabaseConnection


class LoadDataController:

    def __init__(self):
        pass

    def load_data(self, database: IDatabaseConnection, data=None):
        database.connect()
        database.write(data)
        database.close()
        print("✅ Data loaded successfully")
