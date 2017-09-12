using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ComboText : MonoBehaviour {

    public Text selfText;
    public Image SplashImage;

    float transitionTime = 1.5f;
    float timer = 4.0f;

    void Update()
    {
        if(timer < transitionTime)
        {
            timer += Time.deltaTime;

            SplashImage.color = Color.Lerp(new Color(1.0f, 1.0f, 1.0f, 0.25f), new Color(1.0f, 1.0f, 1.0f, 1.0f), timer/0.25f);
            SplashImage.transform.rotation = Quaternion.Lerp(Quaternion.AngleAxis(0, Vector3.forward), Quaternion.AngleAxis(180.0f, Vector3.forward), timer / 0.25f);
            return;
        }
        SplashImage.color = new Color(1.0f, 1.0f, 1.0f, SplashImage.color.a - Time.deltaTime*0.75f);
        selfText.color = new Color(1.0f, 1.0f, 1.0f, selfText.color.a - Time.deltaTime*0.75f);
    }

    public void SetText(int comboCount)
    {
        if (comboCount == 0)
        {
            timer = transitionTime;
            return;
        }

        selfText.color = Color.white;
        this.selfText.text = comboCount + " Hits";
        if (comboCount > 2)
        {
            this.selfText.text += "!";
            timer = 0.0f;
        }
    }
}
