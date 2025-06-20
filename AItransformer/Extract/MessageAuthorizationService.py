from Load import MicrosoftSQLServer, DatabaseConnection


class MessageAuthorizationService:
    def __init__(self, db: DatabaseConnection):
        self.db = db

    def is_valid(self, data):
        # Check if the message already exists in the database
        print(data)
        msg_id = data["messages"][0]["id"]
        group_id = data["messages"][0]["chat_id"]
        # Object van maken, en dan parsen.
        if self.message_exists(msg_id):
            print(f"❌ Message already exists in the database: {msg_id}")
            return False

        # Check if the message is from a whitelisted group
        if not self.message_from_whitelisted_group(group_id):
            print(f"❌ Message from non-whitelisted group: {group_id}")
            return False
        return True

    def message_exists(self, message_id):
        if self.db.get_ad_by_message_id(message_id):
            return True
        return False

    def message_from_whitelisted_group(self, group_id):
        if self.db.get_group_by_id(group_id):
            return True
        return False