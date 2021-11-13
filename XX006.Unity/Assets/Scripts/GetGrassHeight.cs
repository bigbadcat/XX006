using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XX006;

public class GetGrassHeight : MonoBehaviour
{
    [Range(1, 100)]
    public int AreaX = 100;

    [Range(1, 100)]
    public int AreaY = 100;

    [Range(0.1f, 10)]
    public float GapX = 1;

    [Range(0.1f, 10)]
    public float GapZ = 1;

    [Range(1, 64)]
    public float WindGap = 32;

    [Range(1, 16)]
    public float WindSpeed = 8;

    public Vector3 WindDir = new Vector3(-2, -0.5f, 0);
    public Texture WindNoise;

    public Transform[] RoleTargets;

    public Terrain Ground;

    public Mesh GrassMesh;
    public Material GrassMat;
    public ComputeShader ViewFrustumCulling;
    public Material HizmipmapMat;

    private float m_Wind = 0;

    private GrassManager m_GrassManager = new GrassManager();


    void Start()
    {
        GrassChunk.ViewFrustumCulling = ViewFrustumCulling;
        GrassChunk.HizMipmapMat = HizmipmapMat;
        GrassChunk.WindNoise = WindNoise;
        GrassChunk.s_RoleTargetCount = RoleTargets == null ? 0 : Mathf.Min(RoleTargets.Length, GrassChunk.s_RoleTargets.Length);
        for (int i=0; i< GrassChunk.s_RoleTargetCount; ++i)
        {
            GrassChunk.s_RoleTargets[i] = RoleTargets[i];
        }
        BuildGrass();
    }

    public void BuildGrass()
    {
        if (Ground == null || GrassMesh == null || GrassMat == null)
        {
            return;
        }

        int col = (int)(AreaX / GapX) + 1;
        int row = (int)(AreaY / GapZ) + 1;

        TerrainData td = Ground.terrainData;
        for (int i=0; i< row; ++i)
        {
            for (int j=0; j< col; ++j)
            {
                float x = i * GapX + Random.Range(0, GapX / 2);
                float z = j * GapZ + Random.Range(0, GapZ / 2);
                float h = td.GetInterpolatedHeight(x / 100, z / 100);
                Vector3 pos = new Vector3(x, h - 0.002f, z);     //草要往下种一点点距离
                Quaternion q = Quaternion.Euler(0, Random.Range(-90, 90), 0);
                m_GrassManager.AddGrass(0, pos, Matrix4x4.TRS(pos, q, new Vector3(0.5f, Random.Range(0.5f, 1.2f), 1)));
            }
        }

        GrassMat.SetVector("_Move", new Vector4(WindDir.x, WindDir.y, WindDir.z, 0));
    }

    public void Update()
    {
        if (GrassMesh != null && GrassMat != null)
        {
            m_Wind += Time.deltaTime * WindSpeed;
            m_GrassManager.DrawGrass(m_Wind, WindGap, this.transform.forward, GrassMesh, 0, GrassMat);
        }
    }

    private void OnDestroy()
    {
        m_GrassManager.Release();
    }

    public void OnValidate()
    {
        if (GrassMat != null)
        {
            GrassMat.SetVector("_Move", new Vector4(WindDir.x, WindDir.y, WindDir.z, 0));
        }        
    }
}
