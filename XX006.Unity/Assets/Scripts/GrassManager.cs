using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 草的级别。
    /// </summary>
    public enum GrassLevel
    {
        /// <summary>
        /// 静态草。(不会与风飘动，不会产生阴影，不会与角色交互)
        /// </summary>
        Static = 0,

        /// <summary>
        /// 低草。(会与风飘动，不会产生阴影，不会与角色交互)
        /// </summary>
        Low = 1,

        /// <summary>
        /// 中草。(会与风飘动，会产生阴影，不会与角色交互)
        /// </summary>
        Middle = 2,

        /// <summary>
        /// 高草。(会与风飘动，会产生阴影，会与角色交互)
        /// </summary>
        Height = 3,
    }

    /// <summary>
    /// 草地管理器。
    /// </summary>
    public class GrassManager : Singleton<GrassManager>
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 初始化。
        /// </summary>
        protected override void Init()
        {
            Vector3 center = new Vector3(0, 0.5f, 0);
            Vector3 size = new Vector3(1, 1, 0.2f);
            GeometryUtil.GetBoundPoints(center, size, m_PlaneGrassBoundPoints);

            m_PID_Wind = Shader.PropertyToID("_Wind");
            m_PID_WindGap = Shader.PropertyToID("_WindGap");
            m_PID_WindDir = Shader.PropertyToID("_WindDir");
            m_PID_WindNoise = Shader.PropertyToID("_WindNoise");
            m_PID_CameraVPMatrix = Shader.PropertyToID("_CameraVPMatrix");
            m_PID_BoundPoints = Shader.PropertyToID("_BoundPoints");

            m_GrassLayer = LayerMask.NameToLayer("Grass");
        }

        /// <summary>
        /// 释放。
        /// </summary>
        protected override void Release()
        {
            foreach (var kvp in m_GrassChunks)
            {
                kvp.Value.Release();
            }
            m_GrassChunks.Clear();
        }

        /// <summary>
        /// 添加草块。
        /// </summary>
        /// <param name="id">草块编号。</param>
        /// <param name="chunk">草块对象。</param>
        public void AddGrass(int id, GrassChunk chunk)
        {
#if UNITY_EDITOR
            if (m_GrassChunks.ContainsKey(id))
            {
                Log.Error("The grass chunk with ID {0} already exists", id);
                return;
            }
#endif
            m_GrassChunks.Add(id, chunk);
        }

        /// <summary>
        /// 移除草块。
        /// </summary>
        /// <param name="id">草块编号。</param>
        public void RemoveGrass(int id)
        {
            GrassChunk ck;
            if (m_GrassChunks.TryGetValue(id, out ck))
            {
                m_GrassChunks.Remove(id);
                ck.Release();
                ck = null;
            }            
        }

        /// <summary>
        /// 绘制草内容。
        /// </summary>
        /// <param name="wind">风的距离。</param>
        /// <param name="gap">风周期间隔。</param>
        /// <param name="dir">风的方向。</param>
        /// <param name="camera">剔除计算的摄像机。</param>
        public void DrawGrass(float wind, float gap, Vector3 dir, Camera camera)
        {
            if (m_CullingCompute == null || m_GrassChunks.Count <= 0)
            {
                return;
            }

            //设置摄像机视锥和草边框信息
            Matrix4x4 vp = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false) * camera.worldToCameraMatrix;      //VP矩阵
            m_CullingCompute.SetMatrix(m_PID_CameraVPMatrix, vp);
            m_CullingCompute.SetVectorArray(m_PID_BoundPoints, m_PlaneGrassBoundPoints);

            //设置风的参数
            float fm = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
            Vector4 wdir = new Vector4(dir.x, dir.y, dir.z, fm);
            m_CullingCompute.SetFloat(m_PID_Wind, wind);
            m_CullingCompute.SetFloat(m_PID_WindGap, gap);
            m_CullingCompute.SetVector(m_PID_WindDir, wdir);
            m_CullingCompute.SetTexture(m_CullingKernel, m_PID_WindNoise, m_WindNoise);

            //设置压弯参数
            UpdateBendInfo();
            m_CullingCompute.SetFloat("_BendDelta", Time.deltaTime / 2);
            m_CullingCompute.SetInt("_BendCount", m_BendCount);
            m_CullingCompute.SetVectorArray("_BendInfos", m_BendInfos);

            //CPU层队草快级别(量小)的视锥剔除后再队草块进行绘制
            GeometryUtil.GetFrustumPlane(camera, m_CacheFrustumPlanes);
            foreach (var kvp in m_GrassChunks)
            {
                if (GeometryUtil.IsIntersect(kvp.Value.ChunkInfo.BoundPoints, m_CacheFrustumPlanes))
                {
                    kvp.Value.DrawGrass();
                }
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取片面草的边框点AABB。
        /// </summary>
        public Vector4[] PlaneGrassBoundPoints
        {
            get { return m_PlaneGrassBoundPoints; }
        }

        /// <summary>
        /// 获取或设置草的计算Shader。
        /// </summary>
        public ComputeShader CullingCompute
        {
            get { return m_CullingCompute; }
            set
            {
                m_CullingCompute = value;
                if (m_CullingCompute != null)
                {
                    m_CullingKernel = m_CullingCompute.FindKernel("CSGrassCullingHeight");
                }
                else
                {
                    m_CullingKernel = 0;
                }
            }
        }

        /// <summary>
        /// 获取计算核心标识。
        /// </summary>
        public int CullingKernel
        {
            get { return m_CullingKernel; }
        }

        /// <summary>
        /// 草Layer。
        /// </summary>
        public int GrassLayer
        {
            get { return m_GrassLayer; }
        }

        /// <summary>
        /// 获取或设置风力噪音纹理。
        /// </summary>
        public Texture WindNoise
        {
            get { return m_WindNoise; }
            set { m_WindNoise = value; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 更新压弯信息。
        /// </summary>
        private void UpdateBendInfo()
        {
            var bend_areas = BendArea.CurAreas;
            m_BendCount = bend_areas.Count;
            for (int i = 0; i < bend_areas.Count; ++i)
            {
                m_BendInfos[i] = bend_areas[i].Info;
            }
        }

        #endregion

        #region 成员变量----------------------------------------------------------------

        #region Shader属性ID

        private int m_PID_Wind = 0;
        private int m_PID_WindGap = 0;
        private int m_PID_WindDir = 0;
        private int m_PID_WindNoise = 0;
        private int m_PID_CameraVPMatrix = 0;
        private int m_PID_BoundPoints = 0;

        #endregion

        /// <summary>
        /// 草的剔除计算。
        /// </summary>
        private ComputeShader m_CullingCompute;

        /// <summary>
        /// 计算核心标识。
        /// </summary>
        private int m_CullingKernel = 0;

        /// <summary>
        /// 草Layer。
        /// </summary>
        private int m_GrassLayer = 0;

        /// <summary>
        /// 风力噪音图。
        /// </summary>
        private Texture m_WindNoise = null;

        /// <summary>
        /// 压弯对象数量。
        /// </summary>
        private int m_BendCount = 0;

        /// <summary>
        /// 压弯信息列表。(xyz表示坐标，w表示范围缩放系数)
        /// </summary>
        private Vector4[] m_BendInfos = new Vector4[8];

        /// <summary>
        /// 缓存视锥平面数据。
        /// </summary>
        private Vector4[] m_CacheFrustumPlanes = new Vector4[6];

        /// <summary>
        /// 片面草的边框点AABB。
        /// </summary>
        private Vector4[] m_PlaneGrassBoundPoints = new Vector4[8];

        /// <summary>
        /// 当前的草快信息。
        /// </summary>
        private Dictionary<int, GrassChunk> m_GrassChunks = new Dictionary<int, GrassChunk>();

        #endregion
    }
}