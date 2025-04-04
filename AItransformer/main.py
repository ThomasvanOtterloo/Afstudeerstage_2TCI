from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer  # ✅ You need this!

if __name__ == '__main__':
    db = LoadDataController()
    db.load_data(MicrosoftSQLServer())  # ← Don't forget the () to create an instance
   test = 2
