using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    public struct GrassData
    {
        public Matrix4x4 trs;
        public Vector4 wind;
        public Vector4 bend;
    }

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
    /// 草皮块。
    /// </summary>
    public class GrassChunk
    {

        public static Texture WindNoise
        {
            get { return s_WindNoise; }
            set
            {
                s_WindNoise = value;
                if (s_WindNoise != null)
                {
                    s_PID_WindNoise = Shader.PropertyToID("_WindNoise");
                }
            }
        }

        public static ComputeShader ViewFrustumCulling
        {
            get { return s_ViewFrustumCulling; }
            set
            {
                s_ViewFrustumCulling = value;
                if (s_ViewFrustumCulling != null)
                {
                    s_Kernel = ViewFrustumCulling.FindKernel("CSMain");
                    s_PID_CameraPlanes = Shader.PropertyToID("_CameraPlanes");
                    s_PID_CameraVPMatrix = Shader.PropertyToID("_CameraVPMatrix");
                    s_PID_HizDepthTexture = Shader.PropertyToID("_HizDepthTexture");
                    s_PID_BoundPoints = Shader.PropertyToID("_BoundPoints");
                    s_PID_Wind = Shader.PropertyToID("_Wind");
                    s_PID_WindGap = Shader.PropertyToID("_WindGap");
                    s_PID_WindDir = Shader.PropertyToID("_WindDir");
                    s_PID_InstanceCount = Shader.PropertyToID("_InstanceCount");
                }
            }
        }

        public static int s_RoleTargetCount = 0;
        public static Transform[] s_RoleTargets = new Transform[8];
        public static Vector4[] s_RolePositions = new Vector4[8];

        private static ComputeShader s_ViewFrustumCulling;
        public static int s_Kernel = 0;

        public static int s_PID_CameraPlanes = 0;
        public static int s_PID_CameraVPMatrix = 0;
        public static int s_PID_BoundPoints = 0;
        public static int s_PID_HizDepthTexture = 0;
        public static int s_PID_Wind = 0;
        public static int s_PID_WindGap = 0;
        public static int s_PID_WindDir = 0;
        public static int s_PID_WindNoise = 0;
        public static int s_PID_InstanceCount = 0;

        public static Texture s_WindNoise = null;

        public void AddGrass(List<Matrix4x4> trs_list)
        {
            if (trs_list.Count <= 0)
            {
                return;
            }
            if (m_Datas.Count == 0)
            {
                Matrix4x4 trs = trs_list[0];
                m_MinPos = m_MaxPos = new Vector3(trs.m03, trs.m13, trs.m23);
            }
            m_Datas.AddRange(trs_list);
            foreach (var trs in trs_list)
            {
                //用草的位置预留2的边框，精确的做法是计算草的AABB
                Vector3 p = new Vector3(trs.m03, trs.m13, trs.m23);
                m_MinPos.x = Mathf.Min(m_MinPos.x, p.x - 2);
                m_MinPos.y = Mathf.Min(m_MinPos.y, p.y - 2);
                m_MinPos.z = Mathf.Min(m_MinPos.z, p.z - 2);
                m_MaxPos.x = Mathf.Max(m_MaxPos.x, p.x + 2);
                m_MaxPos.y = Mathf.Max(m_MaxPos.y, p.y + 2);
                m_MaxPos.z = Mathf.Max(m_MaxPos.z, p.z + 2);
            }
            GeometryUtil.GetBoundPointsForAABB(m_MinPos, m_MaxPos, m_BoundPoints);
        }

        public Material GrassMat
        {
            get { return m_GrassMat; }
            set { m_GrassMat = value; }
        }


        public Vector3 MinPos
        {
            get { return m_MinPos; }
        }
        public Vector3 MaxPos
        {
            get { return m_MaxPos; }
        }

        public Vector3 Center
        {
            get { return (m_MinPos + m_MaxPos)/ 2; }
        }

        public Vector3 Size
        {
            get { return m_MaxPos - m_MinPos; }
        }

        public Vector4[] BoundPoints
        {
            get { return m_BoundPoints; }
        }

        public void DrawGrass(float wind, float gap, Vector3 dir, Mesh mesh, int submeshIndex, Camera camera)
        {
            if (m_CurCount != m_Datas.Count)
            {
                UpdateBuffers(mesh);
            }
            if (m_CurCount > 0 && s_ViewFrustumCulling != null)
            {
                //设置要准备绘制的实例参数
                m_CullResult.SetCounterValue(0);
                ViewFrustumCulling.SetInt(s_PID_InstanceCount, m_CurCount);
                ViewFrustumCulling.SetBuffer(s_Kernel, "_InstancingBuffer", m_LocalToWorldMatrixBuffer);
                ViewFrustumCulling.SetFloat("_BendDelta", Time.deltaTime / 2);
                ViewFrustumCulling.SetBuffer(s_Kernel, "_BendBuffer", m_BendBuffer);
                ViewFrustumCulling.SetBuffer(s_Kernel, "_CullResult", m_CullResult);
                ViewFrustumCulling.Dispatch(s_Kernel, (int)Mathf.Ceil(m_CurCount / 512.0f), 1, 1);

                ComputeBuffer.CopyCount(m_CullResult, m_ArgsBuffer, sizeof(uint));
                m_GrassMat.SetBuffer("_InstancingBuffer", m_CullResult);
#if UNITY_EDITOR
                camera = null;
#endif
                Bounds bounds = new Bounds((m_MinPos + m_MaxPos) / 2, m_MaxPos - m_MinPos);
                Graphics.DrawMeshInstancedIndirect(mesh, 0, m_GrassMat, bounds, m_ArgsBuffer, 0, null, UnityEngine.Rendering.ShadowCastingMode.On, true, 0, camera);
            }
        }

        public void Release()
        {
            m_LocalToWorldMatrixBuffer?.Release();
            m_LocalToWorldMatrixBuffer = null;
            m_BendBuffer?.Release();
            m_BendBuffer = null;
            m_CullResult?.Release();
            m_CullResult = null;
            m_ArgsBuffer?.Release();
            m_ArgsBuffer = null;
            m_CurCount = 0;
            GameObject.Destroy(m_GrassMat);
            m_GrassMat = null;
        }

        private void UpdateBuffers(Mesh mesh)
        {
            m_LocalToWorldMatrixBuffer?.Release();
            m_LocalToWorldMatrixBuffer = null;
            m_BendBuffer?.Release();
            m_BendBuffer = null;
            m_CullResult?.Release();
            m_CullResult = null;

            m_CurCount = m_Datas.Count;
            m_LocalToWorldMatrixBuffer = new ComputeBuffer(m_CurCount, sizeof(float) * 16);
            m_LocalToWorldMatrixBuffer.SetData(m_Datas);

            Vector4[] bend_buff = new Vector4[m_CurCount];
            m_BendBuffer = new ComputeBuffer(m_CurCount, sizeof(float) * 4);
            m_BendBuffer.SetData(bend_buff);

            m_CullResult = new ComputeBuffer(m_CurCount, sizeof(float) * (16 + 8), ComputeBufferType.Append);

            if (m_ArgsBuffer == null)
            {
                m_ArgsBuffer = new ComputeBuffer(1, m_Args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
            }
            m_Args[0] = (uint)mesh.GetIndexCount(0);
            m_Args[1] = (uint)m_CurCount;
            m_Args[2] = (uint)mesh.GetIndexStart(0);
            m_Args[3] = (uint)mesh.GetBaseVertex(0);
            m_ArgsBuffer.SetData(m_Args);
        }


        private List<Matrix4x4> m_Datas = new List<Matrix4x4>();
        private int m_CurCount = 0;
        private ComputeBuffer m_LocalToWorldMatrixBuffer;
        private ComputeBuffer m_BendBuffer;
        private ComputeBuffer m_CullResult;
        private uint[] m_Args = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer m_ArgsBuffer;

        private Vector3 m_MinPos = Vector3.zero;
        private Vector3 m_MaxPos = Vector3.zero;
        private Vector4[] m_BoundPoints = new Vector4[8];

        private Material m_GrassMat;
    }

}