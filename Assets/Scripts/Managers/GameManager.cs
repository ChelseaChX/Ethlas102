using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Text score;
    [SerializeField] private Text gameOverScore;
    [SerializeField] private GameObject gameOverScreen;

    [Header("Block Settings")]
    [SerializeField] private GameObject blockHolder;
    [SerializeField] private GameObject baseBlock;
    [SerializeField] private float blockStartOffset = 10f;
    [SerializeField] private float blockSpeed = 1f;
    [SerializeField] private float newBlockWaitTime = 1f;
    [SerializeField] private float blockBufferSize = 1f;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraYOffset = 0.1f;
    [SerializeField] private float cameraShakeIntensity = 1f;
    [SerializeField] private float cameraShakeDuration = 1f;

    private AudioSource boomSfx;
    private bool perfectBlock = false;

    public static GameManager Instance { get; private set; }
    public UnityEvent InputReceived { get; private set; }

    public float BlockStartOffset { get { return this.blockStartOffset; } private set { } }
    public float BlockSpeed { get { return this.blockSpeed; } private set { } }
    public float NewBlockWaitTime { get { return this.newBlockWaitTime; } private set { } }
    public float BlockBufferSize { get { return this.blockBufferSize; } private set { } }

    private void Awake()
    {
        // Only one instance of the GameManager should exist at all times.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        if (this.InputReceived == null)
        {
            this.InputReceived = new UnityEvent();
        }

        this.boomSfx = this.GetComponent<AudioSource>();
    }

    public void UpdateScene()
    {
        if (this.perfectBlock)
        {
            this.StartCoroutine(this.CameraUpwardsLerp());
        }
        else
        {
            this.StartCoroutine(this.CameraUpwardsShake());
            this.boomSfx.Play();
        }
        
        this.score.text = this.GetScore().ToString();
    }

    public void TriggerPerfectAlignment()
    {
        this.perfectBlock = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && this.InputReceived != null)
        {
            this.InputReceived.Invoke();
        }
    }

    private int GetScore()
    {
        return this.blockHolder.transform.childCount - 2;
    }

    private IEnumerator CameraUpwardsShake()
    {
        float startTime = Time.time;
        float timeElapsed = Time.time - startTime;
        Vector3 originalPos = this.mainCamera.transform.position;

        while (timeElapsed < this.cameraShakeDuration)
        {
            this.mainCamera.transform.position = originalPos + Random.insideUnitSphere * this.cameraShakeIntensity;
            yield return null;
            timeElapsed = Time.time - startTime;
        }

        this.mainCamera.transform.position = new Vector3(originalPos.x, originalPos.y + cameraYOffset, originalPos.z);
    }

    private IEnumerator CameraUpwardsLerp()
    {
        float startTime = Time.time;
        float elapsedTime = Time.time - startTime;
        Vector3 originalPos = this.mainCamera.transform.position;
        Vector3 endPos = new Vector3(originalPos.x, originalPos.y + cameraYOffset, originalPos.z);

        while (elapsedTime < this.cameraShakeDuration)
        {
            this.mainCamera.transform.position = Vector3.Lerp(originalPos, endPos, 1 / this.cameraShakeDuration * (Time.time - startTime));
            yield return null;
            elapsedTime = Time.time - startTime;
        }

        this.mainCamera.transform.position = endPos;
    }

    public void GameOver()
    {
        int finalScore = this.GetScore();
        this.gameOverScore.text = finalScore.ToString();
        this.gameOverScreen.SetActive(true);
        SceneManager.Instance.SetHighScore(finalScore);
    }
}
