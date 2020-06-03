from __future__ import annotations

import asyncio
import threading
import queue
import pika

import json
import subprocess

from collections import namedtuple

import images.similar_images as sm


class Executor:
    @staticmethod
    def start_messaging(channel, controller):
        # connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
        # channel = connection.channel()
        channel.queue_declare(queue='front')
        channel.queue_declare(queue='back')
        channel.queue_purge(queue='front')
        channel.queue_purge(queue='back')

        def callback(ch, method, properties, body):
            no, p, b = None, None, None
            try:
                no, p, b = controller.prepare_message(body)
                controller.queue.put_nowait([no, p, b])
                controller.event.set()
                controller.event.clear()
            except Exception as e:
                err_msg = 'BAD JSON'
                result = 'ERR'
                images = [[]]
                response = {
                    'taskid': no,
                    'result': result,
                    'error_massage': err_msg,
                    'images': images
                }
                controller.channel.basic_publish(exchange='',
                                                 routing_key='back',
                                                 body=str(response))

        channel.basic_consume(queue='front', on_message_callback=callback, auto_ack=True)
        channel.start_consuming()


class Controller:
    __instance = None

    @staticmethod
    def get_instance(channel) -> Controller:
        if Controller.__instance is None:
            Controller(channel)
        return Controller.__instance

    def __init__(self, channel):
        if Controller.__instance is None:
            Controller.__instance = self
            if channel is not None:
                self.channel = channel
            self.queue = queue.Queue()
            self.event = threading.Event()
            self.thread = threading.Thread(target=self.response)
            self.thread.daemon = True
            self.thread.start()

    def prepare_message(self, message: bytes):
        try:
            x = json.loads(message, object_hook=lambda d: namedtuple('X', d.keys())(*d.values()))
        except Exception as e:
            raise Exception("BAD JSON")
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
        return sm.SimilarImageRecognizer.group_by_local_binary_patters(images_ids_paths)

    def response(self):
        while True:
            if self.event:
                no, p, b = self.queue.get()

                err_msg = None
                images = [[]]
                if p is not None or b is not None:
                    try:
                        images = self.caller(p, b)
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

                self.channel.basic_publish(exchange='',
                                           routing_key='back',
                                           body=str(response))


if __name__ == '__main__':
    subprocess.Popen([r"Piceon.exe"])
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
    channel = connection.channel()
    controller = Controller(channel)

    Executor.start_messaging(channel, controller)
