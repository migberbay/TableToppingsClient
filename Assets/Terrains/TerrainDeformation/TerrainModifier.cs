using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.TerrainAPI;

[System.Serializable]
public class TerrainModifier
{
    [SerializeField] ComputeShader terrainPaintShader;
    //public Dictionary_Int_Texture2D brushes = new Dictionary_Int_Texture2D();
    public bool toCenter;
    private Vector2Int brushSize;
    private float heightOffset = .5f;
    private Texture2D brushTexture;
    private float height;
    public Terrain terrain;
    private RenderTexture prevRenderTexture;
    private Vector2 terrainPoint = Vector2.one;
    private Vector2 previousTerrainPoint;
    private Vector2 hitPoint;
    private List<TreeInstance> trees = new List<TreeInstance>();
    private bool baked;
	private uint x, y, z;
    private RaycastHit hit;
    private Matrix4x4 rotationMatrix;
    private float angle;
    private float prevAngle;
    public TerrainData terrainDataBU;
    public Vector2 VegetationArea { private get; set; }
    #region Terrain Modification

    //public TerrainModifier(ComputeShader computeShader)
    //{
    //    terrainPaintShader = computeShader;
    //}

    public void SetBrushData(Texture2D brush, float height, Vector2Int size)
    {
        brushTexture = brush;
        heightOffset = height;
        brushSize = size;
    }

    public void Execute(bool isInBounds, float angle)
    {
        if(!isInBounds)return;
        GetAreaToModify();
        this.angle = angle;
    }

    void GetAreaToModify()
    {
        // hit = toCenter ? CameraBase.Ray(false, true, "Terrain") : CameraBase.Ray(false, false, "Terrain");
        if(hit.point == Vector3.zero) return;

        terrain = hit.transform.GetComponent<Terrain>();
        terrainPoint = new Vector2((int)((hit.point.x - (brushSize.x * .5f)) - terrain.transform.position.x), (int)((hit.point.z - (brushSize.y * .5f)) - terrain.transform.position.z));
        hitPoint = new Vector2(hit.point.x, hit.point.z);
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

    void ModifyTerrain(Rect selection)
    {
        Vector3 c1 = new Vector3(selection.position.x + terrain.transform.position.x, 0, selection.position.y + terrain.transform.position.z);
        Vector3 c2 = new Vector3(selection.position.x + terrain.transform.position.x, 0, selection.position.y + terrain.transform.position.z + selection.size.y);
        Vector3 c3 = new Vector3(selection.position.x + terrain.transform.position.x + selection.size.x, 0, selection.position.y + terrain.transform.position.z + selection.size.y);
        Vector3 c4 = new Vector3(selection.position.x + terrain.transform.position.x + selection.size.x, 0, selection.position.y + terrain.transform.position.z);
        
        //if(selection.size.x == 32)
        //{
        //    Debugger.Log("c1: "+c1);
        //    Debug.DrawRay(c1, Vector3.up * 100, Color.red, Mathf.Infinity);
        //    Debugger.Log("c2: "+c2);
        //    Debug.DrawRay(c2, Vector3.up * 100, Color.blue, Mathf.Infinity);
        //    Debugger.Log("c3: "+c3);
        //    Debug.DrawRay(c3, Vector3.up * 100, Color.green, Mathf.Infinity);
        //    Debugger.Log("c4: "+c4);
        //    Debug.DrawRay(c4, Vector3.up * 100, Color.yellow, Mathf.Infinity);
        //}

        //HandleTerrainTrees(hitPoint, VegetationArea, angle);
        
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
        terrainPaintShader.SetVector("rotOffset", new Vector2(brushSize.x/2, brushSize.y/2));
        terrainPaintShader.SetTexture(terrainPaintShader.FindKernel("CSMain"), "heightmap", terrainRenderTexture);
        terrainPaintShader.SetTexture(terrainPaintShader.FindKernel("CSMain"), "brush", brushTexture);
        
        terrainPaintShader.Dispatch(terrainPaintShader.FindKernel("CSMain"), (int)(selection.size.x/x), (int)(selection.size.y/y), (int)z);

        Graphics.CopyTexture(terrainRenderTexture, paintContext.destinationRenderTexture);

        TerrainPaintUtility.EndPaintHeightmap(paintContext, "Terrain");

        terrain.terrainData.SyncHeightmap();
    }

    public void HandleTerrainTrees(Vector2 position, Vector2 size, float angle, bool isMultipule = false)
    {
        if (trees == null)trees = new List<TreeInstance>();
        if (trees.Count > 0 && !isMultipule)
        {
            trees.AddRange(terrain.terrainData.treeInstances);
            terrain.terrainData.treeInstances = trees.ToArray();
            trees.Clear();
        }
        ////Trees inside circle
        //foreach (var tree in terrain.terrainData.treeInstances)
        //{
        //    Vector2 p = new Vector2((tree.position.x * terrain.terrainData.size.x) + terrain.transform.position.x, (tree.position.z * terrain.terrainData.size.z) + terrain.transform.position.z);
        //    if(MathHelper.PointInCircle(p, position, size.x))
        //        trees.Add(tree);
        //}
        //Trees inside rectangle
        Vector2 A = Vector2.zero;
        Vector2 B = Vector2.zero;
        Vector2 C = Vector2.zero;
        foreach (var tree in terrain.terrainData.treeInstances)
        {
            Vector2 p = new Vector2((tree.position.x * terrain.terrainData.size.x) + terrain.transform.position.x, (tree.position.z * terrain.terrainData.size.z) + terrain.transform.position.z);
            angle = isMultipule ? 360 - angle : angle;
            // A = MathHelper.GetRotatedCornerPoint(position, new Vector2((position.x + size.x / 2), (position.y + size.y / 2)), angle);
            // B = MathHelper.GetRotatedCornerPoint(position, new Vector2((position.x + size.x / 2), (position.y - size.y / 2)), angle);
            // C = MathHelper.GetRotatedCornerPoint(position, new Vector2((position.x - size.x / 2), (position.y - size.y / 2)), angle);
            
            // if (MathHelper.PointInRectangle(p, A, B, C))
            // {
            //     trees.Add(tree);
                
            //     //Debug.DrawRay(new Vector3(p.x, 30, p.y), Vector3.up * 100, Color.yellow, Mathf.Infinity);
            // }
        }
        //Debug.DrawRay(new Vector3(A.x, 30, A.y), Vector3.up * 100, Color.red, Mathf.Infinity);
        //Debug.DrawRay(new Vector3(B.x, 30, B.y), Vector3.up * 100, Color.blue, Mathf.Infinity);
        //Debug.DrawRay(new Vector3(C.x, 30, C.y), Vector3.up * 100, Color.green, Mathf.Infinity);

        //Debug 1 set of removed trees
        //if (trees.Count > 0)
        //{
        //    terrain.terrainData.treeInstances = trees.ToArray();
        //}

        if (trees.Count > 0)
        {
            TreeInstance[] newTrees = terrain.terrainData.treeInstances.Except(trees).ToArray();
            terrain.terrainData.treeInstances = newTrees;
            if (isMultipule)
                trees.Clear();
            terrain.Flush();
        }
    }
    
    // public void BakeAndRefresh(Building building)
    // {
    //     HandleTerrainTrees(hitPoint, VegetationArea, angle);
    //     baked = true;
    // }

    private void RestoreTerrain(Rect selection)
    {
        if (baked)
        {
            baked = false;
            return;
        }
        PaintContext prevPaintContext = TerrainPaintUtility.BeginPaintHeightmap(terrain, selection);

        Graphics.CopyTexture(prevRenderTexture, prevPaintContext.destinationRenderTexture);

        TerrainPaintUtility.EndPaintHeightmap(prevPaintContext, "Terrain");
        
        terrain.terrainData.SyncHeightmap();
    }

    public void ApplyTerrainData(TerrainData terrainData)
    {
        if(!terrain)
            terrain = GameObject.FindGameObjectWithTag("ResortTerrain").GetComponent<Terrain>();
        // Terrain collider
        terrain.terrainData.SetHeights(0, 0, terrainData.GetHeights(0, 0, terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution));
        // Textures
        terrain.terrainData.SetAlphamaps(0, 0, terrainData.GetAlphamaps(0, 0, terrain.terrainData.alphamapWidth, terrain.terrainData.alphamapHeight));
        // Trees
        terrain.terrainData.treeInstances = terrainData.treeInstances;
        // Grasses
        //terrain.terrainData.SetDetailLayer(0, 0, 0, terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, 0));
        //terrain.terrainData = terrainData;
        terrain.Flush();
    }
    //Vector3 RotateShape(Vector3 pos, Vector2 center, float angle)
    //{
    //    pos.x -= center.x;
    //    pos.z -= center.y;

    //    float s, c;
    //    s = Mathf.Sin((angle) * Mathf.Deg2Rad);
    //    c = Mathf.Cos((angle) * Mathf.Deg2Rad);
    //    //sincos(radians(angle), s, c);
    //    //float2x2 rotationMatrix = new float2x2(c, -s, s, c);

    //   // Vector4 a = Vector4.zero;
    //    //Matrix4x4 b = Matrix4x4.identity;
    //    //Vector4 d = b * a;
    //    Vector3 @new = Vector3.zero;
    //    @new.x = pos.x * c - pos.z * s;
    //    @new.z = pos.z * c + pos.x * s;

    //    @new.x += center.x;
    //    @new.z += center.y;

    //    //rotationMatrix.SetTRS;
    //    return @new;
    //}
    #endregion
}
