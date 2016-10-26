import json
from pprint import pprint

data_file = open('Log_1.txt')
contents = data_file.read()
entries = contents.split("~~~~")
for entry in entries:
    data = json.loads(entry)
    pprint(data)