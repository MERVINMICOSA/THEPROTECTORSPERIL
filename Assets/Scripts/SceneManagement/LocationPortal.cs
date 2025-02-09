using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
   
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;
    PlayerController player;

    public void OnPlayerTriggered(PlayerController player)
    {
        this.player = player;
        StartCoroutine(Teleport());
        Debug.Log("Player entered the portal");
    }
    Fader fader;
    private void Start()
    {
       fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {
        
        yield return fader.FadeIn(0.5f);
        GameController.Instance.PauseGame(true);

       var destPortal = FindObjectsOfType<LocationPortal>().First(x=> x != this && x.destinationPortal == this.destinationPortal);
       player.Character.SetPositionAndSnapToTile(destPortal.Spawnpoint.position);
         
        GameController.Instance.PauseGame(false);
       yield return fader.FadeOut(0.5f);
       
    }

    public Transform Spawnpoint =>spawnPoint;
}
