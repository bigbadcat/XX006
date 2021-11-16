using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    public class GrassManager
    {
        public void AddGrass(int id, Vector3 pos, Matrix4x4 trs)
        {
            GrassChunk ck;
            if (!m_GrassChunks.TryGetValue(id, out ck))
            {
                ck = new GrassChunk();
                m_GrassChunks.Add(id, ck);
            }
            ck.AddGrass(pos, trs);
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
            foreach (var kvp in m_GrassChunks)
            {
                kvp.Value.DrawGrass(wind, gap, dir, mesh, submeshIndex, camera);
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

        private Dictionary<int, GrassChunk> m_GrassChunks = new Dictionary<int, GrassChunk>();
    }
}