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


class TransformDataController:
    def __init__(self, transformer, decorator):
        self.connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
        self.channel = self.connection.channel()
        self.base = transformer
        self.decorator = decorator
        self.db = LoadDataController()

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

    def load_data(self, ad: dict, trader: dict, group: dict):
        self.db.load_data(ad, trader, group)

    def process(self, message):
        # Your real logic would go here
        print(f"[TransformDataController] Processing payload")
        message = self.decode_message(message)

        # Pre-processing:
        ad, trader, group = self.pre_processing(message)

        # transformer processing:
        text_output = self.base.transformData(message=ad.text)

        # Decorator processing for the image
        img_output = ad.image  # use image model if you want to extract the watch from a photo
        video_output = ad.video  # use video model if needed

        # Post-processing:
        ad = self.post_processing(text_output, ad)
        print(f"ad: \n {ad},\n trader:\n {trader} \n group:\n {group}")
        if ad is False:
            print("❌ post_processing failed.")
            return False
        else:
            print("✅ post_processing succeeded.")
            self.load_data(ad, trader, group)

    def decode_message(self, message):
        try:
            decoded = json.loads(message.decode("utf-8"))
            return decoded
        except json.JSONDecodeError as e:
            print("[decode_message] Error decoding message:", e)
            return {}

    def pre_processing(self, message):
        try:
            # Grab the first message from the list
            msg = message.get('messages', [{}])[0]

            trader = self.extract_user(msg)
            ad = self.extract_ad(msg)
            group = self.extract_group(msg)

            return ad, trader, group
        except Exception as e:
            print("[pre_processing] Error decoding message:", e)
            return '', ''

    def extract_group(self, msg):
        try:
            # Grab the first message from the list
            group = GroupDto(
                groupId=msg.get('chat_id'),
                groupName=msg.get('chat_name'),
            )
            return group
        except Exception as e:
            print("[extract_group] Error decoding message:", e)
            return None
    def extract_ad(self, msg):
        try:
            # Grab the first message from the list
            ad = AdDto(
                messageId=msg.get('id'),
                groupId=msg.get('chat_id'),
                groupName=msg.get('chat_name'),
                text=msg.get('text', {}).get('body') or msg.get('image', {}).get('caption', ''),
                image=msg.get('image', {}).get('preview', ''),
                video=msg.get('video', {}).get('preview', '')
            )
            return ad
        except Exception as e:
            print("[extract_ad] Error decoding message:", e)
            return None

    def extract_user(self, msg):
        try:
            # Grab the first message from the list
            user = TraderDto(
                number=msg.get('from'),
                name=msg.get('from_name'),
            )
            return user
        except Exception as e:
            print("[extract_user] Error decoding message:", e)
            return None

    def post_processing(self, output: str, ad_dto: AdDto):
        try:
            # Step 1: Clean
            for token in ("<|im_end|>", "<|im_start|>assistant",
                          "<|im_start|>user", "<|im_start|>system"):
                output = output.replace(token, "").strip()

            # Step 2: Parse AI JSON
            parsed_output = json.loads(output)
            print("✅ post_processing: Valid JSON detected.")

            # Step 3: Attach metadata
            enriched_output = {
                **parsed_output,  # unpack all AI-inferred fields
                "messageId": ad_dto.messageId,
                "groupId": ad_dto.groupId,
                "image": ad_dto.image,
                "video": ad_dto.video,
            }

            return enriched_output

        except json.JSONDecodeError as e:
            print(f"❌ post_processing: Invalid JSON – {e}\nMessage: {output}")
            return False

