class BangEnv(object):
    def __init__(self, starting_life, starting_bullets):
        self.actions = [1,2,3,4]

        self.starting_life = starting_life
        self.starting_bullets = starting_bullets

        self.p1_life = starting_life
        self.p2_life = starting_life

        self.p1_bullets = starting_bullets
        self.p2_bullets = starting_bullets

    def all_states(self):
        states = []
        for p1_life in range(4):
            for p2_life in range(4):
                for p1_bullets in range(10):
                    for p2_bullets in range(10):
                        states.append((p1_life, p1_bullets, p2_life, p2_bullets))
        return states

    def p1_actions(self, state):
        p1_life = state[0]
        p1_bullets = state[1]

        p2_life = state[2]
        p2_bullets = state[3]

        actions = []
        if(p1_bullets >= 0):
            actions.append(1)
            actions.append(3)
        if(p1_bullets > 0):
            actions.append(2)
        if(p1_bullets >= 3):
            actions.append(4)
        return actions

    def p2_actions(self, state):
        p1_life = state[0]
        p1_bullets = state[1]

        p2_life = state[2]
        p2_bullets = state[3]

        actions = []
        if(p2_bullets >= 0):
            actions.append(1)
            actions.append(3)
        if(p2_bullets > 0):
            actions.append(2)
        if(p2_bullets >= 3):
            actions.append(4)
        return actions

    def evaluate(self, state, p1_move, p2_move):
        p1_life = state[0]
        p1_bullets = state[1]

        p2_life = state[2]
        p2_bullets = state[3]

        if(p1_move == 1):
            p1_bullets = min(p1_bullets+1, 9)
        if(p1_move == 2):
            if(p1_bullets <= 0):
                print("enter a valid p1 action")
                return None
            p1_bullets -= 1
            if(p2_move != 3):
                p2_life -= 1
        if(p1_move == 4):
            if(p1_bullets < 3):
                print("enter a valid p1 action")         
                return None
            p1_bullets -= 3
            p2_life -= 1
        if(p2_move == 1):
            p2_bullets = min(p2_bullets+1, 9)
        if(p2_move == 2):
            if(p2_bullets <= 0):
                print("enter a valid p2 action")
                return None
            p2_bullets -= 1
            if(p1_move != 3):
                p1_life -= 1
        if(p2_move == 4):
            if(p2_bullets < 3):
                print("enter a valid p2 action")
                return None
            p2_bullets -= 3
            p1_life -= 1

        p1_life = max(0, p1_life)
        p2_life = max(0, p2_life)

        return (p1_life, p1_bullets, p2_life, p2_bullets)


    #current implementation returns a list of states that p2 uses. This is
    #based on the idea that p2 chooses their action uniformly at random
    def p1_action_states(self, state, p1_move):
        states = []
        p2_actions = self.p2_actions(state)
        for p2_move in p2_actions:
            resulting_state = self.evaluate(state, p1_move, p2_move)
            resulting_reward = (float(resulting_state[2] == 0) - float(resulting_state[0] == 0))
            is_terminal = (resulting_state[0] == 0 or resulting_state[2] == 0)
            states.append((1.0/len(p2_actions),resulting_state, resulting_reward, is_terminal))
        return states

    def p2_action_states(self, state, p2_move):
        states = []
        for p1_move in self.p1_actions(state):
            states.append(self.evaluate(state, p1_move, p2_move))
        return states

    def is_terminal(self):
        return self.p1_life == 0 or self.p2_life == 0

    def mirror_state(self, state):
        return (state[2], state[3], state[0], state[1])

    def reset(self):
        self.p1_life = self.starting_life
        self.p2_life = self.starting_life

        self.p1_bullets = self.starting_bullets
        self.p2_bullets = self.starting_bullets
        return (self.p1_life, self.p1_bullets, self.p2_life, self.p2_bullets)

    def step(self, p1_move, p2_move, debug=False):
        state = (self.p1_life, self.p1_bullets, self.p2_life, self.p2_bullets)
        self.p1_life, self.p1_bullets, self.p2_life, self.p2_bullets = self.evaluate(state, p1_move, p2_move)
        # if(p1_move == 1):
        #     self.p1_bullets = min(self.p1_bullets+1, 9)
        # if(p1_move == 2):
        #     if(self.p1_bullets <= 0):
        #         print("enter a valid p1 action")
        #         return None
        #     self.p1_bullets -= 1
        #     if(p2_move != 3):
        #         self.p2_life -= 1
        # if(p1_move == 4):
        #     if(self.p1_bullets < 3):
        #         print("enter a valid p1 action")
        #         return None
        #     self.p1_bullets -= 3
        #     self.p2_life -= 1

        # if(p2_move == 1):
        #     self.p2_bullets = min(self.p2_bullets+1, 9)
        # if(p2_move == 2):
        #     if(self.p2_bullets <= 0):
        #         print("enter a valid p2 action")
        #         return None
        #     self.p2_bullets -= 1
        #     if(p1_move != 3):
        #         self.p1_life -= 1
        # if(p2_move == 4):
        #     if(self.p2_bullets < 3):
        #         print("enter a valid p2 action")
        #         return None
        #     self.p2_bullets -= 3
        #     self.p1_life -= 1

        # if debug:
        #     print(".")
        #     time.sleep(0.5)
        #     print(".")
        #     time.sleep(0.5)
        #     print(".")
        #     time.sleep(0.5)
        #     if(p1_move == 1):
        #         print("P1: RELOADING")
        #     elif(p1_move == 2):
        #         print("P1: BANG")
        #     elif(p1_move == 3):
        #         print("P1: BLOCK")
        #     elif(p1_move == 4):
        #         print("P1: SUPER BANG")

        #     if(p2_move == 1):
        #         print("P2: RELOADING")
        #     elif(p2_move == 2):
        #         print("P2: BANG")
        #     elif(p2_move == 3):
        #         print("P2: BLOCK")
        #     elif(p2_move == 4):
        #         print("P2: SUPER BANG")

        #     time.sleep(0.5)
        #     print("P1 health: ", p1_life, " bullets: ", p1_bullets)
        #     print("P2 health: ", p2_life, " bullets: ", p2_bullets)
        #     time.sleep(0.5)
    
        p1_reward = (float(self.p2_life == 0) - float(self.p1_life == 0))
        p2_reward = (float(self.p1_life == 0) - float(self.p2_life == 0))
        return ((self.p1_life, self.p1_bullets, self.p2_life, self.p2_bullets),
                p1_reward, 
                p2_reward, 
                self.is_terminal())