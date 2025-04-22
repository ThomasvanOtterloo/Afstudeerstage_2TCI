from Transformer.TransformDataController import TransformDataController
from Extract.QueueService import send_test_message
from Extract.WhatsAppApi import WhatsAppApi


class ExtractDataController:
    def __init__(self):
        pass

    def extract_data(self, data):
        print("📩 Controller Data received:", data)

        try:
            # Extract the text body from the first message
            text_body = data["messages"][0]["text"]["body"]
            print("💬 Text body:", text_body)

            # Send just the text body to the queue
            send_test_message(text_body)
            print("📩 Controller Data sent to the queue")

        except (KeyError, IndexError, TypeError) as e:
            print("❌ Failed to extract text body:", e)
