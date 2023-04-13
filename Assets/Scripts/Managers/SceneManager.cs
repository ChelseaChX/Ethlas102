using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    public int HighScore { get; private set; } = 0;

    public static SceneManager Instance;

    private void Awake()
    {
        // Only one instance of the SceneManager should exist at all times.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            // SceneManager should persist across scenes.
            DontDestroyOnLoad(this);
        }
    }

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void SetHighScore(int score)
    {
        if (score > this.HighScore)
        {
            this.HighScore = score;
        }
    }
}
