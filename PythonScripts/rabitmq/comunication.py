import pika

from PythonScripts.rabitmq.controller import Controller


def start():
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
    channel = connection.channel()
    channel.queue_declare(queue='front')
    channel.queue_declare(queue='back')

    def callback(ch, method, properties, body):
        c = Controller.get_instance()
        p, b = c.prepare_message(body)
        print(p)
        print(*b)
        isDone = c.caller(p, b)
        print(isDone)
        channel.basic_publish(exchange='',
                              routing_key='back',
                              body=str(isDone))

    channel.basic_consume(queue='front', on_message_callback=callback, auto_ack=True)
    channel.start_consuming()
