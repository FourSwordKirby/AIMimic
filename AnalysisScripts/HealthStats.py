import sys


def getStats(filename):
    filename += ".txt"
    data_file = open(filename)
    contents = data_file.read()
    entries = contents.split("\n")

    data = []
    for entry in entries:
        if(entry != ""):
            data.append(float(entry.rstrip()))
    return data

baseName = sys.argv[1]

orig = getStats(baseName+"_health")
ngram = getStats(baseName+"ngram"+"_health")
ghost = getStats(baseName+"ghost"+"_health")
AI = getStats(baseName+"AI"+"_health")

import numpy as np
print("Mean")
print(np.mean(orig))
print(np.mean(ngram))
print(np.mean(ghost))
print(np.mean(AI))
print("Standard Deviation")
print(np.std(orig))
print(np.std(ngram))
print(np.std(ghost))
print(np.std(AI))