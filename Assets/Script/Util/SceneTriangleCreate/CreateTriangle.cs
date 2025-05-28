
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Util
{
    public class CreateTriangle 
    {
        public GameObject SceneRoot;
        private void CreateSceneTriangle()
        {
            if (SceneRoot == null) return;
            int vertexOffset = 0;
            List<Vector3> vertexs = new List<Vector3>();
            List<int> triangle = new List<int>();
            for(int i = 0; i < SceneRoot.transform.childCount; i++)
            {
                Transform tf = SceneRoot.transform.GetChild(i);

                var mesh = SceneRoot.transform.GetChild(i).GetComponent<MeshFilter>()?.sharedMesh;
                if (mesh != null) CreateMeshTriangle(tf, mesh, ref vertexOffset, ref vertexs, ref triangle);
            }

            Terrain terrain = Object.FindObjectOfType<Terrain>();
            if (terrain != null) 
            {
                CreateTerrianTriangle(terrain, ref vertexOffset, ref vertexs, ref triangle);
            }

        }
        private void CreateMeshTriangle(Transform tf, Mesh mesh, ref int vertexOffset, 
                                        ref List<Vector3> vertexs, ref List<int> triangle)
        {
            foreach(Vector3 vertex in mesh.vertices) 
            {
                Vector3 v = tf.TransformPoint(vertex);
                vertexs.Add(v);
            }
            for(int i = 0; i < mesh.subMeshCount; i++)
            {
                int[] triangles = mesh.GetTriangles(i);
                for(int j = 0; j < triangles.Length; j += 3)
                {
                    triangle.Add(triangles[j] + vertexOffset);
                    triangle.Add(triangles[j + 1] + vertexOffset);
                    triangle.Add(triangles[j + 2] + vertexOffset);
                }
            }
            vertexOffset += mesh.vertices.Length;
        }

        private void CreateTerrianTriangle(Terrain terrain, ref int vertexOffset, ref List<Vector3> vertexs, ref List<int> triangle)
        {
            TerrainData data = terrain.terrainData;
            Vector3 terrianPos = terrain.GetPosition();
        }
        
    }
}

