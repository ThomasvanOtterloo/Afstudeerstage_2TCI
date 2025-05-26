import time

from Extract.ExtractDataController import ExtractDataController
from Extract.WhatsAppApi import WhatsAppApi
from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer
from Queue.Queue import setup_queues
from Extract.QueueService import send_messageQ
import pyodbc

from Transformer.BaseTransformerModel import BaseTransformerModel
from Transformer.BrandIdentifierDecorator import BrandIdentifierDecorator
from Transformer.ITransformer import ITransformer
# from Extract.ExtractDataController import ExtractDataController
from Transformer.TransformDataController import TransformDataController
# from Extract.WhatsAppApi import WhatsAppApi
import requests
from flask import Flask, request, jsonify

app = Flask(__name__)

if __name__ == '__main__':
    print("🚀 Starting system...")
    transformer = BaseTransformerModel()
    brand_identifier = BrandIdentifierDecorator(transformer)
    print("📩 Transformer initialized.")

    extractor = ExtractDataController()
    print("📩 Extractor initialized.")

    server = WhatsAppApi()
    print("📩 WhatsApp API initialized.")
    server.on_message(extractor.extract_data)
    server.start()

    setup_queues()

    # Start the TransformDataController to listen for messages
    transform_controller = TransformDataController()
    transform_controller.start_listening()

    print("🚀 System running. Waiting for webhook events...")
    while True:
        time.sleep(1)
