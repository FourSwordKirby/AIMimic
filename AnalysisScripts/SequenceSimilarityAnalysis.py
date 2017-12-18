import json
from pprint import pprint
import PythonParse
import random
import sys

baseName = sys.argv[1]
original =  PythonParse.ParsePlayerLogs(baseName+"orig")
playerLogs = PythonParse.ParsePlayerLogs(baseName)
ngramPlayerLogs = PythonParse.ParsePlayerLogs(baseName+"ngram")
ghostPlayerLogs = PythonParse.ParsePlayerLogs(baseName+"ghost")
AIPlayerLogs = PythonParse.ParsePlayerLogs(baseName+"AI")
otherPlayerLogs = PythonParse.ParsePlayerLogs(sys.argv[2])


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

def evaluate(original, retry, ngram, Ghost, AI, Other):
    original_hist = dict()
    retry_hist = dict()
    ngram_hist = dict()
    Ghost_hist = dict()
    AI_hist = dict()
    Other_hist = dict()

    #Comparator is a compilation of 1 to 3 grams
    for i in range(3):
        n = i+1
        for round in original:
            history = []
            for entry in round:
                if(entry['initiatedPlayer'] == 0):
                    history.append(entry['p1Action'])
                    if(len(history) > n):
                        history.pop(0)
                    if(len(history) == n):
                        if(str(history) not in original_hist):
                            original_hist[str(history)] = 0
                        original_hist[str(history)] += 1

        for round in retry:
            history = []
            for entry in round:
                if(entry['initiatedPlayer'] == 0):
                    history.append(entry['p1Action'])
                    if(len(history) > n):
                        history.pop(0)
                    if(len(history) == n):
                        if(str(history) not in retry_hist):
                            retry_hist[str(history)] = 0
                        retry_hist[str(history)] += 1
        for round in ngram:
            history = []
            for entry in round:
                if(entry['initiatedPlayer'] == 0):
                    history.append(entry['p1Action'])
                    if(len(history) > n):
                        history.pop(0)
                    if(len(history) == n):
                        if(str(history) not in ngram_hist):
                            ngram_hist[str(history)] = 0
                        ngram_hist[str(history)] += 1
        for round in Ghost:
            history = []
            for entry in round:
                if(entry['initiatedPlayer'] == 0):
                    history.append(entry['p1Action'])
                    if(len(history) > n):
                        history.pop(0)
                    if(len(history) == n):
                        if(str(history) not in Ghost_hist):
                            Ghost_hist[str(history)] = 0
                        Ghost_hist[str(history)] += 1
        for round in AI:
            history = []
            for entry in round:
                if(entry['initiatedPlayer'] == 0):
                    history.append(entry['p1Action'])
                    if(len(history) > n):
                        history.pop(0)
                    if(len(history) == n):
                        if(str(history) not in AI_hist):
                            AI_hist[str(history)] = 0
                        AI_hist[str(history)] += 1
        for round in Other:
            history = []
            for entry in round:
                if(entry['initiatedPlayer'] == 0):
                    history.append(entry['p1Action'])
                    if(len(history) > n):
                        history.pop(0)
                    if(len(history) == n):
                        if(str(history) not in Other_hist):
                            Other_hist[str(history)] = 0
                        Other_hist[str(history)] += 1
    #print(original_hist)
    #print(retry_hist)
    #print(ngram_hist)
    #print(Ghost_hist)
    #print(AI_hist)
    #print(Other_hist)

    print("~~~~~~~~~~~~~~~~~~~~~")
    print("retry")
    retry_sim = getSimilarity(original_hist, retry_hist)
    print("ngram")
    ngram_sim = getSimilarity(original_hist, ngram_hist)
    print("Ghost")
    ghost_sim = getSimilarity(original_hist, Ghost_hist)
    print("AI")
    AI_sim = getSimilarity(original_hist, AI_hist)
    print("Other")
    other_sim = getSimilarity(original_hist, Other_hist)
    print("~~~~~~~~~~~~~~~~~~~~~")

    hists = [retry_sim, ngram_sim, ghost_sim, AI_sim, other_sim]
    
    return hists

retry_avg = []
ngram_avg = []
ghost_avg = []
AI_avg = []
other_avg = []

for i in range(5):
    if(i == int(sys.argv[3])):
        print("skip")
        pass
    else:
        m_retry = playerLogs[i:i+1]
        m_ngram = ngramPlayerLogs[i:i+1]
        m_Ghost = ghostPlayerLogs[i:i+1] 
        m_AI = AIPlayerLogs[i:i+1] 
        m_Other = otherPlayerLogs[i:i+1]
        res = evaluate(original, m_retry, m_ngram, m_Ghost, m_AI, m_Other)

        if(res[0] != 1.0):
            retry_avg.append(res[0])
        ngram_avg.append(res[1])
        ghost_avg.append(res[2])
        AI_avg.append(res[3])
        other_avg.append(res[4])

import numpy as np
print("Mean")
print(np.mean(retry_avg))
print(np.mean(ngram_avg))
print(np.mean(ghost_avg))
print(np.mean(AI_avg))
print(np.mean(other_avg))
print("Standard Deviation")
print(np.std(retry_avg))
print(np.std(ngram_avg))
print(np.std(ghost_avg))
print(np.std(AI_avg))
print(np.std(other_avg))