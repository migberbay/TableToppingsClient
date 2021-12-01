using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class TerrainGridManager : MonoBehaviour
{
    public Texture2D hMap;
    public bool modifyTerrainOnLoad, paintTerrainOnLoad;
    public float baseHeightFactor = 0.05f, peaksPercentile = 0.75f;
    public float terrainObjectLength = 100, terrainObjectWidth = 100, terrainMaxHeight = 10;

    public GameObject supportingWallPrefab, cameraLimitSphere, grid;

    private void Start(){

        // int numprints = 50;
        Terrain terrain = this.GetComponent<Terrain>();
        TerrainData tData = terrain.terrainData;

        tData.size = new Vector3(terrainObjectWidth,terrainMaxHeight,terrainObjectLength);
        
        Debug.Log(" map --> source:" + hMap.width.ToString() + "x" + hMap.height.ToString());

        if(modifyTerrainOnLoad){
            float[,] heightMatrix = new float[hMap.width,hMap.height];

            for (int y = 0; y < hMap.height ; y++){
                for (int x = 0; x < hMap.width; x++){
                    heightMatrix[y, x] = hMap.GetPixel(x,y).grayscale + baseHeightFactor;
                    // if(numprints >= 0 && heightMatrix[y, x] > 0){
                    //     Debug.Log(heightMatrix[y, x]);
                    //     numprints--;
                    // }
                }
            }
            
            for (int y = 0; y < hMap.height; y++){
                heightMatrix[y,0] = 0f;
                heightMatrix[y,hMap.width-1] = 0f;
            }

            for (int x = 0; x < hMap.width; x++){
                heightMatrix[0,x] = 0f;
                heightMatrix[hMap.height-1,x] = 0f;
            }

            tData.SetHeights(0,0,heightMatrix);
        }

        if(paintTerrainOnLoad){
            Debug.Log(" map --> splatMaps:" + tData.alphamapWidth.ToString() + "x" + tData.alphamapHeight.ToString());

            // splatmaps is a blending technique where with a 3d array u give each texture 

            float[, ,]splatMapData = new float[tData.alphamapWidth, tData.alphamapHeight, tData.alphamapLayers];
            for (int y = 0; y < tData.alphamapHeight; y++)
            {
                for (int x = 0; x < tData.alphamapWidth; x++)
                {
                    float normalizedHeight = tData.GetHeight(x,y)/terrainMaxHeight;

                    Vector3 splat = new Vector3(1,0,0); //base is the first in the hierarchy
                    
                    if(normalizedHeight > peaksPercentile){
                        splat = Vector3.Lerp(splat, new Vector3(0,0,1), 1); // peaks texture is the third
                    }

                    if(normalizedHeight >= baseHeightFactor-0.05 && normalizedHeight < baseHeightFactor + 0.05){
                        splat = Vector3.Lerp(splat, new Vector3(0,1,0), 1); // low texture is the second
                    }

                    splat.Normalize();
                    splatMapData[y,x,0] = splat.x;
                    splatMapData[y,x,1] = splat.y;
                    splatMapData[y,x,2] = splat.z;
                }
            }
            // and finally assign the new splatmap to the terrainData:
            tData.SetAlphamaps(0, 0, splatMapData);
        }
   
        AddSupportingWallsAndSetCameraLimiter(tData);
        DrawGrid(tData);
    }

    private void AddSupportingWallsAndSetCameraLimiter(TerrainData tData){
        var width = tData.size.x;
        var length = tData.size.z;

        cameraLimitSphere.transform.position.Set(width/2,1,length/2);
        cameraLimitSphere.transform.localScale.Set(width+25,1,length);

        Vector3 scale1 = new Vector3(length/10, 1 , 10);
        Vector3 scale2 = new Vector3(width/10, 1 , 10);

        Vector3 pos1 = new Vector3(0, -50, length/2);
        Vector3 rot1 = new Vector3(90,0,90);

        Vector3 pos2 = new Vector3(width/2, -50, length);
        Vector3 rot2 = new Vector3(90,0,0);

        Vector3 pos3 = new Vector3(width, -50, length/2);
        Vector3 rot3 = new Vector3(90,0,-90);

        Vector3 pos4 = new Vector3(width/2, -50, 0);
        Vector3 rot4 = new Vector3(90,0,180);

        List<Vector3> positions = new List<Vector3>{pos1,pos2,pos3,pos4};
        List<Vector3> rotations = new List<Vector3>{rot1,rot2,rot3,rot4};

        for (int i = 0; i < 4; i++)
        {
            var wallinstance = Instantiate(supportingWallPrefab);
            wallinstance.transform.position = positions[i];
            wallinstance.transform.eulerAngles = rotations[i];

            if(i%2 == 0){
                wallinstance.transform.localScale = scale1;
            }else{
                wallinstance.transform.localScale = scale2;
            }
        }
    }

    // we do a series of raycasts and use the results to draw the grid.
    private void DrawGrid(TerrainData tData){
        var width = tData.size.x;
        var length = tData.size.z;
        var maxHeight = tData.size.y;

        // var gridMesh = grid.GetComponent<MeshCollider>();
        // var mesh = grid.GetComponent<MeshFilter>().mesh;


        for(int x = 0; x < width; x++){
            for(int y = 0; y < width; y++){
                var lineInstance = grid.AddComponent<LineRenderer>();
            }
        }

    }

    private int[,] CalculateHeightMatrix(int width, int length, float maxHeight){
        int[,] res = new int[width, length];

        for (int i = 0; i < width; i++){
            for (int j = 0; j < length; j++){
                RaycastHit[] hits = new RaycastHit[5];
                Physics.Raycast(new Vector3(i,0,j), Vector3.up, out hits[0], maxHeight);
                Physics.Raycast(new Vector3(i+1,0,j), Vector3.up, out hits[1], maxHeight);
                Physics.Raycast(new Vector3(i,0,j+1), Vector3.up, out hits[2], maxHeight);
                Physics.Raycast(new Vector3(i+1,0,j+1), Vector3.up, out hits[3], maxHeight);
                Physics.Raycast(new Vector3(i+0.5f,0,j+0.5f), Vector3.up, out hits[4], maxHeight);
                var heightAverage = 0f;
                foreach (var h in hits)
                {
                    heightAverage += h.point.y;
                }
                heightAverage /= 5;

                res[i,j] = (int)heightAverage + 1;
            }
        }
        return res;
    }

    private void AddNewGridSquare(int x , int y){

    }

    
}