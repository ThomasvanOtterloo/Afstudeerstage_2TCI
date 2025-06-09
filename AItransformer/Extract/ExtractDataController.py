from Transformer.TransformDataController import TransformDataController
from Extract.QueueService import send_messageQ
from Extract.WhatsAppApi import WhatsAppApi
from Extract.MessageAuthorizationService import MessageAuthorizationService
from Load.MicrosoftSQLServer import MicrosoftSQLServer
from Transformer.BrandIdentifierDecorator import BrandIdentifierDecorator


class ExtractDataController:
    def __init__(
        self,
        identifier: BrandIdentifierDecorator,
        db: MicrosoftSQLServer
    ):
        # shared transformer decorator
        self.base = identifier

        # database access for auth & de-duplication
        self.db = db
        self.db.connect()  # ← ensure cursor is created
        self.authorization_service = MessageAuthorizationService(self.db)

        # WhatsApp webhook listener
        self.whatsapp_api = WhatsAppApi()

        print("✅ ExtractDataController initialized")

    def extract_data(self, data):  # webhooks send data to this method.
        if not self.authorization_service.is_valid(data):
            return  # already processed, skip

        send_messageQ(data)
        print("📩 ExtractDataController sent data to the queue")


