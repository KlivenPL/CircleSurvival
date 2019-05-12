using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;

public class Menu : MonoBehaviour {
    public static Menu Instance;

    public static bool IsLoaded { get; set; } = false;
    public Button playBtn;
    public RectTransform mainMenuTr;
    public RectTransform gameTr;
    public RectTransform gameOverTr;

    public Text gameScoreTxt;
    public Text gameOverScoreTxt;
    public Text gameOverHighscoreTxt;
    public Text menuHighscoreTxt;
    public Text newHighscoreTxt;

    public Toggle themedCirclesChk;
    public Toggle musicChk;
    public Toggle soundsChk;

    public Background background;

    private bool _replay = false;

    public Vector3 GameOverTrDefPos { get; set; }

    void Start() {
        GameOverTrDefPos = gameOverTr.transform.position;
        gameOverTr.transform.position = new Vector3(0, -10000, 0);
        gameOverTr.gameObject.SetActive(true);
        Instance = this;
        Utils.UpdateAccelometerDefPos();
        menuHighscoreTxt.text = Utils.GetHighscore().ToString("00.00").Replace(Utils.NumberDecimalSeparator, ":");
        themedCirclesChk.isOn = Utils.GetThemedCircles() == 1 ? true : false;
        background.Init();
        StartCoroutine(Animate());
        IsLoaded = true;

        SceneManager.sceneUnloaded += OnSceneUnloaded;

        AudioManager.Instance.PlayMenuMusic();
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blur = 1f;
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blurSpread = 1f;
    }


    public void PlayBtn() {
        StopAllCoroutines();
        mainMenuTr.gameObject.SetActive(false);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        gameTr.gameObject.SetActive(true);
        Utils.UpdateAccelometerDefPos();
    }

    public void ReplayBtn() {
        GameManager.Instance.ClearLevel();
        _replay = true;
        SceneManager.UnloadSceneAsync(1);
    }

    public void MenuBtn() {
        GameManager.Instance.ClearLevel();
        _replay = false;
        SceneManager.UnloadSceneAsync(1);
    }

    public void OnMusicChkChange() {
        AudioManager.Instance.audioMixer.SetFloat("musicVolume", musicChk.isOn ? -6f : -80f);
    }

    public void OnSoundChkChange() {
        AudioManager.Instance.audioMixer.SetFloat("sfxVolume", soundsChk.isOn ? 0f : -80f);
    }

    void OnSceneUnloaded(Scene scene) {
        menuHighscoreTxt.text = Utils.GetHighscore().ToString("00.00").Replace(Utils.NumberDecimalSeparator, ":");
        gameOverTr.transform.position = new Vector3(0, -10000, 0);
        GameManager.Score = 0;
        if (_replay) {
            _replay = false;
            PlayBtn();
            return;
        }
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blur = 1;
        Camera.main.GetComponent<VignetteAndChromaticAberration>().blurSpread = 1;
        AudioManager.Instance.PlayMenuMusic();
        StartCoroutine(Animate());
        Menu.Instance.mainMenuTr.gameObject.SetActive(true);
    }


    IEnumerator Animate() {
        while (true) {
            playBtn.transform.localScale = (1.2f + Mathf.Sin(Time.time * Time.timeScale) * .1f) * Vector3.one;
            playBtn.image.color = Background.Instance.CurrentColor * 1.25f;
            if (themedCirclesChk.isOn)
                themedCirclesChk.graphic.color = Background.Instance.CurrentColor * 1.25f;
            yield return new WaitForFixedUpdate();
        }
    }

    public void OnThemedCirclesChkChange() {
        Utils.SetThemedCircles(themedCirclesChk.isOn);
    }
}
