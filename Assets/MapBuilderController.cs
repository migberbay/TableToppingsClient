using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public GameObject ToggleButtonPrefab, currentlyActiveOption;
    public GameObject ItemsGrid, PaintTerrainGrid, DetailsGrid;
    

    public TMP_InputField minheight, maxheight, width, length;

    [Header("Water Layer")]
    public GameObject WaterPlane;
    Material watermat;
    public Slider liquidLayerHeightSlider, depthCutoutSlider, depthStrengthSlider, reflectivenessSlider, waveStrengthSlider, normalStrSlider;

    public FlexibleColorPicker shallowPicker, deepPicker;
    string piDeep, piShallow, piDepthCutout, piDepthStrength, piReflectiveness, piWaveLevel, piNormalStrength;


    void Init(){
        //TODO: Read terrain information from scene file and load them into here.
        tData = terrain.terrainData;
        width.text = tData.size.x.ToString();
        length.text = tData.size.z.ToString();
        maxheight.text = tData.size.y.ToString();
        minheight.text = "0";

        //WATER
        var waterMesh = WaterPlane.GetComponent<MeshRenderer>();
        watermat = waterMesh.sharedMaterial;

        piDeep = "Color_36218622185947c6a5ae36366d8e21d8";
        piShallow = "Color_93e06cd551a5449091bcde90b46765a0";
        piDepthCutout = "Vector1_6f56a0970372485390c6587863c2374e";
        piDepthStrength = "Vector1_6c82dffdd68049bcb019d3a9c64c92a0";
        piReflectiveness = "Vector1_6269b1025b26473ca8bc61634f34b537";
        piWaveLevel = "Vector1_7273530c27a34c9f8ee5723b84f96baa";
        piNormalStrength = "Vector1_687f54e8c371429f86b9eaab0e7dfe3e";

        var deepColor = watermat.shader.GetPropertyDefaultVectorValue(watermat.shader.FindPropertyIndex(piDeep));
        var shallowColor = watermat.shader.GetPropertyDefaultVectorValue(watermat.shader.FindPropertyIndex(piShallow));
        var depthCut = watermat.shader.GetPropertyDefaultFloatValue(watermat.shader.FindPropertyIndex(piDepthCutout));
        var depthStr = watermat.shader.GetPropertyDefaultFloatValue(watermat.shader.FindPropertyIndex(piDepthStrength));
        var reflectiveness = watermat.shader.GetPropertyDefaultFloatValue(watermat.shader.FindPropertyIndex(piReflectiveness));
        var waveLevel = watermat.shader.GetPropertyDefaultFloatValue(watermat.shader.FindPropertyIndex(piWaveLevel));
        var normalStrength = watermat.shader.GetPropertyDefaultFloatValue(watermat.shader.FindPropertyIndex(piNormalStrength));
        
        // initializing GUI
        shallowPicker.color = deepColor;
        deepPicker.color = shallowColor;

        liquidLayerHeightSlider.minValue = 0f;
        liquidLayerHeightSlider.maxValue = tData.size.y;
        liquidLayerHeightSlider.value = WaterPlane.transform.position.y;

        depthStrengthSlider.minValue = 0f;
        depthStrengthSlider.maxValue = 2f;
        depthStrengthSlider.value = depthStr;

        reflectivenessSlider.minValue = 0;
        reflectivenessSlider.maxValue = 1;
        reflectivenessSlider.value = reflectiveness;

        waveStrengthSlider.minValue = 0;
        waveStrengthSlider.maxValue = 3;
        waveStrengthSlider.value = waveLevel;

        normalStrSlider.minValue = 0;
        normalStrSlider.maxValue = 1;
        normalStrSlider.value = normalStrength;

        depthCutoutSlider.minValue = 0;
        depthCutoutSlider.maxValue = 5;
        depthCutoutSlider.value = depthCut;
    }

    void Start()
    {
        Init();
        // TODO: get scene information, build the game objects from the files and assign them to spawnable objects.
        // For now the usable objects are pre-asigned to the spawnableObjects List.
        StartCoroutine(AddObjectsToItemsMenu());
    }

    private void Update() {
        
    }

    IEnumerator AddObjectsToItemsMenu(){       
        foreach (var item in terrainLayers){
            var t = item.diffuseTexture;
            var buttonInstance = GameObject.Instantiate(ToggleButtonPrefab);
            buttonInstance.transform.SetParent(PaintTerrainGrid.transform);
            var img = buttonInstance.GetComponent<Image>();
            img.sprite = Sprite.Create(t, new Rect(0,0,128,128), new Vector2(.5f,.5f));
        }

        foreach (var item in detailImages){
            var t = item;
            var buttonInstance = GameObject.Instantiate(ToggleButtonPrefab);
            buttonInstance.transform.SetParent(DetailsGrid.transform);
            var img = buttonInstance.GetComponent<Image>();
            img.sprite = Sprite.Create(t, new Rect(0,0,item.width, item.height), new Vector2(.5f,.5f));
        }


        while(!imagesReady){
            yield return new WaitForSeconds(.25f);
        }

        
        foreach (var item in objectIcons)
        {
            var t = item;
            var buttonInstance = GameObject.Instantiate(ToggleButtonPrefab);
            buttonInstance.transform.SetParent(ItemsGrid.transform);
            var img = buttonInstance.GetComponent<Image>();
            img.sprite = Sprite.Create(t, new Rect(0,0,128,128), new Vector2(.5f,.5f));
        }
    }

    public void ApplyTerrainSettings(){
        float w = int.Parse(width.text), l = int.Parse(length.text),
        maxh = int.Parse(maxheight.text), baseh = int.Parse(minheight.text);
        tData.size = new Vector3(w, maxh, l);

        var normalizedBaseh = baseh/maxh;
        var resol = tData.heightmapResolution;

        Debug.Log("heightmap resolution = " + resol.ToString());

        float[,] hMap = tData.GetHeights(0, 0, resol, resol);

        for (int y = 0; y < resol; y++){
            for (int x = 0; x < resol; x++){
                if(normalizedBaseh > hMap[y,x]){
                    hMap[y,x] = normalizedBaseh;
                }
            }
        }

        // tData.SetHeights(0, 0, hMap);
        tData.SetHeightsDelayLOD(0,0,hMap);
        tData.SyncHeightmap();
    }

    #region WaterOps
    public void ChangeWaterElevation(float value){
        var curpos = WaterPlane.transform.position;
        curpos.y = value;
        WaterPlane.transform.position = curpos;
    }
    public void ChangeShallowColor(Color c){
        Debug.Log("running shallow");
        try{
            watermat.SetVector(piShallow, c);
        }catch{}
    }
    public void ChangeDeepColor(Color c){
        try{
            watermat.SetVector(piDeep, c);
        }catch{}
    }
    public void ChangeWaveStrength(float f){
        try{
            watermat.SetFloat(piWaveLevel, f);
        }catch{}
    }
    public void ChangeDepthStrength(float f){
        try{
            watermat.SetFloat(piDepthStrength, f);
        }catch{}
    }
    public void ChangeReflectiveness(float f){
        try{
            watermat.SetFloat(piReflectiveness, f);
        }catch{}
    }
    public void ChangeNormalStrength(float f){
        try{
            watermat.SetFloat(piNormalStrength, f);
        }catch{}
    }
    public void ChangeDepthCutout(float f){
        try{
            watermat.SetFloat(piDepthCutout, f);
        }catch{}
    }
    // int piDeep, piShallow, piDepthCutout, piDepthStrength, piReflectiveness, piWaveLevel, piNormalStrength;
    #endregion
}
