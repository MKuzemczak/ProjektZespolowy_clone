import pika

from PythonScripts.rabitmq.controller import Controller


def start():
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
    channel = connection.channel()
    channel.queue_declare(queue='front')
    channel.queue_declare(queue='back')

    def callback(ch, method, properties, body):
        c = Controller.get_instance()
        p = None
        b = None
        response = body.decode('UTF-8')
        p, b = c.prepare_message(body)
        if p is not None or b is not None:
            is_done = c.caller(p, b)
            if is_done:
                response = 'DONE'
            else:
                response = 'PYTHON FAILED/BAD PARAM/BAD DATA'
        elif p is None and b is None:
            response = 'BAD PARAMS AND DATA'
        elif p is not None and b is None:
            response = 'NO DATA'
        else:
            response = 'BAD REQUEST'

        channel.basic_publish(exchange='', routing_key='back', body='DONE')
        channel.basic_publish(exchange='',
                              routing_key='back',
                              body=str(response))

    channel.basic_consume(queue='front', on_message_callback=callback, auto_ack=True)
    channel.start_consuming()
