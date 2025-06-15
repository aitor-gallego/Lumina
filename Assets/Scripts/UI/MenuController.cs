using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public CanvasGroup menu;
    public CanvasGroup settings;
    public CanvasGroup credits;
    public GameObject last;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Button creditsBack;
    public InputActionReference backAction;
    public EventSystem eventSystem;
    public AudioClip sfxSound;

    private const int FirstLevelIndex = 1;
    private const int LastLevelIndex = 7;

    private void Awake()
    {
        backAction.action.performed += OnBackPressed;
    }

    private void OnDestroy()
    {
        backAction.action.performed -= OnBackPressed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowMenu();

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
        backAction.action.Enable();
    }

    private void OnDisable()
    {
        backAction.action.Disable();
    }


    public void Play()
    {
        var controller = DataController.Instance;
        var data = controller.CurrentData;
        int last = data.lastCompletedLevel;
        int nextScene;

        if (last < FirstLevelIndex)
        {
            // Nunca ha empezado → nivel 1
            nextScene = FirstLevelIndex;
        }
        else if (last < LastLevelIndex)
        {
            // En medio de la campaña → siguiente nivel
            nextScene = last + 1;
        }
        else
        {
            // Ya completó TODOS los niveles → reiniciamos sólo el progreso
            controller.Reset();
            nextScene = FirstLevelIndex;
        }

        SceneTransitionController.Instance
            .LoadScene(nextScene, SceneTransitionController.duration);
    }

    public void Settings()
    {
        last = EventSystem.current.currentSelectedGameObject;
        menu.gameObject.SetActive(false);
        credits.gameObject.SetActive(false);
        settings.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(musicSlider.gameObject);
    }

    public void Credits()
    {
        last = EventSystem.current.currentSelectedGameObject;
        menu.gameObject.SetActive(false);
        settings.gameObject.SetActive(false);
        credits.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(creditsBack.gameObject);
    }

    public void ShowMenu()
    {
        settings.gameObject.SetActive(false);
        credits.gameObject.SetActive(false);
        menu.gameObject.SetActive(true);

        EventSystem.current.SetSelectedGameObject(last);
    }

    private void OnBackPressed(InputAction.CallbackContext context)
    {
        if (settings.gameObject.activeSelf || credits.gameObject.activeSelf)
        {
            ShowMenu();
        }
    }

    private void SetMusicVolume(float volume)
    {
        AudioController.Instance.SetMusicVolume(volume);
    }

    private void SetSFXVolume(float volume)
    {
        AudioController.Instance.SetSFXVolume(volume);
        AudioController.Instance.PlaySFX(sfxSound);
    }
}
