import os
import sys
from time import sleep
import Gameperf

def cmd(cmd):
    print(cmd)
    os.system(cmd)

def test(pkgpath):
    cmd("adb uninstall com.gpuskin.test")
    cmd("adb install " + pkgpath)
    run()
    pass

def run():
    cmd("adb shell am start -n com.gpuskin.test/com.gpuskin.test.MainActivity")
    Gameperf.run('com.gpuskin.test', 10)
    # sleep(3)
    cmd("adb shell am force-stop com.gpuskin.test")
    pass

if __name__ == "__main__":
    script_directory = os.path.dirname(os.path.abspath(__file__))
    build_directory = os.path.join(script_directory, "Build")
    for filename in os.listdir(build_directory):
        # check file extension
        if not filename.endswith(".apk"):
            continue
        test(os.path.join(build_directory, filename))        
        pass
    
    input("Press Enter to continue...")
    pass