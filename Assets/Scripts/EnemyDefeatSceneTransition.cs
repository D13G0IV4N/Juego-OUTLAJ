using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EnemyDefeatSceneTransition : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private EnemyHealth targetEnemy;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text messageText;
    [SerializeField] private TMP_Text messageTextTMP;
    [SerializeField] [TextArea(2, 4)]
    private string message = "Defeat the boss to help Rodrigo escape";

    [Header("Scene Loading")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;

    [Header("Optional Gameplay Lock")]
    [SerializeField] private bool pauseTimeScale = true;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;

    private bool waitingForContinue;

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
        if (defeatedEnemy != targetEnemy || waitingForContinue)
        {
            return;
        }

        ShowPanelAndPause();
    }

    private void ShowPanelAndPause()
    {
        ApplyMessage();
        SetPanelVisible(true);
        waitingForContinue = true;

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        if (pauseTimeScale)
        {
            Time.timeScale = 0f;
        }
    }

    private void ContinueToNextScene()
    {
        waitingForContinue = false;
        SetPanelVisible(false);

        if (pauseTimeScale)
        {
            Time.timeScale = 1f;
        }

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
