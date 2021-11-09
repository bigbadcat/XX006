using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetQuadMesh : MonoBehaviour
{
    public string SavePath = "Assets/ResourcesEx/Develop/QuadMesh";

    [Range(1, 10)]
    public int Split = 1;

    // Start is called before the first frame update
    void Start()
    {
        Mesh mesh = new Mesh();
        int vcount = 2 + Split * 2;
        Vector3[] vertices = new Vector3[vcount];
        Vector2[] uv = new Vector2[vcount];
        Vector3[] normals = new Vector3[vcount];
        Vector4[] tangents = new Vector4[vcount];
        int[] triangles = new int[Split * 2 * 3];
        for (int i=0; i<vcount; ++i)
        {
            normals[i] = new Vector3(0, 0, -1);
            tangents[i] = new Vector4(1, 0, 0, -1);
        }

        float sh = 1.0f / Split;
        for (int i=0; i<=Split; ++i)
        {
            float y = i * sh;
            Vector3 left = new Vector3(-0.5f, y, 0);
            Vector3 right = new Vector3(0.5f, y, 0);
            vertices[i * 2] = left;
            vertices[i * 2 + 1] = right;
            uv[i * 2] = new Vector2(0, y);
            uv[i * 2 + 1] = new Vector2(1, y);
        }
        for (int i = 0; i < Split; ++i)
        {
            int index = i * 2;
            int t_index = i * 6;
            triangles[t_index] = index + 0;
            triangles[t_index + 1] = index + 3;
            triangles[t_index + 2] = index + 1;
            triangles[t_index + 3] = index + 3;
            triangles[t_index + 4] = index + 0;
            triangles[t_index + 5] = index + 2;
        }

        Vector3[] r_vertices = new Vector3[vcount + 4];
        Vector2[] r_uv = new Vector2[vcount + 4];
        Vector3[] r_normals = new Vector3[vcount + 4];
        Vector4[] r_tangents = new Vector4[vcount + 4];
        int[] r_triangles = new int[triangles.Length + 12];

        for (int i=0; i<vcount; ++i)
        {
            r_vertices[i + 4] = vertices[i];
            r_uv[i + 4] = uv[i];
            r_normals[i + 4] = normals[i];
            r_tangents[i + 4] = tangents[i];
        }
        r_vertices[0] = vertices[0] + new Vector3(0, 0, 0.01f);
        r_vertices[1] = vertices[1] + new Vector3(0, 0, 0.01f);
        r_vertices[2] = vertices[0] + new Vector3(0, 0, -0.01f);
        r_vertices[3] = vertices[1] + new Vector3(0, 0, -0.01f);
        r_vertices[4] = vertices[0] + new Vector3(0, 0.02f, 0);
        r_vertices[5] = vertices[1] + new Vector3(0, 0.02f, 0);

        r_uv[0] = uv[0];
        r_uv[1] = uv[1];
        r_uv[2] = uv[0];
        r_uv[3] = uv[1];
        r_uv[4] = uv[0] + new Vector2(0, 0.01f);
        r_uv[5] = uv[1] + new Vector2(0, 0.01f);

        for (int i=0; i<4; ++i)
        {
            r_normals[i] = new Vector3(0, 0, -1);
            r_tangents[i] = new Vector4(1, 0, 0, -1);
        }

        for (int i=0; i< triangles.Length; ++i)
        {
            r_triangles[i + 12] = triangles[i] + 4;
        }

        r_triangles[0] = 0;
        r_triangles[1] = 5;
        r_triangles[2] = 1;
        r_triangles[3] = 5;
        r_triangles[4] = 0;
        r_triangles[5] = 4;
        r_triangles[6] = 2;
        r_triangles[7] = 5;
        r_triangles[8] = 3;
        r_triangles[9] = 5;
        r_triangles[10] = 2;
        r_triangles[11] = 4;

        mesh.vertices = r_vertices;
        mesh.uv = r_uv;
        mesh.normals = r_normals;
        mesh.tangents = r_tangents;
        mesh.triangles = r_triangles;

#if UNITY_EDITOR

        string path = "Assets/ResourcesEx/Develop/QuadMesh" + string.Format("_root_{0}.asset", Split);
        UnityEditor.AssetDatabase.CreateAsset(mesh, path);

#endif
    }
}
