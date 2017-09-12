import json
from pprint import pprint
import glob, os, sys


os.chdir(sys.argv[1])
for file in glob.glob("*.txt"):
    print(file)
    data_file = open(file)
    contents = data_file.read()
    entries = contents.split("~~~~")
    for entry in entries:
        if(entry.rstrip() != ""):
            data = json.loads(entry)
            pprint(data)
