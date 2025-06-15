using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [Header("musica")]
    public AudioClip menuMusic;
    public AudioClip levelMusic;

    private readonly int maxpool = 5;
    private AudioSource musicSource;
    private AudioSource[] sfxSource;
    private AudioSource loopingSource;
    private int index = 0;
    private AudioClip current;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

            sfxSource = new AudioSource[maxpool];
            for (int i = 0; i < maxpool; i++)
            {
                sfxSource[i] = gameObject.AddComponent<AudioSource>();
                sfxSource[i].playOnAwake = false;
                sfxSource[i].loop = false;
                sfxSource[i].volume = PlayerPrefs.GetFloat("SFXVolume", 0.25f);
            }

            loopingSource = gameObject.AddComponent<AudioSource>();
            loopingSource.playOnAwake = false;
            loopingSource.loop = true;
            loopingSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.25f);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(StartMusic(scene.name));
    }

    IEnumerator StartMusic(string sceneName)
    {
        yield return null;

        if (sceneName == "Menu")
        {
            PlayMusic(menuMusic);
        }
        else if (sceneName == "Final")
        {
            float startVolume = musicSource.volume;
            float duration = 2f;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
                yield return null;
            }

            musicSource.Stop();
            musicSource.volume = startVolume;
        }
        else
        {
            PlayMusic(levelMusic);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;

        sfxSource[index].PlayOneShot(clip);
        index = (index + 1) % maxpool;
    }

    public void PlayLooping(AudioClip clip)
    {
        if (clip == null) return;

        if (loopingSource.isPlaying && loopingSource.clip == clip)
            return;

        loopingSource.clip = clip;
        loopingSource.Play();
        current = clip;
    }

    public void StopLooping()
    {
        loopingSource.Stop();
        current = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        foreach (var src in sfxSource)
        {
            src.volume = volume;
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        loopingSource.volume = volume;
    }

    public void SetPausedState(bool isPaused)
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;

        if (isPaused)
        {
            musicSource.spatialBlend = 1;
        }
        else
        {
            musicSource.spatialBlend = 0;
        }
    }
}
