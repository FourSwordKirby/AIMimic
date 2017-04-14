class BangEnv(object):
    def __init__(self, starting_life, starting_bullets):
        self.actions = [1,2,3,4]

        self.starting_life = starting_life
        self.starting_bullets = starting_bullets

        self.p1_life = starting_life
        self.p2_life = starting_life

        self.p1_bullets = starting_bullets
        self.p2_bullets = starting_bullets

    def is_terminal(self):
        return self.p1_life == 0 or self.p2_life == 0

    def is_terminal_state(self, state):
        return state[0] == 0 or state[2] == 0

    def p1_state_reward(self, state):
        return float(state[2] == 0)- float(state[0] == 0)
    
    def p2_state_reward(self, state):
        return float(state[0] == 0)- float(state[2] == 0)

    def get_p1_actions(self, state):
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

    def get_p2_actions(self, state):
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
            p1_bullets += 1
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
            p2_bullets += 1
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

    def current_state(self):
        return (self.p1_life, self.p1_bullets, self.p2_life, self.p2_bullets)

    def p1_action_states(self, state, p1_move):
        states = []
        for p2_move in self.get_p2_actions(state):
            states.append(self.evaluate(state, p1_move, p2_move))
        return states

    def p2_action_states(self, state, p2_move):
        states = []
        for p1_move in self.get_p1_actions(state):
            states.append(self.evaluate(state, p1_move, p2_move))
        return states

    def reset(self):
        self.p1_life = self.starting_life
        self.p2_life = self.starting_life

        self.p1_bullets = self.starting_bullets
        self.p2_bullets = self.starting_bullets

    def step(self, p1_move, p2_move, debug=False):
        if(p1_move == 1):
            self.p1_bullets += 1
        if(p1_move == 2):
            if(self.p1_bullets <= 0):
                print("enter a valid p1 action")
                return None
            self.p1_bullets -= 1
            if(p2_move != 3):
                self.p2_life -= 1
        if(p1_move == 4):
            if(self.p1_bullets < 3):
                print("enter a valid p1 action")
                return None
            self.p1_bullets -= 3
            self.p2_life -= 1

        if(p2_move == 1):
            self.p2_bullets += 1
        if(p2_move == 2):
            if(self.p2_bullets <= 0):
                print("enter a valid p2 action")
                return None
            self.p2_bullets -= 1
            if(p1_move != 3):
                self.p1_life -= 1
        if(p2_move == 4):
            if(self.p2_bullets < 3):
                print("enter a valid p2 action")
                return None
            self.p2_bullets -= 3
            self.p1_life -= 1

        if debug:
            print(".")
            time.sleep(0.5)
            print(".")
            time.sleep(0.5)
            print(".")
            time.sleep(0.5)
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
            print("P1 health: ", p1_life, " bullets: ", p1_bullets)
            print("P2 health: ", p2_life, " bullets: ", p2_bullets)
            time.sleep(0.5)
    
        return (float(self.p2_life == 0)- float(self.p1_life == 0), 
                float(self.p1_life == 0) - float(self.p2_life == 0), 
                (self.p1_life, self.p1_bullets, self.p2_life, self.p2_bullets))