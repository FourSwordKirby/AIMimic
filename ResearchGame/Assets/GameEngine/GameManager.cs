using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public static List<Player> Players;
    public static CameraControls Camera;
    public static GameObject[] hit_boxes;

    public float timeLimit;
    public static float timeRemaining;

    public List<GameObject> spawnPoints;
    public Text RoundText;
    public Text P1Text;
    public Text P2Text;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            if (this != instance)
            {
                Destroy(this.gameObject);
            }
        }

        Players = new List<Player>(GameObject.FindObjectsOfType<Player>());
        if (Players == null)
        {
            Debug.Log("Cannot find players on the current scene.");
        }

        hit_boxes = GameObject.FindObjectsOfType<GameObject>().Where(x => x.GetComponent<Collider2D>() != null).ToArray();

        timeRemaining = timeLimit;
    }

    void Start()
    {
        for(int i = 0; i < Players.Count;i++)
        {
            Players[i].name = "Player " + i;
            Players[i].transform.position = spawnPoints[i].transform.position;
        }
    }

    void Update()
    {
        if (timeRemaining > 0)
            timeRemaining -= Time.deltaTime;
        if(timeRemaining < 0)
        {
            if (Input.GetKey(KeyCode.R))
                SceneManager.LoadScene(0);
        }
    }


    //Returns an open position in the specified direction that is at most maxDistance away
    public static Vector2 getOpenLocation(Parameters.InputDirection direction, Vector2 startingPosition, float maxDistance = 1.0f)
    {
        Vector2 newPosition = startingPosition;
        Vector2 increment = new Vector2(0, 0);
        float incrementDistance = 0.1f;
        float currentDistance = 0.0f;

        increment = Parameters.VectorToDir(direction) * incrementDistance;
        while (pointCollides(newPosition))
        {
            currentDistance += incrementDistance;
            newPosition += increment;

            if(currentDistance > maxDistance)
                return startingPosition;
        }
        return newPosition + (10f * currentDistance * increment);
    }

    //Checks if there are any collision boxes over the specified point
    private static bool pointCollides(Vector2 point)
    {
        return System.Array.Exists(hit_boxes, (GameObject hitbox) => hitbox.GetComponent<Collider2D>().bounds.Contains(point));
    }

    //public static Vector3 GetRespawnPosition()
    //{
    //    ////We will make this pick a location specified by the stage later
    //    //return stage.spawnPoints[Random.Range(0, stage.spawnPoints.Count)].transform.position;
    //}
}
