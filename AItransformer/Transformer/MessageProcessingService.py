from Dtos.Ad_Dto import AdDto
import json
import os
import requests
from PIL import Image
from urllib.parse import urlparse


class MessageProcessingService:
    def __init__(self):
        self.message_queue = []
        self.baseUrlImage = "https://localhost:7240/Media/"

    def add_message(self, message):
        self.message_queue.append(message)

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
            ad = self.extract_ad(msg)

            return ad
        except Exception as e:
            print("[pre_processing] Error decoding message:", e)
            return '', ''

    def post_processing(self, output: str, ad_dto: AdDto):
        try:
            # Step 1: Clean
            for token in ("<|im_end|>", "<|im_start|>assistant",
                          "<|im_start|>user", "<|im_start|>system"):
                output = output.replace(token, "").strip()

            # Step 2: Parse AI JSON
            parsed_output = json.loads(output)
            print("✅ post_processing: Valid JSON detected. \n", parsed_output)

            # 🚨 Auto-wrap single dicts into a list
            if isinstance(parsed_output, dict):
                parsed_output = [parsed_output]

            if ad_dto.image:
                ad_dto.image = self.download_image_and_return_filename(ad_dto.image, "D:\WatchesImages")  # Save images locally

            # Step 3: Enrich every ad
            enriched_list = []
            for ad in parsed_output:
                enriched_ad = {
                    **ad,
                    "MessageId": ad_dto.messageId,
                    "GroupId": ad_dto.groupId,
                    "Image": ad_dto.image,
                    "Video": ad_dto.video,
                    "PhoneNumber": ad_dto.traderNumber,
                    "TraderName": ad_dto.traderName,
                    "TraderId": 2
                }
                enriched_list.append(enriched_ad)

            return enriched_list

        except json.JSONDecodeError as e:
            print(f"❌ post_processing: Invalid JSON – {e}\nMessage: {output}")
            return False

    def extract_ad(self, msg):
        try:
            # Grab the first message from the list
            ad = AdDto(
                messageId=msg.get('id'),
                groupId=msg.get('chat_id'),
                groupName=msg.get('chat_name'),
                traderNumber=msg.get('from'),
                traderName=msg.get('from_name'),
                text=msg.get('text', {}).get('body', '') or msg.get('image', {}).get('caption', ''),
                image=msg.get('image', {}).get('link', ''),
                video=msg.get('video', {}).get('link', '')
            )
            return ad
        except Exception as e:
            print("[extract_ad] Error decoding message:", e)
            return None

    def download_image_and_return_filename(self, image_url: str, save_dir: str) -> str:
        os.makedirs(save_dir, exist_ok=True)

        try:
            parsed_url = urlparse(image_url)
            filename = os.path.basename(parsed_url.path)
            local_path = os.path.join(save_dir, filename)

            response = requests.get(image_url, timeout=10)
            response.raise_for_status()

            with open(local_path, "wb") as f:
                f.write(response.content)

            self.resize_image_to_720p(local_path)

            return self.baseUrlImage + filename  # or local_path
        except Exception as e:
            print(f"❌ Failed to download image: {e}")
            return None


    def resize_image_to_720p(self, input_path: str):
        try:
            img = Image.open(input_path)

            max_width, max_height = 1280, 720
            if img.width > max_width or img.height > max_height:
                img.thumbnail((max_width, max_height), Image.Resampling.LANCZOS)
                img.save(input_path, format="JPEG", quality=90)
                # print(f"✅ Resized image to max 720p: {input_path}")
            else:
                print(f"ℹ️ Image is already 720p or smaller: {input_path}")
            return img

        except Exception as e:
            print(f"❌ Failed to resize image: {e}")

