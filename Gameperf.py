import requests
import time
from PerfServiceClient import PerfServiceClient
import threading

def init(client):
    client.get_all_devices()
    client.connect_any_android_device()
    client.get_all_apps()

def collect(client, args):
    t1 = threading.Thread(target=client.collect_perf_data, name='collect_perf_data', args=args)
    t2 = threading.Thread(target=client.collect_cap_data, name='collect_cap_data', args=args)
    t1.start()
    t2.start()
    t1.join()
    t2.join()


def run(pkgname, duration):
    args = [pkgname, duration]
    client = PerfServiceClient()
    init(client)
    collect(client, args)