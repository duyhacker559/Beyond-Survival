using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsController : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;
    public AudioMixer VFXaudioMixer;
    public Slider volumeSlider;
    public Slider VFXvolumeSlider;
    public Toggle muteToggle;

    [Header("Graphics")]
    public Toggle fullscreenToggle;

    [Header("UI")]
    public GameObject optionsPanel;
    public Button backButton;

    void Start()
    {
        // Setup mặc định
        volumeSlider.value = 0.8f;
        VFXvolumeSlider.value = 0.8f;
        muteToggle.isOn = false;
        fullscreenToggle.isOn = Screen.fullScreen;

        // Gắn sự kiện
        volumeSlider.onValueChanged.AddListener(SetVolume);
        VFXvolumeSlider.onValueChanged.AddListener(VFXSetVolume);
        muteToggle.onValueChanged.AddListener(SetMute);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        backButton.onClick.AddListener(CloseOptions);
    }

    // AUDIO
    public void SetVolume(float volume)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }

    public void VFXSetVolume(float volume)
    {
        VFXaudioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
    }

    public void SetMute(bool mute)
    {
        AudioListener.volume = mute ? 0f : 1f;
    }

    // GRAPHICS
    public void SetFullscreen(bool isFullscreen)
    {
        if (isFullscreen)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.SetResolution(1280, 720, false); 
        }
    }

    // UI
    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }
}
