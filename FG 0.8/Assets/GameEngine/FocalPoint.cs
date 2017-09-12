using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FocalPoint : MonoBehaviour
{
    public List<GameObject> targets;

    void Awake()
    {
        targets = new List<GameObject>();
    }

    public void addTargets(GameObject target)
    {
        targets.Add(target);
    }

    public void removeTargets(GameObject target)
    {
        targets.Remove(target);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = Vector3.zero;

        foreach (GameObject target in targets)
        {
            newPos += target.transform.position;
        }

        this.transform.position = newPos / (targets.Count);
    }
}