using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class TerrainGridManager : MonoBehaviour
{
    public Texture2D hMap;
    private void Start(){
        TerrainData tData = this.GetComponent<Terrain>().terrainData;
        float[,] heightMatrix = new float[hMap.width,hMap.height];
        Debug.Log(hMap.height.ToString() + " " + hMap.width.ToString());
        
        for (int y = 0; y < hMap.height ; y++){
            for (int x = 0; x < hMap.width; x++){
                heightMatrix[hMap.height - x -1,y] = hMap.GetPixel(x,y).grayscale;
            }
        }
        Debug.Log("setting map height");
        tData.SetHeights(0,0,heightMatrix);
    }
}
