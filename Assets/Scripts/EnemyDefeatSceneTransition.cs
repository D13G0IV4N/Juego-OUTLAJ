using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;


public class EnemyDefeatSceneTransition : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private EnemyHealth targetEnemy;

    [Header("UI (Optional)")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text messageText;
    [SerializeField] private TMP_Text messageTextTMP;
    [SerializeField] [TextArea(2, 4)] private string message = "Lo lograste";

    [Header("Scene Loading")]
    [SerializeField] private string nextSceneName = "Final";
    [SerializeField] private KeyCode continueKey = KeyCode.Space;

    [Header("Transition Mode")]
    [SerializeField] private bool autoContinueAfterDelay = true;
    [SerializeField] private float autoContinueDelay = 2f;

    [Header("Optional Gameplay Lock")]
    [SerializeField] private bool pauseTimeScale = false;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    private bool waitingForContinue;
    private bool transitionStarted;
    private Coroutine autoContinueCoroutine;

    private void Awake()
    {
        ApplyMessage();
        SetPanelVisible(false);
    }

    private void OnEnable()
    {
        if (targetEnemy != null)
        {
            targetEnemy.Died += OnTargetEnemyDefeated;
        }
    }

    private void OnDisable()
    {
        if (targetEnemy != null)
        {
            targetEnemy.Died -= OnTargetEnemyDefeated;
        }

        if (autoContinueCoroutine != null)
        {
            StopCoroutine(autoContinueCoroutine);
            autoContinueCoroutine = null;
        }

        if (pauseTimeScale && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (!waitingForContinue)
        {
            return;
        }

        if (Input.GetKeyDown(continueKey))
        {
            ContinueToNextScene();
        }
    }

    private void OnTargetEnemyDefeated(EnemyHealth defeatedEnemy)
    {
        if (defeatedEnemy != targetEnemy || transitionStarted)
        {
            return;
        }

        transitionStarted = true;
        LockGameplay();

        if (autoContinueAfterDelay)
        {
            autoContinueCoroutine = StartCoroutine(AutoContinueRoutine());
            return;
        }

        ShowPanelAndPause();
    }

    private IEnumerator AutoContinueRoutine()
    {
        ApplyMessage();

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        yield return new WaitForSeconds(autoContinueDelay);

        ContinueToNextScene();
    }

    private void ShowPanelAndPause()
    {
        ApplyMessage();
        SetPanelVisible(true);
        waitingForContinue = true;

        if (pauseTimeScale)
        {
            Time.timeScale = 0f;
        }
    }

    private void LockGameplay()
    {
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }
    }

    private void ContinueToNextScene()
    {
        if (pauseTimeScale && Time.timeScale == 0f)
        {
            Time.timeScale = 1f;
        }

        waitingForContinue = false;
        SetPanelVisible(false);

        if (!string.IsNullOrWhiteSpace(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void ApplyMessage()
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (messageTextTMP != null)
        {
            messageTextTMP.text = message;
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(isVisible);
        }
    }
}