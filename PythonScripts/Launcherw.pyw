import subprocess
import time
import pika

controller = subprocess.Popen('pythonw controller.pyw')
piceon = subprocess.Popen('Piceon.exe')


connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

channel.queue_declare(queue='launcher')

while 1:
    message = channel.basic_get('launcher', True)
    if message[2] == b'closing':
        controller.kill()
        break
    time.sleep(1)
