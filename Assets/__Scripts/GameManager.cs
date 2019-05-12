using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using EZCameraShake;
using UnityStandardAssets.ImageEffects;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; set; } = null;
    private static float _score;

    public bool gameOver { get; set; } = true;

    public static float Score {
        get => _score;
        set {
            _score = value;
            Menu.Instance.gameScoreTxt.text = value.ToString("0");
        }
    }

    private void Start() {
        if (!Menu.IsLoaded) {
            SceneManager.LoadScene(0);
            return;
        }
        Instance = this;
        NewGame();
    }

    void NewGame() {
        AudioManager.Instance.PlayGameMusic();
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blur = .15f;
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blurSpread = .15f;
        Score = 0;
        GetComponent<Spawner>().Init();
        gameOver = false;

    }

    void Update() {
        if (gameOver)
            return;
        if (Input.touchCount > 0) {
            for (int i = 0; i < Input.touchCount; i++) {
                if (Input.GetTouch(i).phase != TouchPhase.Began)
                    continue;
                var worldTouchPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                for (int j = 0; j < Ball.SpawnedBalls.Count; j++) {
                    if (Vector2.Distance(Ball.SpawnedBalls[j].transform.position, worldTouchPos) <= Ball.SpawnedBalls[j].Diameter / 2.0f) {
                        var ballType = Ball.SpawnedBalls[j].BallType;
                        Ball.SpawnedBalls[j].OnTap();
                        if (ballType == BallType.black)
                            return;
                        break;
                    }
                }
            }
        }
    }

    private void FixedUpdate() {
        if (gameOver)
            return;
        float timeScaleDelta = Time.deltaTime / 60f;
        if (Score > 60)
            timeScaleDelta -= timeScaleDelta / Random.Range(1f, 4f);

        Time.timeScale += timeScaleDelta;
        Score += Time.deltaTime / Time.timeScale;
    }

    public void GameOver(Ball ball) {
        Spawner.Instance.enabled = false;
        Spawner.Instance.StopAllCoroutines();
        AudioManager.Instance.musicAS.Stop();
        CameraShaker.Instance.ShakeOnce(4f, 4f, 0.1f, 1f);
        Handheld.Vibrate();
        gameOver = true;
        if (ball.BallType == BallType.black) {
            int ballCount = Ball.SpawnedBalls.Count;
            for (int i = 0; i < Ball.SpawnedBalls.Count; i++) {
                Ball.SpawnedBalls[i].StopAllCoroutines();
                Ball.SpawnedBalls[i].enabled = false;
                if (ballCount > 1)
                    Ball.SpawnedBalls[i].StartCoroutine(Ball.SpawnedBalls[i].OnBlackHomeGameOver(ball));
            }
            StartCoroutine(PostBlackHoleGameOver());
            //Spawner.Instance.StartCoroutine(Spawner.Instance.Earthquake());
        } else {
            for (int i = 0; i < Ball.SpawnedBalls.Count; i++) {
                Ball.SpawnedBalls[i].StopAllCoroutines();
                Ball.SpawnedBalls[i].enabled = false;
                if (Ball.SpawnedBalls[i].BallType != BallType.black)
                    Ball.SpawnedBalls[i].StartCoroutine(Ball.SpawnedBalls[i].OnTimeoutGameOver(ball));
            }
            StartCoroutine(PostTimeoutGameOver());
        }
    }

    void ShowGameOverScreen() {
        Time.timeScale = 1;
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blur = 1;
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blurSpread = 1;

        Menu.Instance.gameOverScoreTxt.text = Score.ToString("00.00").Replace(Utils.NumberDecimalSeparator, ":");
        Menu.Instance.gameOverHighscoreTxt.text = Utils.GetHighscore().ToString("00.00").Replace(Utils.NumberDecimalSeparator, ":");

        Menu.Instance.newHighscoreTxt.gameObject.SetActive(Score > Utils.GetHighscore());
        Menu.Instance.gameTr.gameObject.SetActive(false);
        Menu.Instance.gameOverTr.transform.position = Menu.Instance.GameOverTrDefPos;
        if (Score > Utils.GetHighscore())
            Utils.SetHighscore(Score);
    }


    IEnumerator PostBlackHoleGameOver() {
        bool alreadyDestroyed = false;
        while (true) {
            if (Ball.SpawnedBalls.Count == 1 && alreadyDestroyed == false) {
                yield return new WaitForSeconds(0.5f);
                Ball.SpawnedBalls[0].StartCoroutine(Ball.SpawnedBalls[0].ScaleDown(true));
                alreadyDestroyed = true;
            }
            if (Ball.SpawnedBalls.Count == 0) {
                ShowGameOverScreen();
                break;
            }
            yield return null;
        }
    }

    IEnumerator PostTimeoutGameOver() {
        yield return new WaitForSeconds(1f);
        ShowGameOverScreen();
    }


    public void ClearLevel() {
        for (int i = 0; i < Ball.SpawnedBalls.Count; i++) {
            Ball.SpawnedBalls[i].DestroyBall();
        }
        Ball.SpawnedBalls.Clear();
    }
}
