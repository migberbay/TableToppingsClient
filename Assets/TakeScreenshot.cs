using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TakeScreenshot : MonoBehaviour
{
    public int height = 128, width = 128;
    public RenderTexture ren;
    public string spritename = "";
    public MapBuilderController mbCntrler;
    public Camera screenShotCamera;
    Vector3 originalObjectPosition;

    void Start(){
        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Space)){
        //     CreateIcon("");
        // }
    }

    public void CreateIcon(string filepath){
        if(string.IsNullOrEmpty(spritename)){
            spritename = "defaultIconName";
        }

        screenShotCamera = this.GetComponent<Camera>();

        string path = SetSaveLocation(filepath);
        path += spritename;
        screenShotCamera.targetTexture = ren;

        RenderTexture currentRT = RenderTexture.active;
        screenShotCamera.targetTexture.Release();
        RenderTexture.active = screenShotCamera.targetTexture;
        screenShotCamera.Render();

        Texture2D imgPng = new Texture2D(height, width, TextureFormat.ARGB32, false);

        imgPng.ReadPixels(new Rect(0,0,width,height),0,0);
        imgPng.Apply();
        RenderTexture.active = currentRT;
        byte[] bytesPng = imgPng.EncodeToPNG();
        Debug.Log("creating file: " + path+".png");
        File.WriteAllBytes(path+".png", bytesPng);

        mbCntrler.objectIcons.Add(imgPng);
    }

    string SetSaveLocation(string saveLocation){
        if(!Directory.Exists(saveLocation)){
            saveLocation = Application.streamingAssetsPath+"/Icons/";
            Directory.CreateDirectory(saveLocation);
        }
        return saveLocation;
    }
}
