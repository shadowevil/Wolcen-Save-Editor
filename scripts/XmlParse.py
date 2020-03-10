import xml.etree.ElementTree as ET
import os
import re

#sets the current working directory to the folder the script file is located
dir_path = os.path.dirname(os.path.realpath(__file__))


filename = "AffixesWeapons.xml"
elementName = "MagicEffect"
attributeKey = "EffectId"
attributeValue = "HUDPicture"
#removalstrings =[".png", "accessories/Amulet/", "accessories/Ring/", "accessories/ring/", "accessories/belt/"]

tree = ET.parse(dir_path + "\\" + filename)
root = tree.getroot()

PassedLow = []
PassedHigh = []



for item in root.iter(elementName):
    for x in item.iter("LoRoll"):
        for key, value in x.attrib.items():
            temp = item.attrib.get("EffectId") + "," + key + ", Low, " + value
            if(temp not in PassedLow):
                PassedLow.append(temp)

    for x in item.iter("HiRoll"):
        for key, value in x.attrib.items():
            temp = item.attrib.get("EffectId") + "," + key + ", High, " + value
            if(temp not in PassedHigh):
                PassedHigh.append(temp)



Formatted = []

done = []

for low, high in zip(PassedLow,PassedHigh):
    lowNum = low.split(',')
    highNum = high.split(',')

    if(lowNum[-1] != highNum[-1]):
        temp = " { \"%s\", new string[] { \"%s\", \"%s\" }, " % (lowNum[0], lowNum[1], lowNum[1])
        if(temp not in Formatted):
            Formatted.append(temp) 
    else:
        temp = " { \"%s\", new string[] {\"%s\"}, " % (lowNum[0], lowNum[1])
        if(temp not in Formatted):
            Formatted.append(temp) 



with open(dir_path + "\\" + filename + ".txt", "w") as file:
    for item in Formatted:
        print(item)
        file.write(item + '\n')