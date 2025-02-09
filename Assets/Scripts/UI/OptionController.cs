using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class OptionController : MonoBehaviour
{
    public event Action<int> onOptionSelected;
    public event Action onBack;
    List<Text> optionItems;
    [SerializeField] GameObject option;
    [SerializeField] GameObject volumeControlPanel;  // Reference to the volume control panel (UI)

    int selectedItem = 0;

    private void Awake()
    {
        optionItems = option.GetComponentsInChildren<Text>().ToList();
    }

    public void OpenOption()
    {
        option.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseOption()
    {
        option.SetActive(false);
    }

    public void HandleUpdate()
    {
        int preSelection = selectedItem;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, optionItems.Count - 1);

        if (preSelection != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onOptionSelected?.Invoke(selectedItem);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < optionItems.Count; i++)
        {
            if (i == selectedItem)
                optionItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                optionItems[i].color = Color.white;
        }
    }

    // Method to show the volume control panel (called when the "Volume" option is selected)
    public void OpenVolumeControl()
    {
        if (volumeControlPanel != null)
        {
            volumeControlPanel.SetActive(true);  // Show the volume control UI
        }
    }

    // Method to hide the volume control panel (called when leaving the "Volume" option)
    public void CloseVolumeControl()
    {
        if (volumeControlPanel != null)
        {
            volumeControlPanel.SetActive(false);  // Hide the volume control UI
        }
    }
}
