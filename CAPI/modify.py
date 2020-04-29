import os
pwd = os.getcwd()
pwd2 = pwd.replace('\\', '/')
print(pwd)
print(pwd2)

documents = ["./AI.vcxproj", "./AI.vcxproj.filters"]
for doc in documents:
    lines = open(doc, encoding="utf-8").readlines()
    fp = open(doc, 'w', encoding="utf-8")
    for s in lines:
        str = s.replace(pwd2, ".")
        str = str.replace(pwd, ".")
        fp.write(str)
