import json
from pprint import pprint
import PythonParse
import random
import sys

baseName = sys.argv[1]
playerLogs = PythonParse.ParsePlayerLogs(baseName)
ngramPlayerLogs = PythonParse.ParsePlayerLogs(baseName+"ngram")
ghostPlayerLogs = PythonParse.ParsePlayerLogs(baseName+"ghost")
AIPlayerLogs = PythonParse.ParsePlayerLogs(baseName+"AI")


def getSimilarity(h1_hist, h2_hist):
    similarity = 1
    difference = 0

    keys = set(h1_hist.keys()).union(set(h2_hist.keys()))
    normalize = 1.0/len(keys)

    for key in keys:
        h1_count = h1_hist[key] if key in h1_hist else 0
        h2_count = h2_hist[key] if key in h2_hist else 0
        difference += float(abs(h1_count - h2_count)) / float((h1_count + h2_count))

    similarity -= normalize * difference
    print(similarity)

    return similarity

def evaluate(original):
    original_hist = dict()

    hitCount = 0.0
    entryCount = 0.0

    #Comparator is a compilation of 1 to 3 grams
    for round in original:
        for entry in round:            
            if(entry['p2Status'] == 7 or entry['p2Status'] == 8):
                hitCount += 1
            if(entry['initiatedPlayer'] == 0):
                entryCount += 1
    
    return (hitCount, entryCount)

retry_avg = []
ngram_avg = []
ghost_avg = []
AI_avg = []

retry_count = []
ngram_count = []
ghost_count = []
AI_count = []

for i in range(5):
    m_retry = playerLogs[i:i+1]
    m_ngram = ngramPlayerLogs[i:i+1]
    m_ghost = ghostPlayerLogs[i:i+1] 
    m_AI = AIPlayerLogs[i:i+1]

    h, c = evaluate(m_retry)
    n_h, n_c = evaluate(m_ngram)
    g_h, g_c = evaluate(m_ghost)
    a_h, a_c = evaluate(m_AI)

    retry_avg.append(h)
    ngram_avg.append(n_h)
    ghost_avg.append(g_h)
    AI_avg.append(a_h)

    retry_count.append(c)
    ngram_count.append(n_c)
    ghost_count.append(g_c)
    AI_count.append(a_c)



import numpy as np
print("Mean")
print(np.mean(retry_avg))
print(np.mean(ngram_avg))
print(np.mean(ghost_avg))
print(np.mean(AI_avg))
print("Standard Deviation")
print(np.std(retry_avg))
print(np.std(ngram_avg))
print(np.std(ghost_avg))
print(np.std(AI_avg))

print("Mean")
print(np.mean(retry_count))
print(np.mean(ngram_count))
print(np.mean(ghost_count))
print(np.mean(AI_count))
print("Standard Deviation")
print(np.std(retry_count))
print(np.std(ngram_count))
print(np.std(ghost_count))
print(np.std(AI_count))