using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoundSwitch : MonoBehaviour
{
    [SerializeField] Image soundOnIcon;
    [SerializeField] Image soundoffIcon;
    private bool muted = false;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("muted"))
        {
            PlayerPrefs.SetInt("muted", 0);
            Load();
        }
        else
        {
            Load();
        }
        UpdateButtonIcon();
        AudioListener.pause = muted;
    }


    public bool IsMuted()
    {
        return muted; // Return the current mute state
    }

    public void SetMuted(bool isMuted)
    {
        muted = isMuted;
        AudioListener.pause = muted; // Apply the muted state
        UpdateButtonIcon();          // Update the UI
        Save();                      // Save the state for consistency
    }

    private void UpdateButtonIcon()
    {
        if (muted == false)
        {
            soundOnIcon.enabled = true;
            soundoffIcon.enabled = false;
        }
        else
        {
            soundOnIcon.enabled = false;
            soundoffIcon.enabled = true;
        }
    }
    public void OnButtonPress()
    {
        if(muted == false)
        {
            muted = true;
            AudioListener.pause = true;
        }
        else
        {
            muted = false;
            AudioListener.pause = false;
        }
        Save();
        UpdateButtonIcon();
        
    }

    private void Load()
    {
        muted = PlayerPrefs.GetInt("muted") == 1;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
    }
}
