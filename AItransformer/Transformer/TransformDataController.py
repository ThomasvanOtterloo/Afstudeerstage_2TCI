import pika
import json

from numpy.f2py.auxfuncs import throw_error

from Load.LoadDataController import LoadDataController
from Load.MicrosoftSQLServer import MicrosoftSQLServer

class TransformDataController:
    def __init__(self):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
        self.channel = self.connection.channel()


    def start_listening(self):
        print("[TransformDataController] Listening for messages on 'main_queue'...")

        self.channel.basic_qos(prefetch_count=1)  # Fair dispatch
        self.channel.basic_consume(queue='main_queue', on_message_callback=self.handle_message)

        self.channel.start_consuming()

    def handle_message(self, ch, method, properties, body):
        try:
            message = json.loads(body)
            print("Received message:", message)

            # 👉 Place your transformation logic here
            self.process(message)

            # ✅ Acknowledge successful processing
            ch.basic_ack(delivery_tag=method.delivery_tag)

        except Exception as e:
            print("Error processing message:", str(e))

            # ❌ Reject message so it goes to dead-letter queue
            ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)

    def process(self, message):
        # Your real logic would go here
        print(f"[TransformDataController] Processing payload: {message}")

        # Simulate processing time
        import time
        time.sleep(5)
        print(f"[TransformDataController] Finished processing payload: {message}")

        # Apply logic for validating the message
        if not self.validate_message(message):
            raise ValueError("Invalid message format")

        print("Message validation passed.")
        db = LoadDataController()
        db.load_data(MicrosoftSQLServer(), message)


    def validate_message(self, message):
        if not isinstance(message, str):
            return False
        try:
            json.loads(message)
            return True
        except json.JSONDecodeError:
            return False