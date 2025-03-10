﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] int sceneToLoad = -1;
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(SwitchScene());
        Debug.Log("Player entered the portal");
    }
    Fader fader;
    private void Start()
    {
       fader = FindObjectOfType<Fader>();
    }

    IEnumerator SwitchScene()
    {
        DontDestroyOnLoad(gameObject);
yield return fader.FadeIn(0.5f);
        GameController.Instance.PauseGame(true);
        
       yield return SceneManager.LoadSceneAsync(sceneToLoad);

       var destPortal = FindObjectsOfType<Portal>().First(x=> x != this && x.destinationPortal == this.destinationPortal);
       player.Character.SetPositionAndSnapToTile(destPortal.Spawnpoint.position);
         
        GameController.Instance.PauseGame(false);
       yield return fader.FadeOut(0.5f);
       Destroy(gameObject);
    }

    public Transform Spawnpoint =>spawnPoint;
}

public enum DestinationIdentifier {A,B,C,D,E}