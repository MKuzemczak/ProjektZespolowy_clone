import unittest

import pika

from PythonScripts.rabitmq.controller import Controller, Executor


class ControllerTest(unittest.TestCase):
    def test_controller_without_rabbit(self):
        path = 'D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db'
        c = Controller()
        c.caller('PATH', path)
        param = 'COMPARE'
        body = ['4', '2', '1', '3']
        self.assertEqual(True, c.caller(param, body))

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
