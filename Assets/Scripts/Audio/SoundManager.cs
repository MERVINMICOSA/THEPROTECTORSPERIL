using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;  // Reference to the volume slider
    [SerializeField] private Text muteStatusLabel; // Reference to the Text component showing the mute status
    [SerializeField] private float step = 0.01f; // Volume adjustment step
    private OptionController optionController;
    
    private bool isMuted = false; // Track mute state

    public Slider VolumeSlider => volumeSlider;

    private void Awake()
    {
        if (volumeSlider == null)
            Debug.LogWarning("VolumeSlider reference is missing.");

        // Initialize the OptionController
        optionController = FindObjectOfType<OptionController>();
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1); // Default value is 1
        }

        Load();
        UpdateMuteStatusLabel();
    }

    private void UpdateMuteStatusLabel()
    {
        if (muteStatusLabel != null)
        {
            muteStatusLabel.text = isMuted ? "Muted" : "Unmuted";
        }
    }

    // Set the volume value directly
    public void SetVolume(float volume)
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        AudioListener.volume = volume;
        isMuted = volume == 0; // If volume is 0, mark as muted
        UpdateMuteStatusLabel();
    }

    // Change the volume and save the setting
    public void ChangeVolume()
    {
        if (volumeSlider != null)
        {
            AudioListener.volume = volumeSlider.value;
            isMuted = volumeSlider.value == 0; // If volume is 0, mark as muted
            UpdateMuteStatusLabel();
            Save();
        }
    }

    // Mute the sound
    public void ToggleMute()
    {
        isMuted = !isMuted; // Toggle mute
        AudioListener.volume = isMuted ? 0 : volumeSlider.value; // Mute or unmute
        volumeSlider.value = isMuted ? 0 : volumeSlider.value; // Update slider to 0 or current volume
        UpdateMuteStatusLabel(); // Update the label
        Save(); // Save mute state
    }

    // Load the saved volume setting
    private void Load()
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
        }
        isMuted = volumeSlider.value == 0; // Check if volume is zero and set mute accordingly
        UpdateMuteStatusLabel();
    }

    // Save the current volume setting
    private void Save()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
        }
    }

    // Handle volume adjustment with the keyboard
    private void HandleKeyboardInput()
    {
        if (volumeSlider == null) return;

        if (Input.GetKey(KeyCode.RightArrow))
        {
            AdjustVolume(step); // Slower adjustment
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            AdjustVolume(-step); // Slower adjustment
        }
    }

    // Adjust the volume by a given delta (positive or negative)
    private void AdjustVolume(float delta)
    {
        if (volumeSlider != null)
        {
            volumeSlider.value = Mathf.Clamp(volumeSlider.value + delta, volumeSlider.minValue, volumeSlider.maxValue);
            ChangeVolume();
        }
    }

    // Reset volume to the default value (1)
    public void ResetVolumeToDefault()
    {
        SetVolume(1); // Set to default value
        Save();
    }
}
