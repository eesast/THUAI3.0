import os
pwd = os.getcwd()
pwd2 = pwd.replace('\\', '/')
print(pwd)
print(pwd2)
lines = open("./build/AI.vcxproj", encoding="utf-8").readlines()
fp = open("./build/AI.vcxproj", 'w', encoding="utf-8")
for s in lines:
    fp.write(s.replace(pwd, "..").replace(pwd2, ".."))
