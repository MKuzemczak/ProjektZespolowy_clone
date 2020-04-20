from __future__ import annotations

import pika

from PythonScripts.similar_images import SimilarImageRecognizer


class Executor:
    @staticmethod
    def start_messaging():
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
            self.image_comparator: SimilarImageRecognizer = None  # SimilarImageRecognizer(path)

    def prepare_message(self, message):
        print(message.decode('UTF-8'))
        arr = message.decode('UTF-8').split(' ')
        print(arr)
        return arr[0], arr[1:]

    def caller(self, param, body):
        switcher = {
            'PATH': self.init_path,
            'COMPARE': self.run_comparator,
            'LOCALISATION': self.tag_localisation
        }
        func = switcher.get(param, lambda: False)
        return func(body)

    def init_path(self, path):
        if self.db_path is None:
            self.db_path = path[0]
            self.image_comparator = SimilarImageRecognizer(path[0])
            return True
        else:
            return False

    def run_comparator(self, images_id):
        if self.db_path is None:
            raise Exception("BRAK SCIEZKI")
        return self.image_comparator.compare_images(images_id)

    def tag_localisation(self, body):
        pass


if __name__ == '__main__':
    Executor.start_messaging()
