from Transformer.TransformDataController import TransformDataController
from Extract.QueueService import send_test_message
from Extract.WhatsAppApi import WhatsAppApi


class ExtractDataController:
    def __init__(self):
        pass

    def extract_data(self, data):
        # readableGroups = this.getWhitelistedGroups()
        # check for blocked users
        # if not self.match_group(data["groupId"]):

        # only send message to queue if groupId is in whitelist
        send_test_message(data)
        print("📩 ExtractDataController sent data to the queue")


