import pika
import json

from numpy.f2py.auxfuncs import throw_error

from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer
from BaseTransformerModel import BaseTransformerModel

class TransformDataController:
    def __init__(self, transformer):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
        self.channel = self.connection.channel()
        self.base = transformer
        self.decorator = None



    def start_listening(self):
        print("[TransformDataController] Listening for messages on 'main_queue'...")

        self.channel.basic_qos(prefetch_count=1)  # Fair dispatch
        self.channel.basic_consume(queue='main_queue', on_message_callback=self.handle_message)

        self.channel.start_consuming()

    def handle_message(self, ch, method, properties, body):
        try:
            print("Received message:", body)

            # 👉 Place your transformation logic here
            newJSON = self.process(body)

            # ✅ Acknowledge successful processing
            ch.basic_ack(delivery_tag=method.delivery_tag)

            print("Message validation passed.")
            db = LoadDataController()
            db.load_data(MicrosoftSQLServer(), body)

        except Exception as e:
            print("Error processing message:", str(e))

            # ❌ Reject message so it goes to dead-letter queue
            ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)

    def process(self, message):
        # Your real logic would go here
        print(f"[TransformDataController] Processing payload: {message}")

        #transformer processing:
        json_ad = self.transformer.transformData(message)

        # Apply logic for validating the message
        if not self.validate_json(json_ad):
            raise ValueError("Invalid message format")

        return json_ad




    def validate_json(self, message):
        if not isinstance(message, str):
            return False
        try:
            json.loads(message)
            return True
        except json.JSONDecodeError:
            return False