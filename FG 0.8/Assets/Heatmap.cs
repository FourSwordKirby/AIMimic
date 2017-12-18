using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Heatmap : MonoBehaviour {

    public string playerName;

    public SpriteRenderer p1Marker;
    public SpriteRenderer p2Marker;

    // Use this for initialization
    void Start() {
        GameManager.instance.p1.gameObject.SetActive(false);
        GameManager.instance.p2.gameObject.SetActive(false);

        List<GameEvent> events = GetPastSessions().SelectMany(i => i).ToList();


        Color prevColor = Color.clear;
        for(int i = 0; i < events.Count; i++)
        {
            GameEvent e = events[i];

            GameObject p1 = Instantiate(p1Marker.gameObject);

            p1.SetActive(true);

            prevColor = SetColor(p1, e, true, prevColor);

            p1.transform.position = e.p1Position;
        }
    }


    Color SetColor(GameObject p, GameEvent e, bool isPlayer1, Color attackColor)
    {
        SpriteRenderer s = p.GetComponent<SpriteRenderer>();

        if (!isPlayer1)
            return Color.clear;
        else
        {
            switch (e.p1Status)
            {
                case PlayerStatus.Crouch:
                    s.color = Color.grey;
                    s.sortingOrder = 1;
                    break;
                case PlayerStatus.Moving:
                    s.color = Color.cyan;
                    s.sortingOrder = 1;
                    break;
                case PlayerStatus.Dashing:
                    s.color = Color.green;
                    break;
                case PlayerStatus.AirDashing:
                    s.color = Color.green;
                    break;
                case PlayerStatus.StandAttack: //When the opponent has the stand hitbox out
                    s.color = Color.red;
                    break;
                case PlayerStatus.LowAttack: //When the opponent has the low hitbox out
                    s.color = Color.magenta;
                    s.sortingOrder = 1;
                    break;
                case PlayerStatus.OverheadAttack: //When the opponent has the overhead hitbox out
                    s.color = new Color(139 / 255.0f, 0, 0);
                    s.sortingOrder = 2;
                    break;
                case PlayerStatus.AirAttack: //When the opponent has the AirAttack hitbox out
                    s.color = new Color(255/255.0f, 182/ 255.0f, 193 / 255.0f); //Pink
                    s.sortingOrder = 2;
                    break;
                case PlayerStatus.DP: //When the opponent has the Dp hitbox out
                    s.gameObject.name = "wwww";
                    s.color = new Color(139 / 255.0f, 0, 139 / 255.0f); //Purple
                    s.sortingOrder = 2;
                    break;
                case PlayerStatus.Recovery:
                    s.color = attackColor;
                    s.sortingOrder = 2;
                    break;
                default:
                    s.color = Color.white;
                    break;
            }
        }

        s.color -= Color.black * 0.5f;
        return s.color + Color.black * 0.5f;
    }

    List<List<GameEvent>> GetPastSessions()
    {
        string directoryPath = Application.streamingAssetsPath + "/PlayerLogs/" + playerName + "/";
        Directory.CreateDirectory(directoryPath);

        DirectoryInfo playerDir = new DirectoryInfo(directoryPath);
        List<List<GameEvent>> playerHistory = new List<List<GameEvent>>();
        List<FileInfo> infolist = playerDir.GetFiles().Where(x => !x.Name.EndsWith(".meta")).ToList();
        foreach(FileInfo info in infolist)
        {
            string filePath = info.FullName;
            
            string contents = File.ReadAllText(filePath);
            string[] serializeObjects = contents.Split(new string[] { "~~~~" }, System.StringSplitOptions.RemoveEmptyEntries);
            List<GameEvent> mySnapshots = new List<GameEvent>();
            for (int i = 0; i < serializeObjects.Length; i++)
            {
                mySnapshots.Add(JsonUtility.FromJson<GameEvent>(serializeObjects[i]));
            }
            playerHistory.Add(mySnapshots);
        }
        return playerHistory;
    }
}
