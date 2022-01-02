using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapBuilderController : MonoBehaviour
{

    public Terrain terrain;
    TerrainData tData;
    //Flags
    [Header("Flags")]
    public bool imagesReady = false;

    [Header("Object References")]
    public List<GameObject> spawnableObjects = new List<GameObject>();
    public List<Texture2D> objectIcons = new List<Texture2D>(), detailImages = new List<Texture2D>(); 
    public List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
    public Material waterMat1, waterMat2, waterMat3;

    public GameObject ToggleButtonPrefab, currentlyActiveOption;

    public GameObject ItemsGrid, PaintTerrainGrid, DetailsGrid;

    void Start()
    {
        tData = terrain.terrainData;
        // TODO: get scene information, build the game objects from the files and assign them to spawnable objects.
        // For now the usable objects are pre-asigned to the spawnableObjects List.
        StartCoroutine(AddObjectsToItemsMenu());
    }

    private void Update() {
        
    }

    IEnumerator AddObjectsToItemsMenu(){
        


        while(!imagesReady){
            yield return new WaitForSeconds(.25f);
        }

        Debug.Log("All Images Are Ready");


    }
}
