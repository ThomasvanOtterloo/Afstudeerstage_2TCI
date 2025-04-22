import time

from Extract.ExtractDataController import ExtractDataController
from Extract.WhatsAppApi import WhatsAppApi
from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer
from Queue.Queue import setup_queues
from Extract.QueueService import send_test_message
import pyodbc
# from Extract.ExtractDataController import ExtractDataController
from Transformer.TransformDataController import TransformDataController
# from Extract.WhatsAppApi import WhatsAppApi
import requests
from flask import Flask, request, jsonify
app = Flask(__name__)

if __name__ == '__main__':
    extractor = ExtractDataController()


    server = WhatsAppApi()
    server.on_message(extractor.extract_data)
    server.start()

    setup_queues()


    # Start the TransformDataController to listen for messages
    transform_controller = TransformDataController()
    transform_controller.start_listening()


    print("🚀 System running. Waiting for webhook events...")
    while True:
        time.sleep(1)







