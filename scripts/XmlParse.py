import xml.etree.ElementTree as ET
import os
import re

#sets the current working directory to the folder the script file is located
dir_path = os.path.dirname(os.path.realpath(__file__))

with open(dir_path + "\\" + "output" + ".txt", "w") as file:
    for filename in os.listdir(dir_path):
        if(filename.endswith(".xml")):
            tree = ET.parse(dir_path + "\\" + filename)
            root = tree.getroot()
            skillName = ""
            uidDict = {}
            for item in root.iter("AST"):
                skillName = item.attrib["Name"]
                counter = 0
                for i in item.iter("SkillVariant"):
                    uidDict[i.attrib["UID"]] = counter
                    counter += 1

                file.write(f'{{ \"{skillName}\", \n')
                file.write('\tnew Dictionary<string, int>\n')
                file.write('\t{\n')
                for x in uidDict:
                    file.write(f'\t\t{{\"{x}\", {uidDict[x]}}},\n')
                file.write('\t}\n')
                file.write('},\n')


#filename = "AST_AetherBlade.xml"

#removalstrings =[".png", "accessories/Amulet/", "accessories/Ring/", "accessories/ring/", "accessories/belt/"]

