﻿using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    public string NextScene;
    public SpriteRenderer countDownRenderer;
    public Sprite[] countDown = new Sprite[3];
    public Text winnerText;
    private PlayerAudioSource audioSource;
    private AudioSource music;

    private Transform startMenu;
    private Transform endMenu;

    private List<Player> players;
    private Player[] leaderBoard = new Player[4];


    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Playable")
            .Select(pgo => pgo.GetComponent<Player>())
            .ToList();
        foreach (var p in players)
        {
            var stats = p.GetComponent<PlayerStatisticController>();
            stats.onWin += OnPlayerWin;
            stats.onDie += OnPlayerLoose;
        }

        startMenu = transform.Find("StartMenu");
        endMenu = transform.Find("EndMenu");
        audioSource = transform.Find("AudioSource").GetComponent<PlayerAudioSource>();
        music = GetComponent<AudioSource>();
        startMenu.gameObject.SetActive(true);
        endMenu.gameObject.SetActive(false);
        Time.timeScale = 0;
    }

    private void OnPlayerWin(object sender, EventArgs e)
    {
        for (int i = 0; i < leaderBoard.Length; i++)
        {
            if (leaderBoard[i] == null)
            {
                var player = ((GameObject)sender).GetComponent<Player>();
                leaderBoard[i] = player;
                player.gameObject.SetActive(false);
                break;
            }
        }
        CheckEndGame();
    }

    private void OnPlayerLoose(object sender, EventArgs e)
    {
        for (int i = leaderBoard.Length - 1; i >= 0; i--)
        {
            if (leaderBoard[i] == null)
            {
                var player = ((GameObject)sender).GetComponent<Player>();
                leaderBoard[i] = player;
                player.gameObject.SetActive(false);
                break;
            }
        }
        CheckEndGame();
    }

    private void CheckEndGame()
    {
        var activePlayers = players.Where(p => p.gameObject.activeInHierarchy);
        if (activePlayers.Count() <= 1)
        {
            var lastPlayer = activePlayers.FirstOrDefault();
            if (lastPlayer != null)
            {
                for (int i = 0; i < leaderBoard.Length; i++)
                {
                    if (leaderBoard[i] == null)
                    {
                        leaderBoard[i] = lastPlayer;
                        lastPlayer.gameObject.SetActive(false);
                        break;
                    }
                }
            }
            FinishGame();
        }
    }

    public void StartGame()
    {
        startMenu.gameObject.SetActive(false);
        StartCoroutine(CountDown());
    }

    public void RestartGame()
    {
        foreach (Player player in players)
        {
            player.gameObject.SetActive(true);
            player.Reset();
        }
        var cactuses = GameObject.FindGameObjectsWithTag("Cactus");
        if (cactuses.Any()) {
            foreach (var cactus in cactuses) {
                Destroy(cactus);
            }
        }
        
        leaderBoard = new Player[4];
        startMenu.gameObject.SetActive(false);
        endMenu.gameObject.SetActive(false);
        StartCoroutine(CountDown());
    }

    public void FinishGame()
    {
        var winnerNum = leaderBoard[0].GetComponent<PlayerStatisticController>().id;
        winnerText.text = "Winner: Player " + winnerNum.ToString();
        Time.timeScale = 0;
        endMenu.gameObject.SetActive(true);
    }

    public void LoadNextLevel()
    {
        SceneManager.LoadScene(NextScene);
    }

    private IEnumerator CountDown()
    {
        countDownRenderer.gameObject.SetActive(true);
        countDownRenderer.sprite = countDown[0];
        audioSource.PlayClip("beep");
        yield return new WaitForSecondsRealtime(1);
        countDownRenderer.sprite = countDown[1];
        audioSource.PlayClip("beep");
        yield return new WaitForSecondsRealtime(1);
        countDownRenderer.sprite = countDown[2];
        audioSource.PlayClip("beep");
        yield return new WaitForSecondsRealtime(1);
        audioSource.PlayClip("pistol");
        music.Play();
        countDownRenderer.gameObject.SetActive(false);
        foreach (var pl in players)
        {
            pl.Reset();
        }
        Time.timeScale = 1;
    }
}
