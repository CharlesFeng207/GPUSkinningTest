import os
import sys
from time import sleep
import Gameperf

pkgname = "com.gpuskin.test"
pkgactivity = "{0}/{0}.MainActivity".format(pkgname)

def cmd(cmd):
    print(cmd)
    os.system(cmd)

def test(pkgpath):
    cmd(f"adb uninstall {pkgname}")
    cmd("adb install " + pkgpath)
    run()
    pass

def run():
    cmd(f"adb shell am start -n {pkgactivity}")
    Gameperf.run(pkgname, 15)
    # sleep(3)
    cmd(f"adb shell am force-stop {pkgname}")
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