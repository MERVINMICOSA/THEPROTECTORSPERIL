using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    public event Action<Collider2D> OnEnterTrainersView;
    private Vector2 input;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //remove diagonal movement
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if (Input.GetKeyDown(KeyCode.Z))
            StartCoroutine(Interact());
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        // Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);
        if (collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.offsetY), 0.2f, GameLayers.i.TriggerableLayers);
        CheckIfInTrainersView();
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null)
            {

                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            Debug.Log($"Entered trainers view");
            OnEnterTrainersView?.Invoke(collider);
        }
    }

    public object CaptureState()
    {
        var soundSwitch = FindObjectOfType<SoundSwitch>(); // Reference to SoundSwitch
        return new PlayerSaveData
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList(),
            musicVolume = AudioListener.volume,
            muted = soundSwitch != null && soundSwitch.IsMuted() // Save muted state
        };
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        // Restore position
        transform.position = new Vector3(saveData.position[0], saveData.position[1]);

        // Restore Pokémon party
        var pokemonParty = GetComponent<PokemonParty>();
        pokemonParty.Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();

        // Restore volume
        AudioListener.volume = saveData.musicVolume;
        var soundManager = FindObjectOfType<SoundManager>();
        if (soundManager != null)
        {
            soundManager.SetVolume(saveData.musicVolume);
        }

        // Restore muted state
        var soundSwitch = FindObjectOfType<SoundSwitch>();
        if (soundSwitch != null)
        {
            soundSwitch.SetMuted(saveData.muted);
        }
    }


    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}
[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
    public float musicVolume; // Already added
    public bool muted;        // New field for sound state
}
