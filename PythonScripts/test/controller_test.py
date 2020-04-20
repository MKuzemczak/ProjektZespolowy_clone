import unittest

import pika

from PythonScripts.rabitmq.controller import Controller, Executor


class ControllerTest(unittest.TestCase):
    def test_controller_with_rabit(self):
        path = 'D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db'
        connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
        channel = connection.channel()
        channel.queue_declare(queue='front')
        channel.queue_declare(queue='back')
        channel.basic_publish(exchange='',
                              routing_key='front',
                              body='PATH ' + path)
        channel.basic_publish(exchange='',
                              routing_key='front',
                              body='COMPARE 4 1 2 3')

        connection.close()

        Executor.start_messaging()

    def test_back_queque(self):
        connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
        channel = connection.channel()
        channel.queue_declare(queue='back')
        channel.queue_declare(queue='front')

        def callback(ch, method, properties, body):
            print(body)
            print(method)
            print(properties)

        channel.basic_consume(queue='back', on_message_callback=callback, auto_ack=True)
        channel.start_consuming()
