from __future__ import annotations

from PIL import Image
import pika
import json
import subprocess

from collections import namedtuple

import images.similar_images as sm


class Executor:
    @staticmethod
    def start_messaging():
        connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
        channel = connection.channel()
        channel.queue_declare(queue='front')
        channel.queue_declare(queue='back')
        channel.queue_purge(queue='front')
        channel.queue_purge(queue='back')

        def callback(ch, method, properties, body):

            c = Controller.get_instance()
            p = None
            b = None
            bad_json = False
            err_msg = None
            images = [[]]
            no = None
            try:
                no, p, b = c.prepare_message(body)
            except Exception as e:
                err_msg = 'BAD JSON'
                bad_json = True

            if not bad_json:

                if p is not None or b is not None:
                    try:
                        images = c.caller(p, b)
                        # if is_done == "WORNG FUNC":
                        # raise Exception('WRONG FUNCTION')
                    except Exception as e:
                        err_msg = str(e)
                elif p is None and b is None:
                    err_msg = 'BAD PARAMS AND DATA'
                elif p is not None and b is None:
                    err_msg = 'NO DATA'
                else:
                    err_msg = 'BAD REQUEST'

            if err_msg is None:
                result = 'DONE'
            else:
                result = 'ERR'
            response = {
                'taskid': no,
                'result': result,
                'error_massage': err_msg,
                'images': images

            }
            response = json.dumps(response)
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

    def prepare_message(self, message: bytes):
        decode = message.decode('UTF-8')
        try:
            x = json.loads(message, object_hook=lambda d: namedtuple('X', d.keys())(*d.values()))
        except Exception as e:
            raise Exception("BAD JSON")
        # return arr[0], arr[1], arr[2:]
        return x.taskid, x.type, x.images

    def caller(self, param, body):
        switcher = {
            0: self.initial_msg,
            1: self.run_comparator
        }
        func = switcher.get(param, self.__bad_function)
        try:
            return func(body)
        except Exception as e:
            raise e

    def __bad_function(self, name):
        raise Exception("LACK OF METHOD")

    def initial_msg(self, empty_arg):
        return [[]]

    def run_comparator(self, images_ids_paths):

        for path in images_ids_paths:
            if not path[1].lower().endswith(('.png', '.jpg', '.jpeg', '.tiff', '.bmp', '.gif')):
                raise Exception("BAD PATH")
        #return sm.SimilarImageRecognizer.group_by_histogram_and_probability(images_ids_paths)
        return sm.SimilarImageRecognizer.group_by_binary_desc(images_ids_paths)

if __name__ == '__main__':
    subprocess.Popen([r"Piceon.exe"])
    Executor.start_messaging()
