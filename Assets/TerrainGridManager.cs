using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

public class TerrainGridManager : MonoBehaviour
{
    public Texture2D hMap;
    public bool modifyTerrainOnLoad, paintTerrainOnLoad;
    public float baseHeightFactor = 0.05f, peaksPercentile = 0.75f;
    public float terrainObjectLength = 100, terrainObjectWidth = 100, terrainMaxHeight = 10;

    public GameObject supportingWallPrefab, cameraLimitSphere, grid;

    private void Start(){
        Terrain terrain = this.GetComponent<Terrain>();
        TerrainData tData = terrain.terrainData;

        tData.size = new Vector3(terrainObjectWidth,terrainMaxHeight,terrainObjectLength);
        
        Debug.Log(" map --> source:" + hMap.width.ToString() + "x" + hMap.height.ToString());

        Camera.allCameras[0].transform.position = new Vector3(terrainObjectWidth/2, 60, terrainObjectLength/3);

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

        var path = Application.dataPath + "/GameData/terrain_details.txt";

        //SaveDetailsIntoFile(tData, path);
        LoadDetailsFromFile(tData, path);
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

        // Camera limiter
        cameraLimitSphere.transform.position = new Vector3(width/2,50,length/2-15);
        cameraLimitSphere.transform.localScale = new Vector3(width,100,length);

    }

    private void DrawGrid(TerrainData tData){
        var width = tData.size.x;
        var length = tData.size.z;
        var maxHeight = tData.size.y;

        // var gridMesh = grid.GetComponent<MeshCollider>();
        // var mesh = grid.GetComponent<MeshFilter>().mesh;

        float[,] heights = CalculateHeightMatrix((int)width,(int)length, maxHeight);

        Material whiteDiffuseMat = new Material(Shader.Find("Unlit/Texture"));
        
        for(int x = 0; x < width; x++){
            GameObject o = new GameObject();
            o.transform.parent = grid.transform;
            var lineInstance = o.AddComponent<LineRenderer>();
            lineInstance.positionCount = (int)length; 
            lineInstance.startWidth = 0.025f;
            lineInstance.endWidth = 0.025f;
            lineInstance.material = whiteDiffuseMat;
            Vector3[] positions = new Vector3[(int)length];

            for(int y = 0; y < length; y ++){
                var p = new Vector3(x + 0.5f,heights[x,y] +0.5f, y + 0.5f);
                positions[y] = p;
            }
            lineInstance.SetPositions(positions);
        }

        for(int y = 0; y < length; y++){
            GameObject o = new GameObject();
            o.transform.parent = grid.transform;
            var lineInstance = o.AddComponent<LineRenderer>();
            lineInstance.positionCount = (int)width; 
            lineInstance.startWidth = 0.025f;
            lineInstance.endWidth = 0.025f;
            lineInstance.material = whiteDiffuseMat;
            Vector3[] positions = new Vector3[(int)width];

            for(int x = 0; x < width; x ++){
                var p = new Vector3(x + 0.5f,heights[x,y] +0.5f ,y + 0.5f);
                positions[x] = p;
            }

            lineInstance.SetPositions(positions);
        }

    }

    // we do a series of raycasts and use the results to draw the grid.
    private float[,] CalculateHeightMatrix(int width, int length, float maxHeight){
        float[,] res = new float[width, length];

        for (int i = 0; i < width; i++){
            for (int j = 0; j < length; j++){
                // RaycastHit[] hits = new RaycastHit[5];
                RaycastHit hit = new RaycastHit();
                // Physics.Raycast(new Vector3(i,maxHeight,j), Vector3.down, out hits[0], maxHeight);
                // Physics.Raycast(new Vector3(i+1,maxHeight,j), Vector3.down, out hits[1], maxHeight);
                // Physics.Raycast(new Vector3(i,maxHeight,j+1), Vector3.down, out hits[2], maxHeight);
                // Physics.Raycast(new Vector3(i+1,maxHeight,j+1), Vector3.down, out hits[3], maxHeight);
                Physics.Raycast(new Vector3(i+0.5f,maxHeight,j+0.5f), Vector3.down, out hit, maxHeight);

                // Debug.DrawRay(new Vector3(i+0.5f,maxHeight,j+0.5f), Vector3.down *10, Color.white, 10f, false);

                // var heightAverage = 0f;
                // foreach (var h in hits)
                // {
                //     heightAverage += h.point.y;
                // }
                // heightAverage /= 5;

                res[i,j] = hit.point.y;
            }
        }
        return res;
    }

    private void SaveDetailsIntoFile(TerrainData tData, string path){
        
        string s = "";
        for (int i = 0; i < tData.detailPrototypes.Length; i++)
        {
            int[,] bushLayer =  tData.GetDetailLayer(0,0,tData.detailWidth,tData.detailHeight,i);
            var width = bushLayer.GetLength(0);
            var length = bushLayer.GetLength(1);
            s += generateAStringFromAnArray(bushLayer);
            if(i < tData.detailPrototypes.Length - 2){
                s+=";";
            }
        }
        
        // tData.terrainLayers[0].diffuseTexture;
        // tData.detailPrototypes[0];

        StreamWriter sw = new StreamWriter(path);
        sw.WriteLine(s);
        sw.Close();
    }

    private void LoadDetailsFromFile(TerrainData tData, string path){
        StreamReader sr = new StreamReader(path);
        var s = sr.ReadToEnd();
        var detailLayers = s.Split(';');
        
        // we reset the detail layers to be empty this way it clears the earlier details data.
        for (int i = 0; i < tData.detailPrototypes.Length; i++)
        {
            tData.SetDetailLayer(0,0,i,new int[tData.detailWidth,tData.detailHeight]);
        } 

        for (int i = 0; i < detailLayers.Length; i++)
        {
            // TODO: get detailPrototype information from textures and apply them, currently has them ingrained into terrain.
            // tData.detailPrototypes[i]; 
            tData.SetDetailLayer(0,0,i,loadAnArrayFromString(detailLayers[i]));
        }
    }


    // YOINKED CODE FROM HERE: 
    public static void CopyStream(Stream input, Stream output)
    // Helper funtion to copy from one stream to another
    {
        // Magic number is 2^16
        byte[] buffer = new byte[32768];
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }

     public static string Compress(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream())
        {
            using (var gs = new GZipStream(mso, CompressionMode.Compress))
            {
                CopyStream(msi, gs);
            }
            return Convert.ToBase64String(mso.ToArray());
        }
    }

    public static string Decompress(string s)
    {
        var bytes = Convert.FromBase64String(s);
        using (var msi = new MemoryStream(bytes))
        using (var mso = new MemoryStream())
        {
            using (var gs = new GZipStream(msi, CompressionMode.Decompress))
            {
                CopyStream(gs, mso);
            }
            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }

    public string generateAStringFromAnArray(int[,] arrayWeWantToSave){
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, arrayWeWantToSave);
        
        var s = Convert.ToBase64String(ms.ToArray());
        s = Compress(s);
        return s;
    }

    public int[,] loadAnArrayFromString(string s)
    {
        s = Decompress(s);

        BinaryFormatter bf = new BinaryFormatter();
        Byte[] by = Convert.FromBase64String(s);
        MemoryStream sr = new MemoryStream(by);

        return (int[,])bf.Deserialize(sr);
    }
}