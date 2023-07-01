import requests
import json
from websocket import create_connection
import threading
import time

HOST = '127.0.0.1'
PORT = 3000
http_prefix = "http://{0}:{1}".format(HOST, PORT)

# 采集性能数据对应的websocket地址
perf_data_uri = "ws://{0}:{1}/CollectPerfData".format(HOST, PORT)
capture_data_uri = "ws://{0}:{1}/CollectCaptureData".format(HOST, PORT)

# 开始和停止测试采集性能数据的命令
data_start = {"action": "startCollect",
              "appName": ""}
data_stop = {"action": "stopCollect"}

# 开始和停止采集截图的命令
cap_start = {"action": "startCapture"}
cap_stop = {"action": "stopCapture"}

def request_get(url, params=None, log=True):
    print(url)
    result = requests.get(url, params).json()
    if log: 
        print(result)

    return result

class PerfServiceClient:
    def __init__(self):
        self.current_time = -1  # 当前采集的性能数据所对应的事件
        self.current_device_info = {}  # 当前选中设备的设备信息

        self.perf_data = []  # 存放已采集的所有性能数据
        self.capture_data = []  # 存放截图保存路径

        self.devices = []  # 电脑上连接的所有设备
        self.apps = []  # 选中设备上的所有应用信息
        self.perf_ws = create_connection(perf_data_uri)  # 用来获取性能数据的websocket
        self.cap_ws = create_connection(capture_data_uri)  # 用来获取截图数据的websocket


    def collect_perf_data(self, packageName, duration):

        found_item = next((item for item in self.apps['data'] if item["packageName"] == packageName), None)
        if not found_item:
            print("app no found")
            return

        print(found_item)

        data_start['appName'] = found_item['appName']
        
        data = True
        self.perf_ws.send(json.dumps(data_start))
        print('collect_perf_data...')

        while data:
            data = self.perf_ws.recv()
            data = json.loads(data)
            
            if 'stopCollect' in data:
                self.current_time = -1
                data = False
            elif 'time' in data:
                self.current_time = int(data['time'])
                if self.current_time == 0:
                    self.set_scene('自动化测试')
                if self.current_time == 2:
                    self.start_record()
                # if self.current_time == 10:
                #     self.start_subscene('TikTok子场景')
                # if self.current_time == 15:
                #     self.stop_subscene()
                if self.current_time == duration:
                    self.stop_record()
                if self.current_time == duration + 1:
                    self.perf_ws.send(json.dumps(data_stop))

        print('collect_perf_data terminate')
        self.perf_ws.close()
        self.perf_ws = None

    def collect_cap_data(self, packageName, duration):
        data = True
        while self.current_time < 0:
            time.sleep(1)
        self.cap_ws.send(json.dumps(cap_start))

        print('collect_cap_data...')
        while data:
            data = self.cap_ws.recv()
            data = json.loads(data)
            # print(data)
            if 'stopCapture' in data:
                data = None
            if self.current_time >= duration - 1:
                self.cap_ws.send(json.dumps(cap_stop))
            time.sleep(1)
        
        print('collect_cap_data terminate')
        self.cap_ws.close()
        self.cap_ws = None

    # 开始记录
    def start_record(self):
        return request_get('{0}/startRecord'.format(http_prefix))

    # 结束记录,可以选择是否导出与是否上传
    def stop_record(self):
        return request_get('{0}/stopRecord'.format(http_prefix), params={'fileExport': 0, 'fileUpload': 1})

    # 连接设备
    def connect_device(self, udid):
        self.current_device_info = request_get(
            '{0}/connectDevice'.format(http_prefix), params={'udid': udid})
    
    def connect_device_by_name(self, device_name):
        # find udid by deviceName
        found_item = next((item for item in self.devices["data"] if item["deviceName"] == device_name), None)
        if not found_item:
            print("device no found")
            return
        self.connect_device(found_item['udid'])

    # 获取所有应用
    def get_all_apps(self):
        self.apps = request_get('{0}/regainAllApps'.format(http_prefix), False)

    # 获取所有设备
    def get_all_devices(self):
        self.devices = request_get(
            '{0}/getAllDevices'.format(http_prefix))

    # 子场景开始
    def start_subscene(self, subscene_name):
        return request_get('{0}/subSceneStart'.format(http_prefix), params={'subSceneName': subscene_name})

    # 子场景结束
    def stop_subscene(self):
        return request_get('{0}/subSceneEnd'.format(http_prefix))

    # 设置场景命名
    def set_scene(self, scene_name):
        return request_get('{0}/setScene'.format(http_prefix), params={'sceneName': scene_name})

    # 上传数据
    def upload_data(self, json_path):
        return request_get('{0}/uploadData'.format(http_prefix), params={'jsonPath': json_path})
    