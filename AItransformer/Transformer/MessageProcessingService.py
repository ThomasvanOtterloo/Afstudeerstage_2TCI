from Dtos.Ad_Dto import AdDto
import json


class MessageProcessingService:
    def __init__(self):
        self.message_queue = []

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

            # Step 3: Enrich every ad
            enriched_list = []
            for ad in parsed_output:
                enriched_ad = {
                    **ad,
                    "MessageId": ad_dto.messageId,
                    "GroupId": ad_dto.groupId,
                    "Image": ad_dto.image,
                    "Video": ad_dto.video,
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
                image=msg.get('image', {}).get('preview', ''),
                video=msg.get('video', {}).get('preview', '')
            )
            return ad
        except Exception as e:
            print("[extract_ad] Error decoding message:", e)
            return None
