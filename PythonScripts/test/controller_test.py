import unittest

import pika

from controller import Executor
import json

from db.db_service import DBService


class ControllerTest(unittest.TestCase):

    def test_controller_with_rabit(self):
        db = DBService('D:\Programming\python\SIFT\\venv\Include\projekt_zesp.db')
        images_id = [5, 1, 2, 3, 4, 11, 6, 7, 8, 10, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21]
        sel = '('
        for index in range(len(images_id) - 1):
            sel = sel + str(images_id[index]) + ', '
        sel = sel + str(images_id[len(images_id) - 1]) + ')'
        images_path = db.create_select('IMAGE', 'Id', sel)
        # images_path = list(map(lambda x: x[1], images_path))
        images_path = list(map(lambda x: [x[0], x[1]], images_path))

        connection = pika.BlockingConnection(pika.ConnectionParameters('localhost'))
        channel = connection.channel()
        channel.queue_declare(queue='front')
        channel.queue_declare(queue='back')
        json_init = json.loads('{"taskid":123,"type":0,"images":[]}')
        json_init = json.dumps(json_init)


        json_compare = {"taskid": 5678,
                        "type": 1,
                        "images": images_path
                        }
        json_compare = json.dumps(json_compare)
        print(json_compare)
        channel.basic_publish(exchange='',
                              routing_key='front',
                              body=json_init)

        channel.basic_publish(exchange='',
                              routing_key='front',
                              body=json_compare)

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
