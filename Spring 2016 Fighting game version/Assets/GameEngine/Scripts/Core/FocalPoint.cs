using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FocalPoint : MonoBehaviour {

    public List<Mobile> targets;

    void Awake()
    {
        targets = new List<Mobile>();
    }

    public void addTargets(Mobile target)
    {
        targets.Add(target);
    }

    public void removeTargets(Mobile target)
    {
        targets.Remove(target);
    }

	// Update is called once per frame
	void Update () 
    {
        Vector3 newPos = Vector3.zero;

        foreach (Mobile target in targets)
        {
            newPos += target.transform.position;
        }

        this.transform.position = newPos / (targets.Count);
	}
}
