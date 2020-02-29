import xml.etree.ElementTree as ET
import os
import re

#sets the current working directory to the folder the script file is located
dir_path = os.path.dirname(os.path.realpath(__file__))


filename = "Accessories.xml"
elementName = "Item"
attributeKey = "Name"
attributeValue = "HUDPicture"
removalstrings =[".png", "accessories/Amulet/", "accessories/Ring/", "accessories/ring/", "accessories/belt/"]

tree = ET.parse(dir_path + "\\" + filename)
root = tree.getroot()

dictCollection = {}
for item in root.iter(elementName):
    name = item.attrib.get(attributeKey)
    p = re.compile('|'.join(map(re.escape, removalstrings)))
    path = p.sub('', item.attrib.get(attributeValue))
    dictCollection[name] = path

with open(dir_path + "\\" + filename + ".txt", "w") as file:
    for item in dictCollection:
        print("{ \"%s\", \"%s\" }," % (item, dictCollection[item]))
        file.write("{ \"%s\", \"%s\" },\n" % (item, dictCollection[item]))
