import pika
import json

def send_test_message(data):
    connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
    channel = connection.channel()

    channel.basic_publish(
        exchange='',
        routing_key='main_queue',
        body=json.dumps(data),
        properties=pika.BasicProperties(delivery_mode=2)  # Make message persistent
    )

    print("Sent test message to main_queue.")
    connection.close()
