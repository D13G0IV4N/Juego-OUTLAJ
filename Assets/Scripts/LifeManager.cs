using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LifeManager : MonoBehaviour
{
    public Image[] hearts;
    public int maxLives = 4;
    public int currentLives;

    public GameObject gameOverCanvas;
    public VideoPlayer gameOverVideoPlayer;
    public PlayerMovement playerMovement;

    private bool isGameOver = false;

    void Start()
    {
        currentLives = maxLives;
        UpdateHearts();

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        isGameOver = false;
    }

    void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            RestartLevel();
        }
    }

    public void LoseLife()
    {
        if (currentLives <= 0 || isGameOver) return;

        currentLives--;
        UpdateHearts();

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public bool RestoreLife()
    {
        if (isGameOver || currentLives >= maxLives)
        {
            return false;
        }

        currentLives++;
        UpdateHearts();
        return true;
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].enabled = i < currentLives;
        }
    }

    void GameOver()
    {
        isGameOver = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;

            SpriteRenderer sr = playerMovement.GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.enabled = false;
        }

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (gameOverVideoPlayer != null)
            gameOverVideoPlayer.Play();
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
