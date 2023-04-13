using UnityEngine;

public class PlayAgainButton : MonoBehaviour
{
    public void PlayAgain()
    {
        SceneManager.Instance.RestartGame();
    }
}
