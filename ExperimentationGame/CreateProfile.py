#This will compile all of the play data from a profile folder into 1 homogenous profile of the games played
#The profile highlights the state that was encountered as well as the action taken
#It makes the assumption that the process is markovian, prior actions taken and history don't matter

#Player 2 is always the recorded player

import json
from pprint import pprint
import glob, os, sys

def clean(data):
    true_data = []
    for elem in data:
        true_data.append(elem[-1])
    return true_data

def ParsePlayerLogs(player_name):
    playerData = []
    roundData = []
    os.chdir(player_name)

    aggregate_profile = []
    for file in glob.glob("*_rand.log"):
        print(file)
        data_file = open(file)
        game_log = []

        contents = data_file.read()
        moves = contents.split("\n~~~\n")

        for move in moves[:-1]:
            stats = move.split('\n')
            print(stats)
            p1_health = int(stats[0][11])
            p1_bullet = int(stats[0][23])
            p2_health = int(stats[1][11])
            p2_bullet = int(stats[1][23])
            p1_move = int(stats[2][11])
            p2_move = int(stats[3][11])
            game_log.append((p1_health, p1_bullet, p2_health, p2_bullet, p1_move, p2_move))
        victory = moves[-1].split('\n')[0][8]
        aggregate_profile.append(game_log)

    profile = open("profile.txt", 'w', 1)
    profile.write(str(aggregate_profile))

def ParseGameplayLogs(player_name):
    playerData = []
    roundData = []
    os.chdir(player_name)

    aggregate_profile = []
    for file in glob.glob("*_rand.log"):
        print(file)
        data_file = open(file)
        game_log = []

        contents = data_file.read()
        moves = contents.split("\n~~~\n")

        for move in moves[:-1]:
            stats = move.split('\n')
            print(stats)
            p1_health = stats[0][11]
            p1_bullet = stats[0][22]
            p2_health = stats[1][11]
            p2_bullet = stats[1][22]
            p1_move = stats[2][11]
            p2_move = stats[3][11]
            game_log.append((p1_health, p1_bullet, p2_health, p2_bullet, p1_move, p2_move))
        print(moves[-1])
        victory = moves[-1].split('\n')[0][8]
        is_AI = moves[-1].split('\n')[1][16]
        print(victory)
        print(is_AI)
        aggregate_profile.append(game_log)

    profile = open("profile.txt", 'a', 1)
    profile.write(str(aggregate_profile))

if __name__ == '__main__':
    ParsePlayerLogs("Roger")