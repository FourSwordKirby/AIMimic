﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour {
    public static GameManager instance;

    public Player p1;
    public Player p2;
    public int p1Victories;
    public int p2Victories;
    public int currentRound;

    public static int roundToWin = 2;
    public static string p1Name;
    public static string p2Name;

    public CameraControls Camera;

    public float timeLimit;
    public float timeRemaining;

    public GameObject hitEffect;
    public GameObject blockEffect;

    public GameObject spawnPoint1;
    public GameObject spawnPoint2;
    public Text RoundText;
    public Text P1NameText;
    public Text P2NameText;
    public StockCount P1RoundCount;
    public StockCount P2RoundCount;
    public ComboText P1ComboText;
    public ComboText P2ComboText;
    public RematchUI rematchUI;

    //Shouldn't be here but hacking
    public List<AudioClip> sfx;

    private float countDown;
    private float roundEndTimer;
    public bool roundOver { get; private set; }
    private bool firstBoot = true;

    /// <summary>
    /// Used mainly for coordinating how long the AI should do moves. This means that it counts up 
    /// only when there is no hitstop
    /// </summary>
    public int currentFrame { get; private set; }

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        timeRemaining = timeLimit;
        roundOver = true;
    }

    void Update()
    {
        if (firstBoot)
        {
            firstBoot = false;
            roundOver = true;
            LoadSet();
        }

        //Hard Quit Macro
        if (Input.GetKey(KeyCode.Q) && Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.E))
        {
            Quit();
        }

        if (countDown > 0)
        {
            countDown -= Time.deltaTime;
            if((countDown > 1))
            {
                if(currentRound == 1)
                    RoundText.text = ((int)countDown).ToString();
                else
                    RoundText.text = "Round " + currentRound;

                p1.enabled = false;
                p2.enabled = false;
            }
            else 
            {
                roundOver = false;
                RoundText.text = "GO!";
                p1.enabled = true;
                p2.enabled = true;
            }
            return;
        }

        if (!roundOver)
        {
            RoundText.text = "";
            if (timeRemaining > 0 && !(p1.ActionFsm.CurrentState is SuspendState || p2.ActionFsm.CurrentState is SuspendState))
            {
                timeRemaining -= Time.deltaTime;
                currentFrame++;
            }

            if (p1.health <= 0 || p2.health <= 0 || timeRemaining <= 0)
            {
                Time.timeScale = 0.75f;
                currentRound++;

                if (timeRemaining > 0)
                    RoundText.text = "K.O.!!";
                else
                    RoundText.text = "Time Over.";

                if (p1.health > 0 || p2.health > 0)
                {
                    if (p1.health > p2.health)
                    {
                        p1Victories++;
                        RoundText.text += " P1 Win";
                    }
                    if (p2.health > p1.health)
                    {
                        p2Victories++;
                        RoundText.text += " P2 Win";
                    }
                }

                playSound("Success");

                if (p1Victories >= roundToWin)
                    RoundText.text = "P1 WINS";
                if (p2Victories >= roundToWin)
                    RoundText.text = "P2 WINS";

                //if(p1.grounded && !p1.stunned)
                    p1.enabled = false;
                //if(p2.grounded && !p2.stunned)
                    p2.enabled = false;

                P1RoundCount.SetStockCount(p1Victories);
                P2RoundCount.SetStockCount(p2Victories);

                roundOver = true;
            }
            return;
        }

        if (!((p1Victories >= roundToWin)|| (p2Victories >= roundToWin)))
        {
            if (roundEndTimer > 0)
            {
                roundEndTimer -= Time.deltaTime;
                if (roundEndTimer < 0.5f)
                {
                    if (roundEndTimer <= 0 || Controls.attackInputDown(p1) || Controls.attackInputDown(p2))
                        LoadRound();
                }
            }
        }
        else
        {
            if (roundEndTimer > 0.0f)
            {
                roundEndTimer -= Time.deltaTime;
                if (roundEndTimer <= 0.5f)
                {
                    rematchUI.Activate();
                }
                return;
            }
        }
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    public void LoadSet()
    {
        P1NameText.text = p1Name;
        P2NameText.text = p2Name;

        p1Victories = 0;
        p2Victories = 0;
        P1RoundCount.SetStockCount(p1Victories);
        P2RoundCount.SetStockCount(p2Victories);
        currentRound = 1;

        P1RoundCount.roundLimit = roundToWin;
        P2RoundCount.roundLimit = roundToWin;
        LoadRound();
    }

    void LoadRound()
    {
        playSound("Startup", true);

        Time.timeScale = 1.0f;
        countDown = 4.0f;
        roundEndTimer = 2.5f;

        timeRemaining = timeLimit;
        currentFrame = 0;

        p1.enabled = false;
        p2.enabled = false;
        p1.Reset();
        p2.Reset();
        p1.transform.position = spawnPoint1.transform.position;
        p2.transform.position = spawnPoint2.transform.position;
    }

    public static void SpawnBlockIndicator(Vector3 position)
    {
        GameObject blockEffect = Instantiate(instance.blockEffect);
        blockEffect.transform.position = position;
    }

    public static void SpawnHitIndicator(Vector3 position)
    {
        GameObject hitEffect = Instantiate(instance.hitEffect);
        hitEffect.transform.position = position;
    }

    public static void AddCombo(Player player)
    {
        player.comboCount++;
        if (player == instance.p1)
            instance.P1ComboText.SetText(player.comboCount);
        if (player == instance.p2)
            instance.P2ComboText.SetText(player.comboCount);
    }

    public static void EndCombo(Player player)
    {
        player.comboCount = 0;
        if (player == instance.p1)
            instance.P1ComboText.SetText(player.comboCount);
        if (player == instance.p2)
            instance.P2ComboText.SetText(player.comboCount);
    }

    public void playSound(string soundName, bool startingSound = false)
    {
        Vector3 position;
        if (!startingSound)
            position = Camera.transform.position;
        else
            position = Vector3.back * 10.0f;
        AudioClip sound = sfx.Find(x => x.name == soundName);
        AudioSource.PlayClipAtPoint(sound, position);
    }
}
