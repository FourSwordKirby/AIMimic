using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionboxManager : MonoBehaviour {

    public List<Collisionbox> CollisionBoxes;
    private Dictionary<string, Collisionbox> AllCollisionBoxes;

  	void Awake()
    {
        AllCollisionBoxes = new Dictionary<string, Collisionbox>();
        //Initializes internal dictionary
        foreach(Collisionbox collisionbox in CollisionBoxes)
        {
           AllCollisionBoxes.Add(collisionbox.name, collisionbox);
        }
    }

    public void activateHitBox(string frameName)
    {
        AllCollisionBoxes[frameName].Activate();
    }

    public void deactivateHitBox(string frameName)
    {
        AllCollisionBoxes[frameName].Deactivate();
    }

    public void deactivateAllHitboxes()
    {
        foreach(Collisionbox col in AllCollisionBoxes.Values){
            col.Deactivate();
        }
    }

    public void activateAllHitboxes()
    {
        foreach (Collisionbox col in AllCollisionBoxes.Values)
        {
            col.Activate();
        }
    }

    public Collisionbox getHitbox(string frameName)
    {
        return AllCollisionBoxes[frameName];
    }
}
