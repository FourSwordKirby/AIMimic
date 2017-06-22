import json
from pprint import pprint
import glob, os, sys


def ParsePlayerLogs(player_name):
    playerData = []
    roundData = []
    os.chdir(player_name)
    lastSeenTime = 0.0

    for file in glob.glob("*.txt"):
        print(file)
        data_file = open(file)
        contents = data_file.read()
        entries = contents.split("~~~~")
        for entry in entries:
            if(entry.rstrip() != ""):
                data = json.loads(entry)

                currentTime = data["frameTaken"]
                if currentTime < lastSeenTime:
                    playerData.append(roundData)
                    roundData = []

                roundData.append(data)
                lastSeenTime = currentTime

    if len(roundData) > 0:
        playerData.append(roundData)
    return playerData

if __name__ == '__main__':
    ParsePlayerLogs(sys.argv[1])