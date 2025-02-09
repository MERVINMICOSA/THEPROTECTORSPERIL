using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] List<SceneDetails> connectedScenes;
    [SerializeField] AudioClip sceneMusic;
    public bool IsLoaded { get; private set; }

    List<SavableEntity> savableEntities;

    private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Player"))
    {
        Debug.Log($"Entered {sceneName}");

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError($"Scene name is not set for {gameObject.name}. Please assign a valid scene name.");
            return;
        }

        LoadScene();
        GameController.Instance.SetCurrentScene(this);

            if (sceneMusic != null)
                AudioManager.i.PlayMusic(sceneMusic, fade: true ) ;

        foreach (var scene in connectedScenes)
            scene.LoadScene();

        var previouslyLoadedScenes = GameController.Instance.PrevScene?.SelectMany(scene => scene.connectedScenes).ToList();
        if (previouslyLoadedScenes != null)
        {
            foreach (var scene in previouslyLoadedScenes)
            {
                if (!connectedScenes.Contains(scene) && scene != this)
                    scene.UnloadScene();
            }
        }

        var lastPrevScene = GameController.Instance.PrevScene.LastOrDefault();
        if (lastPrevScene != null && !connectedScenes.Contains(lastPrevScene))
        {
            lastPrevScene.UnloadScene();
        }
    }
}


    public void LoadScene()
    {
        if (!IsLoaded)
        {
            Debug.Log($"Loading scene: {gameObject.name}");
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);
            Debug.Log($"Unloading scene: {gameObject.name}");
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(sceneName);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();
        return savableEntities;
    }
}
