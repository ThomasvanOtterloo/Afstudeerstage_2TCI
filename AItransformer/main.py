import time
import threading

from Extract.ExtractDataController import ExtractDataController
from Extract.WhatsAppApi import WhatsAppApi
from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer
from Queue.Queue import setup_queues
from Transformer.BaseTransformerModel import BaseTransformerModel
from Transformer.BrandIdentifierDecorator import BrandIdentifierDecorator
from Transformer.MessageProcessingService import MessageProcessingService
from Transformer.TransformDataController import TransformDataController

if __name__ == "__main__":
    print("🚀 Starting system...")

    # 1) Load & decorate your transformer once
    base_model = BaseTransformerModel()
    identifier = BrandIdentifierDecorator(base_model)
    print("📩 Transformer initialized.")

    # 2) Connect to SQL Server
    db = MicrosoftSQLServer(server="localhost", database="EonWatches")
    db.connect()
    print("✅ Database connected.")

    # 3) Initialize the extractor (webhook handler)
    extractor = ExtractDataController(identifier, db)
    print("✅ ExtractDataController initialized.")

    # 4) Start the Flask webhook server in a background thread
    whatsapp_api = WhatsAppApi()
    whatsapp_api.on_message(extractor.extract_data)
    flask_thread = threading.Thread(target=whatsapp_api.start, daemon=True)
    flask_thread.start()
    print("📩 WhatsApp API initialized and listening on /webhook")

    # 5) Set up RabbitMQ queues
    setup_queues()
    print("✅ Queues are set up.")

    # 6) Initialize the loader and transformer consumer
    load_controller      = LoadDataController(db)
    message_processor    = MessageProcessingService()
    transform_controller = TransformDataController(
        identifier,
        load_controller,
        message_processor
    )
    print("✅ TransformDataController initialized.")

    # 7) Start consuming messages (this will block the main thread)
    transform_controller.start_listening()

    # If you ever need to keep the main thread alive separately:
    # while True:
    #     time.sleep(1)
