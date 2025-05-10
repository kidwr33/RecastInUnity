using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreateTriangle : MonoBehaviour
{
    Voxel voxel = new Voxel();
    Bounds bounds;
    HeightField heightField;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            Vector3[] vertexs = new Vector3[3];
            vertexs[0] = new Vector3(0, 0, 0);
            vertexs[1] = new Vector3(2, 8, 4);
            vertexs[2] = new Vector3(3, 0, 0);

            // 生成一个meshrenderer画出这个三角形
            Mesh mesh = new Mesh();
            mesh.vertices = vertexs;
            int[] triangles = new int[3];
            triangles[0] = 0;
            triangles[1] = 1;
            triangles[2] = 2;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            GameObject go = new GameObject("Triangle");
            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Standard"));
            mf.mesh = mesh;

            bounds = new Bounds();
            bounds.center = Vector3.zero;
            bounds.extents = Vector3.one * 10;
            heightField = new HeightField(bounds, 0.5f, 0.5f);
            voxel.RasterizeTri(vertexs, 3, heightField);
            for(int i = 0; i < heightField.width; i++)
                for(int j = 0; j < heightField.height; j++)
                {
                    int index = j * heightField.width + i;
                    if (heightField.spans[index] != null) Debug.Log(heightField.spans[index].min + " " + heightField.spans[index].max);
                }
            
        }
    }

    Color color = Color.green;
    private void OnDrawGizmos()
    {
        //画出heifield的span
        Gizmos.color = color;
        if (bounds != null )
        {
            Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
            for (int i = 0; i < heightField.width; i++)
            {
                for (int j = 0; j < heightField.depth; j++)
                {
                    int index = j * heightField.width + i;
                    Span currentSpan = heightField.spans[index];
                    while (currentSpan != null)
                    {
                        Vector3 center = new Vector3(i + 0.5f, 0, j + 0.5f) * heightField.cellSize + new Vector3(0, 1, 0) * (currentSpan.min + (currentSpan.max - currentSpan.min) / 2) * heightField.cellHeight + heightField.bound.min;
                        Vector3 size = new Vector3(heightField.cellSize, (currentSpan.max - currentSpan.min) * heightField.cellHeight, heightField.cellSize);
                        Gizmos.DrawWireCube(center, size);
                        currentSpan = currentSpan.next;
                    }
                }
            }
            
        }
       
    }
}
