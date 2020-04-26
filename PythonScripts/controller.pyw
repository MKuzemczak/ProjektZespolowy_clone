from __future__ import annotations

import os

import pika

#from PythonScripts.similar_images import SimilarImageRecognizer
import images.similar_images as sm


class Executor:
    @staticmethod
    def start_messaging():
        connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
        channel = connection.channel()
        channel.queue_declare(queue='front')
        channel.queue_declare(queue='back')

        def callback(ch, method, properties, body):
            c = Controller.get_instance()
            p = None
            b = None
            no, p, b = c.prepare_message(body)
            response = no + '-'
            if p is not None or b is not None:
                is_done = False
                try:
                    is_done = c.caller(p, b)
                    # if is_done == "WORNG FUNC":
                    # raise Exception('WRONG FUNCTION')
                except Exception as e:
                    response += str(e)
                if is_done:
                    response += 'DONE'

            elif p is None and b is None:
                response += 'BAD PARAMS AND DATA'
            elif p is not None and b is None:
                response += 'NO DATA'
            else:
                response += 'BAD REQUEST'

            channel.basic_publish(exchange='',
                                  routing_key='back',
                                  body=str(response))

        channel.basic_consume(queue='front', on_message_callback=callback, auto_ack=True)
        channel.start_consuming()


class Controller:
    __instance = None

    @staticmethod
    def get_instance() -> Controller:
        if Controller.__instance is None:
            Controller()
        return Controller.__instance

    def __init__(self):
        if Controller.__instance is None:
            Controller.__instance = self
            self.db_path = None
            self.image_comparator: sm.SimilarImageRecognizer = None  # SimilarImageRecognizer(path)

    def prepare_message(self, message: bytes):
        arr = message.decode('UTF-8').split(' ')
        return arr[0], arr[1], arr[2:]

    def caller(self, param, body):
        switcher = {
            'PATH': self.init_path,
            'COMPARE': self.run_comparator,
            'LOCALISATION': self.tag_localisation
        }
        func = switcher.get(param, self.__bad_function)
        try:
            return func(body)
        except Exception as e:
            raise e

    def __bad_function(self, name):
        raise Exception("LACK OF METHOD")

    def init_path(self, db_path):
        if not os.path.exists(db_path[0]):
            raise Exception('LACK OF FILE')
        if not db_path[0].endswith('.db'):
            raise Exception('WRONG EXTENSION')
        if self.db_path is None:
            self.db_path = db_path[0]
            self.image_comparator = sm.SimilarImageRecognizer(db_path[0])
            return True
        else:
            return False

    def run_comparator(self, images_id):
        if self.db_path is None:
            raise Exception("LACK OF PATH")
        for i in images_id:
            try:
                int(i)
            except ValueError:
                raise Exception('WRONG ID')
        return self.image_comparator.compare_images(images_id)

    def tag_localisation(self, body):
        pass


if __name__ == '__main__':
    Executor.start_messaging()