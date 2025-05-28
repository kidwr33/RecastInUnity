using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVertexOrder : MonoBehaviour
{
    Mesh mesh;
    // Start is called before the first frame update
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        foreach(Vector3 v in mesh.vertices)
        {
            Debug.Log(v);
        }
        for (int i = 0; i < mesh.subMeshCount; i++)
        {
            int[] triangles = mesh.GetTriangles(i);
            for (int j = 0; j < triangles.Length; j += 3)
            {
                Debug.Log(triangles[j] + " " + triangles[j + 1] + " " + triangles[j + 2]);
            }
        }
    }

    
}
