using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanVisualizer : MonoBehaviour {
    public TransitionSolver transitionSolver;

    public int maxDisplay;
    public GameObject visualizationSprite;

    public List<List<GameObject>> visualizations = new List<List<GameObject>>();

    List<Transition> currentPlan;
    // Update is called once per frame
    void Update () {
        List<Transition> plan = transitionSolver.desiredTransitions;

        if(plan != currentPlan)
        {
            foreach(List<GameObject> pastVisualization in visualizations)
            {
                for (int i = 0; i < pastVisualization.Count; i++)
                {
                    GameObject node = pastVisualization[i];
                    node.GetComponent<SpriteRenderer>().color *= 0.8f; 
                }
            }

            currentPlan = plan;
            List<GameObject> currentVisualization = new List<GameObject>();
            for (int i = 0; i < plan.Count; i++)
            {
                GameObject node = Instantiate(visualizationSprite);
                node.GetComponent<SpriteRenderer>().enabled = true;
                node.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.clear, (float)i / (float)plan.Count);
                node.transform.position = new Vector2(plan[i].result.xPos / 2, plan[i].result.yPos);

                //Do some additional things with interpolations, visuals etc.

                currentVisualization.Add(node);
            }

            visualizations.Add(currentVisualization);
            if(visualizations.Count > maxDisplay)
            {
                foreach (GameObject node in visualizations[0])
                    Destroy(node);
                visualizations.RemoveAt(0);
            }
        }
    }
}
