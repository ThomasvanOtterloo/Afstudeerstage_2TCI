import pika
import json

from numpy.f2py.auxfuncs import throw_error
from ua_parser.loaders import load_data
from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer

class TransformDataController:
    def __init__(self, transformer, decorator):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
        self.channel = self.connection.channel()
        self.base = transformer
        self.decorator = decorator

    def start_listening(self):
        print("[TransformDataController] Listening for messages on 'main_queue'...")

        self.channel.basic_qos(prefetch_count=1)  # Fair dispatch
        self.channel.basic_consume(queue='main_queue', on_message_callback=self.handle_message)

        self.channel.start_consuming()

    def handle_message(self, ch, method, properties, body):
        try:
            print("Received message:", body)
            newJSON = self.process(body)

            # ✅ Acknowledge successful processing
            ch.basic_ack(delivery_tag=method.delivery_tag)

            load_data(newJSON)
        except Exception as e:
            print("Error processing message:", str(e))

            # ❌ Reject message so it goes to dead-letter queue
            ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)

    def load_data(self, message):
        db = LoadDataController()
        db.load_data(MicrosoftSQLServer(), message)

    def process(self, message):
        # Your real logic would go here
        print(f"[TransformDataController] Processing payload: {message}")

        # Pre-processing:
        pre_processing_result = self.pre_processing(message)

        # based on pro-processing result, pick transformer or decorator
        # logic here.

        #transformer processing:
        json_ad = self.base.transformData(pre_processing_result)

        post_processing_result = self.post_processing(json_ad)
        return post_processing_result

    def pre_processing(self, message):
        if not isinstance(message, str):
            return False
        try:
            json.loads(message)
            return True
        except json.JSONDecodeError:
            return False


    def post_processing(self, message):
        if not isinstance(message, str):
            return False
        try:
            json.loads(message)
            return True
        except json.JSONDecodeError:
            return False