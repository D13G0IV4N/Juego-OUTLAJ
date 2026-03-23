using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;


public class FinalBossGameOver : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private FinalBossPlayerHealth playerHealth;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private VideoPlayer gameOverVideoPlayer;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private string sceneToRestart = "FinalBoss";

    private bool isGameOver = false;

    private void Start()
    {
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(false);

        if (playerHealth != null)
            playerHealth.Died += OnPlayerDied;
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.Died -= OnPlayerDied;
    }

    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene(sceneToRestart);
        }
    }

    private void OnPlayerDied()
    {
        isGameOver = true;

        if (playerMovement != null)
            playerMovement.enabled = false;

        SpriteRenderer sr = null;
        if (playerMovement != null)
            sr = playerMovement.GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.enabled = false;

        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        if (gameOverVideoPlayer != null)
            gameOverVideoPlayer.Play();
    }
}