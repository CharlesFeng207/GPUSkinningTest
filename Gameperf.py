import requests
import time
from PerfServiceClient import PerfServiceClient
import threading

def connect(client):
    client.get_all_devices()
    client.connect_device_by_name('KB2000')
    client.get_all_apps()

def collect(client):
    t1 = threading.Thread(target=client.collect_perf_data, name='collect_perf_data')
    t2 = threading.Thread(target=client.collect_cap_data, name='collect_cap_data')
    t1.start()
    t2.start()
    t1.join()
    t2.join()


def run():
    client = PerfServiceClient()
    connect(client)
    collect(client)
    client.disconnect()

run()
time.sleep(3)
run()