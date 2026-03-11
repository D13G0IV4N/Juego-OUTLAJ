using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Intro";

    public void Play()
    {   
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Salir (en el Editor solo se verá este mensaje).");
    }
}