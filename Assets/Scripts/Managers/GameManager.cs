﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;            
    public float m_StartDelay = 3f;             
    public float m_EndDelay = 3f;               
    public CameraControl m_CameraControl;       
    public Text m_MessageText;                  
    public GameObject[] m_TankPrefabs;
    public TankManager[] m_Tanks;               
    public List<Transform> wayPointsForAI;

    private int m_RoundNumber;                  
    private WaitForSeconds m_StartWait;         
    private WaitForSeconds m_EndWait;           
    private TankManager m_RoundWinner;          
    private TankManager m_GameWinner;           


    private void Start()
    {
        print("starting");
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        StartCoroutine(GameLoop());
    }


    private void SpawnAllTanks()
    {
        m_Tanks[0].m_Instance =
            Instantiate(m_TankPrefabs[0], m_Tanks[0].m_SpawnPoint.position, m_Tanks[0].m_SpawnPoint.rotation) as GameObject;
        // set player number as 1 for ownself
        m_Tanks[0].m_PlayerNumber = 1;
        m_Tanks[0].SetupPlayerTank();

        for (int i = 1; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefabs[i], m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].SetupAI(wayPointsForAI);
        }
    }


    private void SetCameraTargets()
    {
        Transform[] targets = new Transform[m_Tanks.Length];

        for (int i = 0; i < targets.Length; i++)
            targets[i] = m_Tanks[i].m_Instance.transform;

        m_CameraControl.m_Targets = targets;
    }


    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());
        print(m_Tanks);
        if (m_GameWinner != null) SceneManager.LoadScene(0);
        else StartCoroutine(GameLoop());
    }


    private IEnumerator RoundStarting()
    {
        ResetAllTanks();
        DisableTankControl();

        m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        m_MessageText.text = $"ROUND {m_RoundNumber}";

        yield return m_StartWait;
    }


    private IEnumerator RoundPlaying()
    {
        EnableTankControl();

        m_MessageText.text = string.Empty;

        while (!OneTankLeft()) yield return null;
    }


    private IEnumerator RoundEnding()
    {
        DisableTankControl();

        m_RoundWinner = null;

        m_RoundWinner = GetRoundWinner();
        //give penalty to player who wins
        if(m_RoundWinner.m_PlayerNumber == 1){
            m_RoundWinner.m_Wins += 1;
        }
        else if (m_RoundWinner.m_PlayerNumber != 1){
            m_RoundWinner.m_Wins -= 1;
        }

        m_GameWinner = GetGameWinner();

        string message = EndMessage();
        m_MessageText.text = message;

        yield return m_EndWait;
    }


    private bool OneTankLeft()
    {
        int numTanksLeft = 0;

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Instance.activeSelf) {
                numTanksLeft++;
            }
            else if (!m_Tanks[0].m_Instance.activeSelf){
                return true;
            }
        }

        return numTanksLeft <= 1;
    }

    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {   

            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        return null;
    }

    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        return null;
    }


    private string EndMessage()
    {
        var sb = new StringBuilder();

        
        if(m_RoundWinner.m_PlayerNumber==1){
            sb.Append("You Won!");
        }
        else sb.Append("You died a terrible death get wrecked!");

        sb.Append("\n\n\n\n");

        sb.AppendLine($"{m_Tanks[0].m_ColoredPlayerText}: {m_Tanks[0].m_Wins} WINS");

        if (m_GameWinner != null)

            sb.Append($"{m_GameWinner.m_ColoredPlayerText} WINS THE GAME!");

        return sb.ToString();
    }


    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].Reset();
    }


    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].EnableControl();
    }


    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++) m_Tanks[i].DisableControl();
    }

}