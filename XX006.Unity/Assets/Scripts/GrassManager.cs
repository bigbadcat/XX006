using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    public class GrassManager
    {
        public GrassManager()
        {
            GeometryUtil.GetBoundPoints(m_BoundCenter, m_BoundSize, s_BoundPoints);
        }

        public GrassChunk GetOrCreateChunk(int id)
        {
            GrassChunk ck;
            if (!m_GrassChunks.TryGetValue(id, out ck))
            {
                ck = new GrassChunk();
                m_GrassChunks.Add(id, ck);
            }
            return ck;
        }

        /// <summary>
        /// 移除草。
        /// </summary>
        /// <param name="map">地图编号。</param>
        public void RemoveGrass(int map)
        {
            m_GrassChunks.Remove(map);
        }

        Vector4[] points = new Vector4[8];


        public void DrawGrass(float wind, float gap, Vector3 dir, Mesh mesh, int submeshIndex, Camera camera)
        {
            Matrix4x4 vp = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix;      //VP矩阵
            GrassChunk.ViewFrustumCulling.SetMatrix(GrassChunk.s_PID_CameraVPMatrix, vp);
            GrassChunk.ViewFrustumCulling.SetVectorArray(GrassChunk.s_PID_BoundPoints, s_BoundPoints);

            //设置风的参数
            float fm = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
            Vector4 wdir = new Vector4(dir.x, dir.y, dir.z, fm);
            GrassChunk.ViewFrustumCulling.SetFloat(GrassChunk.s_PID_Wind, wind);
            GrassChunk.ViewFrustumCulling.SetFloat(GrassChunk.s_PID_WindGap, gap);
            GrassChunk.ViewFrustumCulling.SetVector(GrassChunk.s_PID_WindDir, wdir);
            GrassChunk.ViewFrustumCulling.SetTexture(GrassChunk.s_Kernel, GrassChunk.s_PID_WindNoise, GrassChunk.s_WindNoise);
            var bend_areas = BendArea.CurAreas;
            for (int i = 0; i < bend_areas.Count; ++i)
            {
                GrassChunk.s_RolePositions[i] = bend_areas[i].Info;
            }
            GrassChunk.ViewFrustumCulling.SetInt("_RoleCount", bend_areas.Count);
            GrassChunk.ViewFrustumCulling.SetVectorArray("_RoleInfos", GrassChunk.s_RolePositions);

            GeometryUtil.GetFrustumPlane(camera, s_CachePlanes);
            foreach (var kvp in m_GrassChunks)
            {
                bool show = true;
                var bound_points = kvp.Value.BoundPoints;
                for (int i = 0; i < s_CachePlanes.Length && show; ++i)
                {
                    int out_count = 0;
                    for (int j = 0; j < bound_points.Length; ++j)
                    {
                        if (GeometryUtil.IsOutsideThePlane(s_CachePlanes[i], bound_points[j]))
                        {
                            ++out_count;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (out_count == bound_points.Length)
                    {
                        show = false;
                    }
                }

                if (show)
                {
                    kvp.Value.DrawGrass(wind, gap, dir, mesh, submeshIndex, camera);
                }                
            }
        }

        public void Release()
        {
            foreach (var kvp in m_GrassChunks)
            {
                kvp.Value.Release();
            }
            m_GrassChunks.Clear();
        }

        private static Vector4[] s_CachePlanes = new Vector4[6];
        private static Vector4[] s_BoundPoints = new Vector4[8];

        private Dictionary<int, GrassChunk> m_GrassChunks = new Dictionary<int, GrassChunk>();

        private Vector3 m_BoundCenter = new Vector3(0, 0.5f, 0);
        private Vector3 m_BoundSize = new Vector3(1, 1, 0.2f);
    }
}