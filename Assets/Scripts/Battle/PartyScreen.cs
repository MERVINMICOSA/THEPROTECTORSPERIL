﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] Text messageText;

    PartyMemberUI[] memberSlots;
    List<Pokemon> pokemons;
    PokemonParty party;

    public Pokemon SelectedMember => pokemons[selection];

    
    
    int selection = 0;
    public BattleState? CalledFrom {get; set;}

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        party = PokemonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        pokemons = party.Pokemons;
        for (int i = 0; i < memberSlots.Length; i++) 
        {
            if (i < pokemons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(pokemons[i]);
            }
                
            else
                memberSlots[i].gameObject.SetActive(false);
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a Monster";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < pokemons.Count; i++)
        {
            if (i == selectedMember)
                memberSlots[i].SetSelected(true);
            else
                memberSlots[i].SetSelected(false);
        }
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {

        var prevSelection = selection;
        if (Input.GetKeyDown(KeyCode.RightArrow))
            ++selection;
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            --selection;
        else if (Input.GetKeyDown(KeyCode.DownArrow))
            selection += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            selection -= 2;

        selection = Mathf.Clamp(selection, 0, pokemons.Count - 1);

        if(selection != prevSelection)
        UpdateMemberSelection(selection);

        if (Input.GetKeyDown(KeyCode.Z))
        {
            onSelected?.Invoke();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            onBack?.Invoke();
        }
    }
    public void SetMessageText (string message)
    {
        messageText.text = message;
    }

  
}
