import pika
import json

from numpy.f2py.auxfuncs import throw_error
from ua_parser.loaders import load_data

from Dtos.Ad_Dto import AdDto
from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer
from functools import reduce
from operator import getitem
from Dtos.Trader_Dto import TraderDto
from Dtos.Group_Dto import GroupDto
from Transformer.MessageProcessingService import MessageProcessingService


class TransformDataController:
    def __init__(self, transformer, decorator):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
        self.channel = self.connection.channel()
        self.base = transformer
        self.decorator = decorator
        self.db = LoadDataController()
        self.msg_processing_service = MessageProcessingService()

    def start_listening(self):
        print("[TransformDataController] Listening for messages on 'main_queue'...")

        self.channel.basic_qos(prefetch_count=1)  # Fair dispatch
        self.channel.basic_consume(queue='main_queue', on_message_callback=self.handle_message)

        self.channel.start_consuming()

    def handle_message(self, ch, method, properties, body):

        try:
            print("Received message")
            self.process(body)

            # ✅ Acknowledge successful processing
            ch.basic_ack(delivery_tag=method.delivery_tag)
        except Exception as e:
            print("Error processing message:", str(e))

            # ❌ Reject message so it goes to dead-letter queue
            ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)

    def load_data(self, ad: list):
        self.db.load_data(ad)

    def process(self, message):
        # Your real logic would go here
        print(f"[TransformDataController] Processing payload")
        message = self.msg_processing_service.decode_message(message)

        # Pre-processing:
        ad = self.msg_processing_service.pre_processing(message)

        if ad.text == '':
            print("❌ No text found in the message. Probably a media message.")
            return
        text_output = self.base.transformData(message=ad.text)

        # Decorator processing for the image
        # img_output = ad.image  # use image model if you want to extract the watch from a photo
        # video_output = ad.video  # use video model if needed

        # Post-processing:
        ad = self.msg_processing_service.post_processing(text_output, ad)
        print(f"ad: \n {ad}")
        if ad is False:
            print("❌ post_processing failed.")
        else:
            print("✅ post_processing succeeded.")
            self.load_data(ad)
