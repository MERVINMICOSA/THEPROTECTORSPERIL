using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class MainMenuUI : MonoBehaviour
{
    public event Action<int> onMainMenuSelected;
    public event Action onBack;
    List<Text> mainMenuItems;
    [SerializeField] GameObject mainMenu;

    int selectedItem = 0;

    private void Awake()
    {
        mainMenuItems = mainMenu.GetComponentsInChildren<Text>().ToList();

    }
    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
    }


    public void HandleUpdate()
    {

        int preSelection = selectedItem;
        if (Input.GetKeyDown(KeyCode.DownArrow))
            ++selectedItem;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            --selectedItem;

        selectedItem = Mathf.Clamp(selectedItem, 0, mainMenuItems.Count - 1);

        if (preSelection != selectedItem)
            UpdateItemSelection();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onMainMenuSelected?.Invoke(selectedItem);
            CloseMainMenu();
            
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
            CloseMainMenu();
        }
    }

    void UpdateItemSelection()
    {
        for (int i = 0; i < mainMenuItems.Count; i++)
        {
            if (i == selectedItem)
                mainMenuItems[i].color = GlobalSettings.i.HighlightedColor;
            else
                mainMenuItems[i].color = Color.white;
        }
    }

   
    }
