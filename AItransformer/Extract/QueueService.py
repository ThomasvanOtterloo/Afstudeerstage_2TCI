import pika
import json

def send_messageQ(data):
    connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
    channel = connection.channel()

    # Convert dict to JSON string, then encode to bytes
    json_data = json.dumps(data).encode('utf-8')

    channel.basic_publish(
        exchange='',
        routing_key='main_queue',
        body=json_data,
        properties=pika.BasicProperties(delivery_mode=2)  # Make message persistent
    )

    print("Sent test message to main_queue.")
    connection.close()
