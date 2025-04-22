import pika
import json

def callback(ch, method, properties, body):
    try:
        data = json.loads(body)
        print("Processing:", data)

        # Simulate failure
        if data['id'] == 123:
            raise Exception("DB is down or validation failed")

        # If successful
        ch.basic_ack(delivery_tag=method.delivery_tag)

    except Exception as e:
        print("Error:", e)
        # Reject and don't requeue so it goes to dead letter
        ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)

connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
channel = connection.channel()

channel.basic_consume(queue='main_queue', on_message_callback=callback)

print("Waiting for messages...")
channel.start_consuming()
