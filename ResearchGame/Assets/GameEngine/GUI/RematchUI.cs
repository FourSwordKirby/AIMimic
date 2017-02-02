using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RematchUI : MonoBehaviour {

    public Image ClassifyPanel;
    public Image RematchPanel;

    public Image OffensePanel1;
    public Image DefensePanel1;

    public Image OffensePanel2;
    public Image DefensePanel2;

    public Image YesPanel;
    public Image NoPanel;

    public int p1Class;
    public int p2Class;

    public bool classifierEnabled;
    public int position;

    float delay = 0.5f;

    // Update is called once per frame
    void Update () {
        if(delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }


        if (!classifierEnabled)
        {
            position = 1;
            ClassifyPanel.gameObject.SetActive(false);
        }
        else
        {
            ClassifyPanel.gameObject.SetActive(true);
            if (Controls.getDirection(GameManager.instance.p1).y < 0 || Controls.getDirection(GameManager.instance.p2).y < 0)
            {
                position = 1;
            }
            if (Controls.getDirection(GameManager.instance.p1).y > 0 || Controls.getDirection(GameManager.instance.p2).y > 0)
            {
                position = 0;
            }
        }

        if (position == 0)
        {
            ClassifyPanel.color = Color.white - Color.black * 0.4f;
            RematchPanel.color = Color.white - Color.black * 0.8f;
            if (Controls.attackInputDown(GameManager.instance.p1))
            {
                OffensePanel1.color = Color.white;
                DefensePanel1.color = Color.white - Color.black * 0.6f;
                p1Class = 1;
            }
            if(Controls.jumpInputDown(GameManager.instance.p1))
            {
                OffensePanel1.color = Color.white - Color.black * 0.6f;
                DefensePanel1.color = Color.white;
                p1Class = 2;
            }
            if (Controls.attackInputDown(GameManager.instance.p2))
            {
                OffensePanel2.color = Color.white;
                DefensePanel2.color = Color.white - Color.black * 0.6f;
                p2Class = 1;
            }
            if (Controls.jumpInputDown(GameManager.instance.p2))
            {
                OffensePanel2.color = Color.white - Color.black * 0.6f;
                DefensePanel2.color = Color.white;
                p2Class = 2;
            }
        }
        if(position == 1)
        {
            ClassifyPanel.color = Color.white - Color.black * 0.8f;
            RematchPanel.color = Color.white - Color.black * 0.4f;
            if (Controls.attackInputDown(GameManager.instance.p1) || Controls.attackInputDown(GameManager.instance.p2))
            {
                this.gameObject.SetActive(false);
                GameManager.instance.LoadSet();
            }
            if (Controls.jumpInputDown(GameManager.instance.p1) || Controls.jumpInputDown(GameManager.instance.p2))
            {
                GameManager.instance.Quit();
            }
        }
    }

    public void Activate()
    {
        this.gameObject.SetActive(true);
        delay = 0.5f;
    }
}
