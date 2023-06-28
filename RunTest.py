import os
import sys
from time import sleep


def test(pkgname):
    os.system("adb uninstall com.gpuskin.test")
    script_directory = os.path.dirname(os.path.abspath(__file__))
    pkgpath = os.path.join(script_directory, "Build", pkgname + ".apk")
    os.system("adb install " + pkgpath)
    run()
    pass

def run():
    os.system("adb shell am start -n com.gpuskin.test/com.gpuskin.test.MainActivity")
    sleep(10)
    os.system("adb shell am force-stop com.gpuskin.test")
    pass

if __name__ == "__main__":
    print(sys.argv[1])
    buids = sys.argv[1].split(";")
    for build in buids:
        test(build)
    input("Press Enter to continue...")
    pass