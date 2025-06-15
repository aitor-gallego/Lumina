using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    public GameObject pausemenu;
    public CanvasGroup settings;
    public Slider musicSlider;
    public Slider sfxSlider;
    public InputActionReference input;
    public InputActionReference closeinput;
    public InputActionReference backAction;
    public GameObject player;
    public EventSystem eventSystem;
    public AudioClip sfxSound;
    public GameObject first;
    private GameObject last;
    private bool paused = false;

    private void Awake()
    {
        pausemenu.SetActive(false);
        settings.gameObject.SetActive(false);

        input.action.performed += OnOpenPressed;
        closeinput.action.performed += OnClosePressed;
        backAction.action.performed += OnBackPressed;

        closeinput.action.Disable();
    }

    private void OnDestroy()
    {
        input.action.performed -= OnOpenPressed;
        closeinput.action.performed -= OnClosePressed;
        backAction.action.performed -= OnBackPressed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.25f);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        input.action.Enable();
        backAction.action.Enable();
    }

    private void OnDisable()
    {
        input.action.Disable();
        closeinput.action.Disable();
        backAction.action.Disable();
    }

    private void OnOpenPressed(InputAction.CallbackContext context)
    {
        if (!paused)
        {
            Pause();
            input.action.Disable();
            closeinput.action.Enable();
        }
    }

    private void OnClosePressed(InputAction.CallbackContext context)
    {
        if (paused)
        {
            Resume();
            input.action.Enable();
            closeinput.action.Disable();
        }
    }

    public void Pause()
    {
        pausemenu.SetActive(true);
        Time.timeScale = 0f;
        paused = true;
        PlayerController.inputBlocked = true;

        if (AudioController.Instance != null)
            AudioController.Instance.SetPausedState(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(first);
    }

    public void Resume()
    {
        pausemenu.SetActive(false);
        settings.gameObject.SetActive(false);

        Time.timeScale = 1f;
        paused = false;
        PlayerController.inputBlocked = false;

        if (AudioController.Instance != null)
            AudioController.Instance.SetPausedState(false);
    }

    public void Settings()
    {
        last = EventSystem.current.currentSelectedGameObject;

        pausemenu.SetActive(false);
        settings.gameObject.SetActive(true);
        settings.alpha = 1f;
        settings.blocksRaycasts = true;
        settings.interactable = true;

        backAction.action.Enable();

        StartCoroutine(SetFirstSelectedDelayed(musicSlider.gameObject));
    }

    private IEnumerator SetFirstSelectedDelayed(GameObject go)
    {
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(go);
    }

    public void OnBackPressed(InputAction.CallbackContext context)
    {
        if (settings.gameObject.activeSelf)
        {
            settings.gameObject.SetActive(false);

            pausemenu.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(last != null ? last : first);
        }
    }

    public void ShowMenu()
    {
        settings.gameObject.SetActive(false);
        pausemenu.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(last);
    }

    public void Reset()
    {
        player.GetComponent<PlayerController>().Reset();
        Resume();
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void SetMusicVolume(float volume)
    {
        AudioController.Instance.SetMusicVolume(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    private void SetSFXVolume(float volume)
    {
        AudioController.Instance.SetSFXVolume(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        AudioController.Instance.PlaySFX(sfxSound);
    }
}
