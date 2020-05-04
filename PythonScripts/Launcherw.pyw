import subprocess
import time
import pika

launcher_queue_name = 'launcher'

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

channel.queue_declare(queue=launcher_queue_name)
channel.queue_purge(queue=launcher_queue_name)


controller = subprocess.Popen('pythonw controller.pyw')
piceon = subprocess.Popen('Piceon.exe')

while 1:
    message = channel.basic_get(launcher_queue_name, True)
    if message[2] == b'closing':
        controller.kill()
        break
    time.sleep(1)
