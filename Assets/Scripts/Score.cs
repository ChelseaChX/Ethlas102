using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    private void Start()
    {
        this.GetComponent<Text>().text = (SceneManager.Instance.HighScore).ToString();
    }
}
