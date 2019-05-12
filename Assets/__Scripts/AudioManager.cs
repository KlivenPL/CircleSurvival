using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundEffect {
    spawn1,
    spawn2,
    spawn3,
    explosion1,
    explosion2,
    pop,
    blackHole,
    bassDrop
}

public class AudioManager : MonoBehaviour {
    public static AudioManager Instance { get; set; } = null;

    public AudioMixer audioMixer;

    public AudioClip[] music;
    public AudioClip[] soundEffects;

    public AudioSource musicAS;
    public AudioSource sfxAS;

    private void Awake() {
        Instance = this;
    }

    public void PlayMenuMusic() {
        PlayMusic(music[0]);
    }

    public void PlayGameMusic() {
        AudioClip tmpMusic;
        do
            tmpMusic = music[Random.Range(1, music.Length)];
        while (tmpMusic == musicAS.clip);

        PlayMusic(tmpMusic);
    }

    public void PlayMusic(AudioClip music) {
        StopAllCoroutines();
        StartCoroutine(PlayMusicIE(music));
    }

    public void PlaySfx(SoundEffect sfx, float vol) {
        sfxAS.PlayOneShot(soundEffects[(int)sfx], vol);
    }

    public void PlaySpawnSfx() {
        PlaySfx((SoundEffect)Random.Range((int)SoundEffect.spawn1, (int)SoundEffect.spawn3), 0.5f);
    }

    public IEnumerator PlayMusicIE(AudioClip music) {
        var vol = 1f;
        if (musicAS.isPlaying) {
            while (musicAS.volume > 0) {
                musicAS.volume -= Time.deltaTime / Time.timeScale * 3f;
                yield return null;
            }
        }
        musicAS.volume = 0;
        musicAS.Stop();
        if (music != null) {
            musicAS.clip = music;
            musicAS.Play();
            while (musicAS.volume <= vol) {
                musicAS.volume += Time.deltaTime / Time.timeScale * 3f;
                yield return null;
            }
        }
    }
}
