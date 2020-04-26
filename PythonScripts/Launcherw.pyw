import subprocess
import time
import pika

controller = subprocess.Popen('pythonw rabbitmq/controller.pyw')
rabbit = subprocess.Popen('Piceon.exe')


connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

channel.queue_declare(queue='launcher')

while 1:
    message = channel.basic_get('launcher', True)
    if message[2] == b'closing':
        receiver.kill()
        break
    time.sleep(1)
