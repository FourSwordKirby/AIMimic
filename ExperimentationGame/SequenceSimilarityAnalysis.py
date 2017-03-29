import json
from pprint import pprint
import PythonParse
import random

playerLogs = PythonParse.ParsePlayerLogs("Dunjunmstr")
random.shuffle(playerLogs)

test = playerLogs[len(playerLogs)/2:len(playerLogs)]
holdout = playerLogs[0:len(playerLogs)/2]

#implementation of the 3gram comparator
n = 3

player_hist = dict()

for round in test:
    history = []
    for entry in round:
        if(entry['initiatedPlayer'] == 1):
            history.append(entry['p2Action'])
            if(len(history) > n):
                history.pop(0)
            if(len(history) == n):
                if(not player_hist.has_key(str(history))):
                    player_hist[str(history)] = 0
                player_hist[str(history)] += 1
print player_hist

# holdout_hist = dict()
# for round in holdout:
#     history = []
#     for entry in round:
#         if(entry['initiatedPlayer'] == 1):
#             history.append(entry['p2Action'])
#             if(len(history) > n):
#                 history.pop(0)
#             if(len(history) == n):
#                 if(not holdout_hist.has_key(str(history))):
#                     holdout_hist[str(history)] = 0
#                 holdout_hist[str(history)] += 1
# print holdout_hist

holdout_hist = dict()
for round in test:
    history = []
    for entry in round:
        if(entry['initiatedPlayer'] == 0):
            history.append(entry['p1Action'])
            if(len(history) > n):
                history.pop(0)
            if(len(history) == n):
                if(not holdout_hist.has_key(str(history))):
                    holdout_hist[str(history)] = 0
                holdout_hist[str(history)] += 1
print holdout_hist


similarity = 1
difference = 0

keys = set(holdout_hist.keys()).intersection(set(player_hist.keys()))
normalize = 1.0/len(keys)

for key in keys:
    difference += float(abs(holdout_hist[key] - player_hist[key])) / float((holdout_hist[key] + player_hist[key]))

similarity -= normalize * difference

print similarity