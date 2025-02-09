using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PokemonParty : MonoBehaviour
{
    [SerializeField] List<Pokemon> pokemons;

    public event Action OnUpdated;

    public List<Pokemon> Pokemons
    {
        get
        {

            return pokemons;
        }
        set
        {
            pokemons = value;
            OnUpdated?.Invoke();
        }
    }

    private void Awake()
    {
        foreach (var pokemon in pokemons)
        {
            pokemon.Init();
        }
    }

    private void Start()
    {
        
    }

    public Pokemon GetHealthyPokemon()
    {
       return pokemons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddPokemon(Pokemon newpokemon)
    {
        if (pokemons.Count < 6)
        {
            pokemons.Add(newpokemon);
            OnUpdated?.Invoke();
        }
        else
        {

        }
    }

    public static PokemonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<PokemonParty>();
    }
}
