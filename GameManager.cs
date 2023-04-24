using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public LevelBuilder levelBuilder;
    public GameObject nextButton;
    public Button upButton;
    public Button downButton;
    public Button leftButton;
    public Button rightButton;
    public Text timerText;
    public Text gameOverText;
    public float timeLimit = 60f;
    private bool readyForInput;
    private Player player;
    private Queue<Vector2> moveQueue = new Queue<Vector2>();
    private float remainingTime;

    void Start() {
        nextButton.SetActive(false);
        upButton.onClick.AddListener(() => MovePlayer(Vector2.up));
        downButton.onClick.AddListener(() => MovePlayer(Vector2.down));
        leftButton.onClick.AddListener(() => MovePlayer(Vector2.left));
        rightButton.onClick.AddListener(() => MovePlayer(Vector2.right));
        ResetScene();
    }

    void Update() {
        if (moveQueue.Count > 0 && readyForInput) {
            readyForInput = false;
            player.Move(moveQueue.Dequeue());
            nextButton.SetActive(IsLevelComplete());
        } else {
            readyForInput = true;
        }

        UpdateTimer();
    }

    void UpdateTimer() {
        if (nextButton.activeSelf) { // Vérifiez si le bouton Next est actif
        return; // Si c'est le cas, sortez de la fonction sans mettre à jour le chronomètre
    }
        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0) {
            remainingTime = 0;
            upButton.interactable = false;
            downButton.interactable = false;
            leftButton.interactable = false;
            rightButton.interactable = false;
            gameOverText.gameObject.SetActive(true);
        }
        timerText.text = $"Time: {Mathf.FloorToInt(remainingTime)}";
    }

    void MovePlayer(Vector2 direction) {
        if (remainingTime > 0) {
            moveQueue.Enqueue(direction);
        }
    }

    public void NextLevel() {
        nextButton.SetActive(false);
        levelBuilder.NextLevel();
        ResetScene();
    }

    public void ResetScene() {
        remainingTime = timeLimit;
        gameOverText.gameObject.SetActive(false);
        nextButton.SetActive(false);
        StartCoroutine(ResetSceneAsync());
        upButton.interactable = true;
    downButton.interactable = true;
    leftButton.interactable = true;
    rightButton.interactable = true;
    }

    bool IsLevelComplete() {
        Box[] boxes = FindObjectsOfType<Box>();
        foreach (var box in boxes) {
            if (!box.onCrox) return false;
        }
        return true;
    }

    IEnumerator ResetSceneAsync() {
        if (SceneManager.sceneCount > 1) {
            AsyncOperation async = SceneManager.UnloadSceneAsync("LevelScene");
            while (!async.isDone) {
                yield return null;
                Debug.Log("Unloading");
            }
            Debug.Log("Unloading done.");
            Resources.UnloadUnusedAssets();
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LevelScene", LoadSceneMode.Additive);
        while (!asyncLoad.isDone) {
            yield return null;
            Debug.Log("Loading");
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("LevelScene"));
        levelBuilder.Build();
        player = FindObjectOfType<Player>();
        Debug.Log("Level loaded.");
        readyForInput = true;
    }
}
