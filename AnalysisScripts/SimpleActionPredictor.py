from PythonParse import ParsePlayerLogs

import numpy as np
import pandas as pd

data = ParsePlayerLogs("Jotaro")
snapshots = [snapshot for game in data for snapshot in game]

def process_elem(e):
    if e == False:
        return 0.0
    elif e == True:
        return 1.0
    elif str(e).isdigit() or isinstance(e, float):
        return float(e)
    else:
        return None

def flatten_dict(d):
    values = [(k,d[k]) for k in sorted(d.keys())]
    result = dict()
    for (key, elem) in values:
        if isinstance(elem, dict):
            elem = flatten_dict(elem)
            for sub_key in sorted(elem.keys()):
                result[key + "_" + sub_key] = elem[sub_key]
        else:
            new_elem = process_elem(elem)
            if new_elem != None:
                result[key] = new_elem
    return result

snapshots = pd.DataFrame([flatten_dict(snapshot) for snapshot in snapshots])

p1_snapshots = snapshots.loc[snapshots['initiatedPlayer'] == 0]
#p1_snapshots = p1_snapshots.loc[snapshots['p1Interrupt'] == 0.0]
#p1_snapshots = p1_snapshots.loc[snapshots['p2Interrupt'] == 0.0]


p2_snapshots = snapshots.loc[snapshots['initiatedPlayer'] == 1]
#p2_snapshots = p2_snapshots.loc[snapshots['p1Interrupt'] == 0.0]
#p2_snapshots = p2_snapshots.loc[snapshots['p2Interrupt'] == 0.0]


p2_train = p2_snapshots.tail(-2000)
p2_test = p2_snapshots.tail(2000)

#Ngram programming
from collections import deque, Counter
import random

#Generating the set of ngrams from training set of experience
n = 3
p2_ngrams = []

prev_frame = -1
current_ngram = deque()
current_ngram.extend([None] * n)

for index, snapshot in p2_train.iterrows():
    frame = snapshot['frameTaken']
    action = snapshot['p2Action']
    if(frame < prev_frame):
        current_ngram = deque()
        current_ngram.extend([None] * n)

    prev_frame = frame
    current_ngram.append(action)
    current_ngram.popleft()
    p2_ngrams.append(tuple(current_ngram))

ngram_table = Counter(p2_ngrams)


#Testing how well the ngrams do at prediction results
def getAction(ngram, ngram_counts):
    prior = list(ngram)[1:]
    weights = dict()
    for action in range(13):
        key = tuple(prior+[action])
        if(key in ngram_counts.keys()):
            weights[action] =  ngram_counts[key]
    
    softmax = sum(weights.values())
    rng = random.uniform(0.0, softmax)
    threshold = 0.0
    for action in weights.keys():
        threshold += weights[action]
        if rng < threshold:
            return action

successes = 0
total_trials = 0

prev_frame = -1
current_ngram = deque()
current_ngram.extend([None] * n)

for index, snapshot in p2_test.iterrows():
    frame = snapshot['frameTaken']
    action = snapshot['p2Action']
    if(frame < prev_frame):
        current_ngram = deque()
        current_ngram.extend([None] * n)

    ngram_action = getAction(current_ngram, ngram_table)

    if(action == ngram_action):
        successes += 1

    prev_frame = frame
    current_ngram.append(action)
    current_ngram.popleft()

    total_trials += 1

print(successes/total_trials)
print("Completed")



