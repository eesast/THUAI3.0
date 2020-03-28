import json
import time
import re


def generate(data, file):
    i = 0
    for item in data:
        if item == "Wheat":
            i = 1
            file.write(
                "struct DishProperty\n{\nint Score;\nint CookTime;\nint TaskTime;\n};\n")
            file.write(
                "const static std::map<int, DishProperty> DishInfo =\n{\n")
        elif item == "CookingTable":
            i = 2
            file.write("};\n")
            file.write(
                "const static std::unordered_map<int, std::list<Protobuf::DishType>> CookingTable =\n{\n")
        if i == 0:
            if type(data[item]) is dict:
                file.write("struct " + item+"\n")
                file.write("{\n")
                generate(data[item], file)
                file.write("};\n")
            elif type(data[item]) is int:
                file.write("const static int " + item +
                           " = " + str(data[item]) + ";\n")
            elif type(data[item]) is float:
                file.write("const static double " + item +
                           " = " + str(data[item]) + ";\n")
        elif i == 1:
            file.write("{int(Protobuf::" + item + "), {" +
                       str(data[item]["Score"]) + "," +
                       str(data[item]["CookTime"] if "CookTime" in data[item] else 0) + "," +
                       str(data[item]["TaskTime"] if "TaskTime" in data[item] else 0) + "} },\n")
        elif i == 2:
            for material in data["CookingTable"]:
                file.write(
                    "{int(Protobuf::" + data["CookingTable"][material] + "), {"+re.sub("[A-Z]", lambda x: ",Protobuf::"+x.group(0), material).lstrip(",")+"} },\n")


with open('Config.json', 'r') as f:
    data = json.load(f)
    # print(type(data) is dict)
    with open('Constant.h', 'w') as fileOut:
        generate(data, fileOut)
        fileOut.write("};\n")
