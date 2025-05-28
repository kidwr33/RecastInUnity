using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrianVertexGenerate : MonoBehaviour
{
    // Start is called before the first frame update
    public Terrain terrain;
    private TerrainData terrainData;
    void Start()
    {
        if (terrainData == null) terrainData = terrain.terrainData;
        GenerateTerrainVertex();
    }

    private Vector3[,] vertex;
    void GenerateTerrainVertex()
    {
        int w = terrainData.heightmapResolution, h = terrainData.heightmapResolution;
        float[,] heightsNormalize = terrainData.GetHeights(0, 0, w, h);
        vertex= new Vector3[w, h];
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                Vector3 pos = new Vector3(i, heightsNormalize[j,i], j);
                pos = Vector3.Scale(pos, terrainData.heightmapScale) + terrain.GetPosition();
                vertex[i,j] = (pos);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if(vertex == null) return;
        for (int i = 0;i < 10;i++)
            for(int j = 0;j < vertex.GetLength(1); j++)
            {
                Gizmos.DrawSphere(vertex[i,j], 0.2f );
            }
    }
}
