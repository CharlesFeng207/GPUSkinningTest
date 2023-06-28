import requests

def connect():
    result = requests.get("http://127.0.0.1:3000/getAllDevices")
    print(result.json())

     # find udid by deviceName
    found_item = next((item for item in result.json()["data"] if item["deviceName"] == "KB2000"), None)
    if not found_item:
        print("no device found")
        return
        
    print(found_item["udid"])

    result = requests.get("http://127.0.0.1:3000/connectDevice?udid=" + found_item["udid"])
    print(result.json())
    
    result = requests.get("http://127.0.0.1:3000/regainAllApps")
    print(result.json())
    
    found_item = next((item for item in result.json()["data"] if item["packageName"] == "com.gpuskin.test"), None)
    if not found_item:
        print("no device found")
        return
    
    print(found_item)
connect()