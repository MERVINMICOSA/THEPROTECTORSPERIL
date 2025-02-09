using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject essentialObjectPrefab;

    private void Awake()
    {
        // Check if there are already EssentialObjects in the scene
        var existingObjects = FindObjectsOfType<EssentialObjects>();
        if (existingObjects.Length > 0)
        {
            return; // Exit if essential objects already exist
        }

        // Determine the spawn position
        Vector3 spawnPos = Vector3.zero; // Default to origin

        var grid = FindObjectOfType<Grid>();
        if (grid != null)
        {
            spawnPos = grid.transform.position;
        }

        // Instantiate the essential object prefab
        Instantiate(essentialObjectPrefab, spawnPos, Quaternion.identity);
    }
}
