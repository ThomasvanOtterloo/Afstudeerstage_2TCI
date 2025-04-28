class MessageAuthorizationService:
    """
    Service for authorizing messages.
    """

    def __init__(self, database_controller=None):
        DatabaseController = None

    def authorize_message(self, message_id, user_id):


        return True

    def is_authorized(self, user, message):
        """
        Check if a user is authorized to access a message.
        """
        # Implement your authorization logic here
        return True

    def getWhitelistedGroups(self):
        # get whitelisted groups from the database
        # return a list of group IDs
        return ["groupId': '120363416829988594@g.us", "67890"]

    def match_group(self, group_id):
        # check if groupId is in the whitelist
        # return True or False
        whitelisted_groups = self.getWhitelistedGroups()
        return group_id in whitelisted_groups