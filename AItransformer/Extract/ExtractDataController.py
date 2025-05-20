from Transformer.TransformDataController import TransformDataController
from Extract.QueueService import send_messageQ
from Extract.WhatsAppApi import WhatsAppApi


class ExtractDataController:
    def __init__(self):
        pass

    def extract_data(self, data):  # webhooks send data to this method.
        send_messageQ(data)
        print("📩 ExtractDataController sent data to the queue")
