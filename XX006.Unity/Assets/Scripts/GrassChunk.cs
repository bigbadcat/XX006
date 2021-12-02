using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 草快信息。
    /// </summary>
    public class GrassChunkInfo
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">草的编号。</param>
        public GrassChunkInfo(int id)
        {
            m_ID = id;
        }

        /// <summary>
        /// 添加一个草。
        /// </summary>
        /// <param name="trs">草的位置信息。</param>
        public void AddGrass(Matrix4x4 trs)
        {
            if (m_TRSDatas.Count == 0)
            {
                m_MinPos = m_MaxPos = new Vector3(trs.m03, trs.m13, trs.m23);
            }
            else
            {
                UpdateAABB(trs);
            }
            m_TRSDatas.Add(trs);            
            GeometryUtil.GetBoundPointsForAABB(m_MinPos, m_MaxPos, m_BoundPoints);
            m_Radius = Size.magnitude / 2;
        }

        /// <summary>
        /// 添加一批草。
        /// </summary>
        /// <param name="trs_list">草的位置信息列表。</param>
        public void AddGrass(List<Matrix4x4> trs_list)
        {
            if (trs_list.Count <= 0)
            {
                return;
            }

            if (m_TRSDatas.Count == 0)
            {
                Matrix4x4 trs = trs_list[0];
                m_MinPos = m_MaxPos = new Vector3(trs.m03, trs.m13, trs.m23);
            }
            m_TRSDatas.AddRange(trs_list);
            for (int i=1; i<m_TRSDatas.Count; ++i)
            {
                UpdateAABB(m_TRSDatas[i]);
            }
            GeometryUtil.GetBoundPointsForAABB(m_MinPos, m_MaxPos, m_BoundPoints);
            m_Radius = Size.magnitude / 2;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取草的编号。
        /// </summary>
        public int ID
        {
            get { return m_ID; }
        }

        /// <summary>
        /// 获取每颗草的世界变换。
        /// </summary>
        public List<Matrix4x4> TRSDatas
        {
            get { return m_TRSDatas; }
        }

        /// <summary>
        /// 获取草快边界最小位置。
        /// </summary>
        public Vector3 MinPos
        {
            get { return m_MinPos; }
        }

        /// <summary>
        /// 获取草快边界最大位置。
        /// </summary>
        public Vector3 MaxPos
        {
            get { return m_MaxPos; }
        }

        /// <summary>
        /// 获取草快边界中心位置。
        /// </summary>
        public Vector3 Center
        {
            get { return (m_MinPos + m_MaxPos) / 2; }
        }

        /// <summary>
        /// 获取草快边界尺寸。
        /// </summary>
        public Vector3 Size
        {
            get { return m_MaxPos - m_MinPos; }
        }

        /// <summary>
        /// 获取边界盒子。
        /// </summary>
        public Bounds Border
        {
            get { return new Bounds(Center, Size); }
        }

        /// <summary>
        /// 获取草快形边界的8个点。
        /// </summary>
        public Vector4[] BoundPoints
        {
            get { return m_BoundPoints; }
        }

        /// <summary>
        /// 获取草块距离中心点的最大半径。
        /// </summary>
        public float Radius
        {
            get { return m_Radius; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 更新边框。
        /// </summary>
        /// <param name="trs">草的位置信息。</param>
        private void UpdateAABB(Matrix4x4 trs)
        {
            //计算草的AABB
            var points = GrassManager.Instance.PlaneGrassBoundPoints;
            for (int i = 0; i < points.Length; ++i)
            {
                Vector3 wp = trs.MultiplyPoint(points[i]);
                m_MinPos.x = Mathf.Min(m_MinPos.x, wp.x);
                m_MinPos.y = Mathf.Min(m_MinPos.y, wp.y);
                m_MinPos.z = Mathf.Min(m_MinPos.z, wp.z);
                m_MaxPos.x = Mathf.Max(m_MaxPos.x, wp.x);
                m_MaxPos.y = Mathf.Max(m_MaxPos.y, wp.y);
                m_MaxPos.z = Mathf.Max(m_MaxPos.z, wp.z);
            }
        }

        #endregion

        #region 成员变量----------------------------------------------------------------

        #endregion

        /// <summary>
        /// 草快编号。
        /// </summary>
        private int m_ID = 0;

        /// <summary>
        /// 每颗草的世界变换。
        /// </summary>
        private List<Matrix4x4> m_TRSDatas = new List<Matrix4x4>();

        /// <summary>
        /// 草快边界最小位置。
        /// </summary>
        private Vector3 m_MinPos = Vector3.zero;

        /// <summary>
        /// 草快边界最大位置。
        /// </summary>
        private Vector3 m_MaxPos = Vector3.zero;

        /// <summary>
        /// 草快矩形边界的8个点。
        /// </summary>
        private Vector4[] m_BoundPoints = new Vector4[8];

        /// <summary>
        /// 草快最大半径。
        /// </summary>
        private float m_Radius = 0;
    }

    /// <summary>
    /// 草皮块。
    /// </summary>
    public class GrassChunk
    {

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 初始化。
        /// </summary>
        /// <param name="info">草快信息。</param>
        /// <param name="mesh">草网格。</param>
        /// <param name="mat">草材质。</param>
        public void Init(GrassChunkInfo info, Mesh[] mesh, Material[] mat)
        {
            m_ChunkInfo = info;
            m_GrassMesh = mesh;
            m_GrassMat = mat;
        }

        /// <summary>
        /// 绘制草快。
        /// </summary>
        /// <param name="lod">细节层级。</param>
        public void DrawGrass(int lod)
        {
            if (m_BufferCount != m_ChunkInfo.TRSDatas.Count)
            {
                UpdateBuffers();                
            }
            if (m_BufferCount <= 0)
            {
                return;
            }
            if (m_CurLOD != lod)
            {
                UpdateLOD(lod);
            }

            //剔除、风和压弯计算
            ComputeShader cs_culling = GrassManager.Instance.CullingCompute;
            int kernel = GrassManager.Instance.CullingKernel[lod];
            m_CullResult.SetCounterValue(0);
            cs_culling.SetInt(s_PID_InstanceCount, m_BufferCount);
            cs_culling.SetBuffer(kernel, s_PID_InstanceBuffer, m_TRSBuffer);
            if (m_CurLOD == 0)
            {
                cs_culling.SetBuffer(kernel, s_PID_BendBuffer, m_BendBuffer);
            }
            cs_culling.SetBuffer(kernel, s_PID_LOD_CullResult[lod], m_CullResult);
            cs_culling.Dispatch(kernel, (int)Mathf.Ceil(m_BufferCount / 512.0f), 1, 1);

            //绘制
            int layer = GrassManager.Instance.GrassLayer;
            ComputeBuffer.CopyCount(m_CullResult, m_ArgsBuffer, sizeof(uint));
            ShadowCastingMode scm = m_CurLOD <= 2 ? ShadowCastingMode.On : ShadowCastingMode.Off;
            m_GrassMat[lod].SetBuffer(s_PID_InstanceBuffer, m_CullResult);
            Graphics.DrawMeshInstancedIndirect(m_GrassMesh[lod], 0, m_GrassMat[lod], m_ChunkInfo.Border, m_ArgsBuffer, 0, null, scm, true, layer);
        }

        /// <summary>
        /// 释放资源。
        /// </summary>
        public void Release()
        {
            m_TRSBuffer?.Release();
            m_TRSBuffer = null;
            m_BendBuffer?.Release();
            m_BendBuffer = null;
            m_CullResult?.Release();
            m_CullResult = null;
            m_ArgsBuffer?.Release();
            m_ArgsBuffer = null;
            m_BufferCount = 0;
            for (int i = 0; i < m_GrassMat.Length; ++i)
            {
                GameObject.Destroy(m_GrassMat[i]);
            }
            m_GrassMat = null;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取草快信息。
        /// </summary>
        public GrassChunkInfo ChunkInfo
        {
            get { return m_ChunkInfo; }
        }

        /// <summary>
        /// 获取草网格。
        /// </summary>
        public Mesh[] GrassMesh
        {
            get { return m_GrassMesh; }
        }

        /// <summary>
        /// 获取草材质。
        /// </summary>
        public Material[] GrassMat
        {
            get { return m_GrassMat; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 初始化静态数据。
        /// </summary>
        static GrassChunk()
        {
            s_PID_InstanceCount = Shader.PropertyToID("_InstanceCount");
            s_PID_InstanceBuffer = Shader.PropertyToID("_InstancingBuffer");
            s_PID_BendBuffer = Shader.PropertyToID("_BendBuffer");
            s_PID_LOD_CullResult[0] = Shader.PropertyToID("_CullResult");
            s_PID_LOD_CullResult[1] = Shader.PropertyToID("_CullResult12");
            s_PID_LOD_CullResult[2] = Shader.PropertyToID("_CullResult12");
            s_PID_LOD_CullResult[3] = Shader.PropertyToID("_CullResult3");
        }

        /// <summary>
        /// 更新缓存数据。
        /// </summary>
        private void UpdateBuffers()
        {
            m_TRSBuffer?.Release();
            m_TRSBuffer = null;
            m_BendBuffer?.Release();
            m_BendBuffer = null;
            

            m_BufferCount = m_ChunkInfo.TRSDatas.Count;
            m_TRSBuffer = new ComputeBuffer(m_BufferCount, sizeof(float) * 16);
            m_TRSBuffer.SetData(m_ChunkInfo.TRSDatas);

            Vector4[] bend_buff = new Vector4[m_BufferCount];
            m_BendBuffer = new ComputeBuffer(m_BufferCount, sizeof(float) * 4);
            m_BendBuffer.SetData(bend_buff);
            UpdateCullResult();
        }

        /// <summary>
        /// 更新绘制参数。
        /// </summary>
        /// <param name="lod">细节层级。</param>
        private void UpdateLOD(int lod)
        {
            Mesh mesh = m_GrassMesh[lod];
            m_CurLOD = lod;
            if (m_ArgsBuffer == null)
            {
                m_ArgsBuffer = new ComputeBuffer(1, s_CacheArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            }
            s_CacheArgs[0] = (uint)mesh.GetIndexCount(0);
            s_CacheArgs[1] = (uint)m_BufferCount;
            s_CacheArgs[2] = (uint)mesh.GetIndexStart(0);
            s_CacheArgs[3] = (uint)mesh.GetBaseVertex(0);
            m_ArgsBuffer.SetData(s_CacheArgs);
            UpdateCullResult();
        }

        /// <summary>
        /// 更新剔除结果缓冲区。
        /// </summary>
        private void UpdateCullResult()
        {
            if (m_BufferCount <= 0 || m_CurLOD < 0)
            {
                return;
            }

            m_CullResult?.Release();
            m_CullResult = null;
            m_CullResult = new ComputeBuffer(m_BufferCount, sizeof(float) * s_LOD_INSTANCING_SIZE[m_CurLOD], ComputeBufferType.Append);
        }

        #endregion

        #region 成员变量----------------------------------------------------------------

        #region Shader属性ID

        private static int s_PID_InstanceCount = 0;
        private static int s_PID_InstanceBuffer = 0;
        private static int s_PID_BendBuffer = 0;
        private static int[] s_PID_LOD_CullResult = new int[4];

        #endregion

        /// <summary>
        /// 绘制参数缓存数据。
        /// </summary>
        private static uint[] s_CacheArgs = new uint[5] { 0, 0, 0, 0, 0 };

        /// <summary>
        /// 草实例不同LOD的数据大小。
        /// </summary>
        private static int[] s_LOD_INSTANCING_SIZE = new int[4] {24, 20, 20, 16 };

        /// <summary>
        /// 草快信息。
        /// </summary>
        private GrassChunkInfo m_ChunkInfo = null;

        /// <summary>
        /// 草网格。
        /// </summary>
        private Mesh[] m_GrassMesh = null;

        /// <summary>
        /// 草快材质。
        /// </summary>
        private Material[] m_GrassMat = null;

        /// <summary>
        /// 缓冲区对应的实例数量。
        /// </summary>
        private int m_BufferCount = 0;

        /// <summary>
        /// 草变换数据缓冲区。
        /// </summary>
        private ComputeBuffer m_TRSBuffer;

        /// <summary>
        /// 草压弯状态缓冲区。
        /// </summary>
        private ComputeBuffer m_BendBuffer;

        /// <summary>
        /// 剔除结果缓冲区。(最终要绘制的实例数据)
        /// </summary>
        private ComputeBuffer m_CullResult;

        /// <summary>
        /// 当前LOD。
        /// </summary>
        private int m_CurLOD = -1;

        /// <summary>
        /// 绘制参数缓冲区。
        /// </summary>
        private ComputeBuffer m_ArgsBuffer;

        #endregion
    }
}