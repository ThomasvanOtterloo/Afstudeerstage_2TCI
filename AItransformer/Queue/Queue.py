# Queue.py
import pika


def setup_queues():
    connection = pika.BlockingConnection(pika.ConnectionParameters("localhost"))
    channel = connection.channel()

    # Dead-letter queue
    channel.queue_declare(queue='dead_letter_queue', durable=True)

    # Main queue with dead-letter config
    channel.queue_declare(
        queue='main_queue',
        durable=True,
        arguments={
            'x-dead-letter-exchange': '',  # Default exchange
            'x-dead-letter-routing-key': 'dead_letter_queue'
        }
    )

    print("Queues are set up.")
    connection.close()
