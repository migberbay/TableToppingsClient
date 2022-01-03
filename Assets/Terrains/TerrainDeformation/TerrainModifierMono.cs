using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

public class TerrainModifierMono : MonoBehaviour
{
    [SerializeField] ComputeShader terrainPaintShader;
    public float heightOffset = .5f;
    public float angle;
    public Vector2Int brushSize;
    public Texture2D brushTexture;
    private Terrain terrain;
    private RenderTexture prevRenderTexture;
    private float height;
    private Vector2 terrainPoint = Vector2.one;
    private Vector2 previousTerrainPoint;
    private Vector2 hitPoint;
    private float prevAngle;
    private uint x, y, z;
    private RaycastHit hit;
    // private DrawShapes drawShapes;

    // private void Start()
    // {
    //     drawShapes = GetComponent<DrawShapes>();
    // }

    public void Update()
    {
        GetAreaToModify();
    }
    void GetAreaToModify()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction*100, Color.red);
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Terrain")))
        {
            terrain = hit.transform.GetComponent<Terrain>();
            terrainPoint = new Vector2((int)((hit.point.x - (brushSize.x * .5f)) - terrain.transform.position.x), (int)((hit.point.z - (brushSize.y * .5f)) - terrain.transform.position.z));
            hitPoint = new Vector2(hit.point.x, hit.point.z);
            // if(drawShapes)
            //     drawShapes.SetWorldPoint(hit.point);
            if (terrainPoint != previousTerrainPoint || prevAngle != angle)
            {
                Rect prevRect = new Rect(previousTerrainPoint, brushSize);
                if (prevRect.height != 0 && prevRect.width != 0 && prevRenderTexture)
                    RestoreTerrain(prevRect);
                Rect rect = new Rect(terrainPoint, brushSize);
                ModifyTerrain(rect);
                previousTerrainPoint = terrainPoint;
                prevAngle = angle;
            }
        }
    }

    void ModifyTerrain(Rect selection)
    {
        Vector3 c1 = new Vector3(selection.position.x + terrain.transform.position.x, 0, selection.position.y + terrain.transform.position.z);
        Vector3 c2 = new Vector3(selection.position.x + terrain.transform.position.x, 0, selection.position.y + terrain.transform.position.z + selection.size.y);
        Vector3 c3 = new Vector3(selection.position.x + terrain.transform.position.x + selection.size.x, 0, selection.position.y + terrain.transform.position.z + selection.size.y);
        Vector3 c4 = new Vector3(selection.position.x + terrain.transform.position.x + selection.size.x, 0, selection.position.y + terrain.transform.position.z);

        PaintContext paintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, selection);

        RenderTexture terrainRenderTexture = new RenderTexture(paintContext.sourceRenderTexture.width, paintContext.sourceRenderTexture.height, 0, RenderTextureFormat.R16);
        terrainRenderTexture.enableRandomWrite = true;
        Graphics.CopyTexture(paintContext.sourceRenderTexture, terrainRenderTexture);

        prevRenderTexture = new RenderTexture(paintContext.sourceRenderTexture.width, paintContext.sourceRenderTexture.height, 0, RenderTextureFormat.R16);
        Graphics.CopyTexture(paintContext.sourceRenderTexture, prevRenderTexture);

        float h0 = terrain.SampleHeight(c1);
        float h1 = terrain.SampleHeight(c2);
        float h2 = terrain.SampleHeight(c3);
        float h3 = terrain.SampleHeight(c4);

        height = ((h0 + h1 + h2 + h3) / 4f) / terrain.terrainData.size.y;

        terrainPaintShader.GetKernelThreadGroupSizes(terrainPaintShader.FindKernel("CSMain"), out x, out y, out z);

        terrainPaintShader.SetFloat("height", height * heightOffset);
        terrainPaintShader.SetFloat("angle", angle);
        //Debug.Log(x+"/"+y+"/"+z);
        terrainPaintShader.SetVector("rotOffset", new Vector2(brushSize.x / 2, brushSize.y / 2));
        terrainPaintShader.SetTexture(terrainPaintShader.FindKernel("CSMain"), "heightmap", terrainRenderTexture);
        terrainPaintShader.SetTexture(terrainPaintShader.FindKernel("CSMain"), "brush", brushTexture);

        terrainPaintShader.Dispatch(terrainPaintShader.FindKernel("CSMain"), (int)(selection.size.x / x), (int)(selection.size.y / y), (int)z);

        Graphics.CopyTexture(terrainRenderTexture, paintContext.destinationRenderTexture);

        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain");

        terrain.terrainData.SyncHeightmap();
    }

    private void RestoreTerrain(Rect selection)
    {
        PaintContext prevPaintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, selection);
        Graphics.CopyTexture(prevRenderTexture, prevPaintContext.destinationRenderTexture);
        TerrainPaintUtility.EndPaintHeightmap(prevPaintContext, "Terrain");
        terrain.terrainData.SyncHeightmap();
    }
}
