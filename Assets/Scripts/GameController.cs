using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Option, Paused, Bag, Menu, MainMenu, PartyScreen, VolumeControl }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] SoundManager soundManager; // Reference to SoundManager

    private GameState state;
    private GameState stateBeforePause;

    public SceneDetails CurrentScene { get; private set; }
    public List<SceneDetails> PrevScene { get; private set; } = new List<SceneDetails>();

    public static GameController Instance { get; private set; }

    MenuController menuController;
    MainMenuUI mainMenuUI;
    OptionController optionController;

    // New variable to track mute state
    private bool isMuted = false;

    private void Awake()
    {
        Instance = this;
        PokemonDB.Init();
        ConditionsDB.Init();
        MoveDB.Init();
        menuController = GetComponent<MenuController>();
        mainMenuUI = GetComponentInChildren<MainMenuUI>();
        optionController = GetComponent<OptionController>();
    }

    private void Start()
    {
        state = GameState.MainMenu;

        battleSystem.OnBattleOver += EndBattle;
        partyScreen.Init();

        playerController.OnEnterTrainersView += (Collider2D trainerCollider) =>
        {
            var trainer = trainerCollider.GetComponentInParent<TrainerControler>();
            if (trainer != null)
            {
                StartCoroutine(trainer.TriggerTrainerBattle(playerController));
            }
        };

        DialogManager.Instance.OnShowDialog += () => state = GameState.Dialog;
        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (state == GameState.Dialog)
                state = GameState.FreeRoam;
        };

        menuController.onBack += () => { state = GameState.FreeRoam; };

        menuController.onMenuSelected += OnMenuSelected;
        mainMenuUI.onMainMenuSelected += OnMainMenuSelected;
        optionController.onOptionSelected += OnOptionSelected;

        // Initialize mute state from PlayerPrefs
        isMuted = PlayerPrefs.GetInt("isMuted", 0) == 1;
        ApplyMuteState();
    }

    public void PauseGame(bool pause)
    {
        if (pause)
        {
            stateBeforePause = state;
            state = GameState.Paused;
        }
        else
        {
            state = stateBeforePause;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        mainMenuUI.HandleUpdate();
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<PokemonParty>();
        var wildPokemon = CurrentScene.GetComponent<MapArea>().GetRandomWildPokemon();
        var wildPokemonCopy = new Pokemon(wildPokemon.Base, wildPokemon.Level);

        battleSystem.StartBattle(playerParty, wildPokemonCopy);
    }

    private TrainerControler trainer;
    public void StartTrainerBattle(TrainerControler trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<PokemonParty>();
        var trainerParty = trainer.GetComponent<PokemonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void OnEnterTrainersView(TrainerControler trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    private void EndBattle(bool won)
    {
        if (trainer != null && won)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    private void Update()
    {
        switch (state)
        {
            case GameState.MainMenu:
                mainMenuUI.OpenMainMenu();
                mainMenuUI.HandleUpdate();
                break;
            case GameState.Option:
                optionController.HandleUpdate();
                break;
            case GameState.FreeRoam:
                playerController.HandleUpdate();
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    menuController.OpenMenu();
                    state = GameState.Menu;
                }
                break;
            case GameState.Battle:
                battleSystem.HandleUpdate();
                break;
            case GameState.Dialog:
                DialogManager.Instance.HandleUpdate();
                break;
            case GameState.Menu:
                menuController.HandleUpdate();
                break;
            case GameState.PartyScreen:
                partyScreen.HandleUpdate(
                    onSelected: () => { },
                    onBack: () =>
                    {
                        partyScreen.gameObject.SetActive(false);
                        state = GameState.FreeRoam;
                    }
                );
                break;
            case GameState.Bag:
                inventoryUI.HandleUpdate(
                    onBack: () =>
                    {
                        inventoryUI.gameObject.SetActive(false);
                        state = GameState.FreeRoam;
                    }
                );
                break;
            case GameState.VolumeControl:
                HandleVolumeControl(); // Now only handled if the user selects volume
                break;
        }

     
    }


    public void SetCurrentScene(SceneDetails currScene)
    {
        if (CurrentScene != null && !PrevScene.Contains(CurrentScene))
        {
            PrevScene.Add(CurrentScene);
        }

        CurrentScene = currScene;
    }

    void OnMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if (selectedItem == 1)
        {
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if (selectedItem == 2)
        {
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 3)
        {
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }
    }

    void OnMainMenuSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // New game
            mainMenuUI.CloseMainMenu();
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 1)
        {
            // Load game
            SavingSystem.i.Load("saveSlot1");
            mainMenuUI.CloseMainMenu();
            optionController.CloseOption();
            state = GameState.FreeRoam;
        }
        else if (selectedItem == 2)
        {
            // Options
            state = GameState.Option;
            mainMenuUI.CloseMainMenu();
           
            optionController.OpenOption();
        }
        else if (selectedItem == 3)
        {
            // Quit
        }
    }

    void OnOptionSelected(int selectedItem)
    {
        if (selectedItem == 0)
        {
            // Volume
            state = GameState.VolumeControl; // Open the volume control screen
        }
        else if (selectedItem == 1)
        {
            // Sound (mute/unmute toggle)
            soundManager.ToggleMute();  // Call the ToggleMute function
        }
        else if (selectedItem == 2)
        {
            // Developers
        }
        else if (selectedItem == 3)
        {
            // Back
            optionController.CloseOption();
            state = GameState.MainMenu;  // Going back to Main Menu
        }
    }





    // New method to toggle mute/unmute
    private void ToggleMute()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt("isMuted", isMuted ? 1 : 0); // Save mute state
        ApplyMuteState(); // Apply the mute state
    }

    // Apply the mute state
    private void ApplyMuteState()
    {
        AudioListener.pause = isMuted; // Mute or unmute the game
        // Optionally update UI elements (e.g., sound icon) if needed
        if (isMuted)
        {
            Debug.Log("Game Muted");
            // Update sound icon or any UI indication for mute
        }
        else
        {
            Debug.Log("Game Unmuted");
            // Update sound icon or any UI indication for unmute
        }
    }

    private void HandleVolumeControl()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Increase volume by 0.1 increment
            if (soundManager != null)
            {
                soundManager.VolumeSlider.value = Mathf.Clamp(
                    soundManager.VolumeSlider.value + 0.1f,
                    soundManager.VolumeSlider.minValue,
                    soundManager.VolumeSlider.maxValue
                );
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Decrease volume by 0.1 increment
            if (soundManager != null)
            {
                soundManager.VolumeSlider.value = Mathf.Clamp(
                    soundManager.VolumeSlider.value - 0.1f,
                    soundManager.VolumeSlider.minValue,
                    soundManager.VolumeSlider.maxValue
                );
            }
        }

        // Press X to return to the Option menu
        if (Input.GetKeyDown(KeyCode.X))
        {
            state = GameState.Option;
        }
    }
}
