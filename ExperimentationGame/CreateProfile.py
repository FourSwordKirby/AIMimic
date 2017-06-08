#This will compile all of the play data from a profile folder into 1 homogenous profile of the games played
#The profile highlights the state that was encountered as well as the action taken
#It makes the assumption that the process is markovian, prior actions taken and history don't matter

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