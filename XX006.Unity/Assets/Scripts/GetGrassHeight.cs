using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;
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
        
    public float Distance = 10;

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
        Vector3 mypos = transform.position;
        int n = 0;
        float min_h = 0;
        float max_h = 0;
        for (int i=0; i< row; ++i)
        {
            for (int j=0; j< col; ++j)
            {
                float x = i * GapX + Random.Range(0, GapX / 2);
                float z = j * GapZ + Random.Range(0, GapZ / 2);
                float h = td.GetInterpolatedHeight(x / 100, z / 100);
                Vector3 pos = new Vector3(x, h - 0.002f, z);     //草要往下种一点点距离
                mypos.y = pos.y;
                if ((mypos-pos).sqrMagnitude < Distance * Distance)
                {
                    Quaternion q = Quaternion.Euler(0, Random.Range(-90, 90), 0);
                    //float rs = Random.Range(0.4f, 0.8f);
                    //float ry = rs * Random.Range(0.5f, 1.2f);
                    float rs = Random.Range(0.4f, 0.8f);
                    float ry = (float)System.Math.Sqrt((-2) * System.Math.Log(Random.Range(0, 1.0f), System.Math.E)) / 5;       //高度按标准正态分布随机
                    ry = 0.25f + ry * 0.75f;
                    ry = ry * 3.5f * rs;
                    m_GrassManager.AddGrass(0, pos, Matrix4x4.TRS(pos, q, new Vector3(rs, ry, 1)));
                    ++n;

                    if (n == 1)
                    {
                        min_h = max_h = ry;
                    }
                    else
                    {
                        min_h = Mathf.Min(min_h, ry);
                        max_h = Mathf.Max(max_h, ry);
                    }
                }                
            }
        }
        Log.Info("GrassCount:{0} minh:{1} maxh:{2}", n, min_h, max_h);
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
