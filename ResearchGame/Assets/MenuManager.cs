using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;

public class MenuManager : MonoBehaviour {

    public bool enableLogging;
    public bool useAI;
    public bool p1ready;
    public bool p2ready;
    public int roundsToWin;

    public Image p1ReadyImage;
    public Image p2ReadyImage;
    public Text p1ReadyText;
    public Text p2ReadyText;
    public Image loggingImage;
    public Image useAIImage;
    public Dropdown roundDropdown;

    public Image instructions;

    public InputField p1Text;
    public InputField p2Text;

    void Update()
    {
        if (p1ready && p2ready)
        {
            GameManager.roundToWin = roundsToWin;
            GameManager.p1Name = p1Text.text;
            GameManager.p2Name = p2Text.text;
            //GameManager.recordData = enableLogging;
            //GameManager.useAI = useAI;
            SceneManager.LoadScene(1);
        }


        //Setting Values
        //P1 readiness
        if (p1Text.text.Trim() != "")
        {
            if (Input.GetButtonDown("P1 Attack") && !Input.GetKey(KeyCode.Alpha1))
            {
                if(!useAI || p2ready)
                    p1ready = !p1ready;
            }
        }
        else
            p1ready = false;

        //P2 readiness
        if (!useAI)
        {
            if (p2Text.text.Trim() != "")
            {
                if (Input.GetButtonDown("P2 Attack") && !Input.GetKey(KeyCode.U))
                    p2ready = !p2ready;
            }
            else
                p2ready = false;
        }
        else
        {
            name = p2Text.text.Trim();
            if (name == "test")
                p2ready = true;
            //Do a check to see if the name is in the DB
        }

        //Logging Usage
        /*
        if ((Input.GetButtonDown("P1 Jump") || Input.GetButtonDown("P2 Jump")) && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.UpArrow))
            enableLogging = !enableLogging;

        //AI Usage
        if ((Input.GetButtonDown("P1 Special") || Input.GetButtonDown("P2 Special")) && !Input.GetKey(KeyCode.Alpha2) && !Input.GetKey(KeyCode.I))
        {
            useAI = !useAI;
            p2Text.text = "";
            p1ready = false;
            p2ready = false;
        }
        */

        if ((Input.GetButtonDown("P1 Enhance") || Input.GetButtonDown("P2 Enhance")) && !Input.GetKey(KeyCode.Alpha4) && !Input.GetKey(KeyCode.P))
        {
            instructions.gameObject.SetActive(!instructions.gameObject.activeSelf);
        }

        if(instructions.gameObject.activeSelf)
        {
            p1ready = false;
            p2ready = false;
        }

        //Round Count
        roundsToWin = roundDropdown.value + 1;

        //Name pruning
        p1Text.text = Regex.Replace(p1Text.text, @"[^A-Za-z]+", "");
        p2Text.text = Regex.Replace(p2Text.text, @"[^A-Za-z]+", "");



        //Graphics updates
        if (p1Text.text.Trim() != "")
        {
            if (p1ready)
            {
                p1ReadyImage.color = Color.white;
                p1ReadyText.text = "Confirmed";
            }
            else
            {
                p1ReadyImage.color = Color.grey;
                p1ReadyText.text = "Ready?";
            }
        }
        else
            p1ReadyImage.color = Color.black * 0.2f;


        if (p2Text.text.Trim() != "")
        { 
            if (p2ready)
            {
                p2ReadyImage.color = Color.white;
                p2ReadyText.text = "Confirmed";
            }
            else
            {
                p2ReadyImage.color = Color.grey;
                p2ReadyText.text = "Ready?";
            }
        }
        else
            p2ReadyImage.color = Color.black * 0.2f;


        if (useAI)
        {
            enableLogging = false;
            useAIImage.color = Color.white;
        }
        else
            useAIImage.color = Color.grey;

        if (enableLogging)
            loggingImage.color = Color.white;
        else
            loggingImage.color = Color.grey;
    }
}
