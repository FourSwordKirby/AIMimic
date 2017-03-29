import getpass
import time
import random
#moves are
#1 = load bullet
#2 = fire bullet
#3 = block
#4 = use powerful bullet

def get_p2_action(life, bullets):
    #do a thing
    #do this tomorrow
    return random.randint(1,5)


def main():
    starting_life = 1
    starting_bullets = 1

    p1_life = starting_life
    p2_life = starting_life

    p1_bullets = starting_bullets
    p2_bullets = starting_bullets

    f = open('p1_action.txt', 'r')
    past_p1_actions = f.read()
    f.close()

    p1_actions = []
    p2_actions = []
    round = 0

    while(p1_life > 0 and p2_life > 0):
        print("===========[ROUND " + str(round) + " OF BANG]===========")
        print("P1 health: ", p1_life, " bullets ", p1_bullets)
        print("P2 health: ", p2_life, " bullets ", p2_bullets)

        p1_move = getpass.getpass("Enter p1 attack: ")
        if(not p1_move.isdigit()):
            print ("enter a valid action")
            continue
        p1_move = int(p1_move)
        if(p1_move != 1 and p1_move != 2 and p1_move != 3 and p1_move != 4):
            print ("enter a valid action")
            continue
        # p2_move = getpass.getpass("Enter p2 attack: ")
        # if(not p2_move.isdigit()):
        #     print ("enter a valid action")
        #     continue
        # p2_move = int(p2_move)
        # if(p2_move != 1 and p2_move != 2 and p2_move != 3 and p2_move != 4):
        #     print ("enter a valid action")
        #     continue
        p2_move = get_p2_action(p2_life, p2_bullets)



        print(".")
        time.sleep(0.5)
        print(".")
        time.sleep(0.5)
        print(".")
        time.sleep(0.5)


        if(p1_move == 1):
            p1_bullets += 1
        if(p2_move == 1):
            p2_bullets += 1

        if(p1_move == 2):
            if(p1_bullets <= 0):
                print("enter a valid p1 action")
                continue
            p1_bullets -= 1
            if(p2_move != 3):
                p2_life -= 1
        if(p2_move == 2):
            if(p2_bullets <= 0):
                print("enter a valid p2 action")
                continue
            p2_bullets -= 1
            if(p1_move != 3):
                p1_life -= 1

        if(p1_move == 4):
            if(p1_bullets < 3):
                print("enter a valid p1 action")
                continue
            p1_bullets -= 3
            p2_life -= 1
        if(p2_move == 4):
            if(p2_bullets < 3):
                print("enter a valid p2 action")
                continue
            p2_bullets -= 3
            p1_life -= 1

        if(p1_move == 1):
            print("P1: RELOADING")
        elif(p1_move == 2):
            print("P1: BANG")
        elif(p1_move == 3):
            print("P1: BLOCK")
        elif(p1_move == 4):
            print("P1: SUPER BANG")

        if(p2_move == 1):
            print("P2: RELOADING")
        elif(p2_move == 2):
            print("P2: BANG")
        elif(p2_move == 3):
            print("P2: BLOCK")
        elif(p2_move == 4):
            print("P2: SUPER BANG")


        time.sleep(0.5)
        print("P1 health: ", p1_life, " bullets ", p1_bullets)
        print("P2 health: ", p2_life, " bullets ", p2_bullets)
        time.sleep(0.5)
        p1_actions.append(p1_move)
        p2_actions.append(p2_move)


        round += 1


    f = open('p1_action.txt', 'w')
    f.write(past_p1_actions + '\n' + str(p1_actions)) 
    f.close()

if __name__ == '__main__':
    main()