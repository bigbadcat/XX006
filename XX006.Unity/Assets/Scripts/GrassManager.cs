using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XX006
{
    public static class GeometryUtil
    {
        /// <summary>
        /// 获取垂直与某个法线且经过特定点的平面方程。
        /// </summary>
        /// <param name="normal">垂直平面的法线。</param>
        /// <param name="point">要经过的点。</param>
        /// <returns>xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</returns>
        public static Vector4 GetPlane(Vector3 normal, Vector3 point)
        {
            return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point));
        }

        /// <summary>
        /// 通过三点获取一个平面方程。点的顺时针的面表示正面。
        /// </summary>
        /// <param name="a">A点。</param>
        /// <param name="b">B点。</param>
        /// <param name="c">C点。</param>
        /// <returns>xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</returns>
        public static Vector4 GetPlane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            return GetPlane(normal, a);
        }

        /// <summary>
        /// 获取摄像机视锥体远平面的四个点。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左下，右下，左上，右上四个点的世界坐标。</returns>
        public static Vector3[] GetCameraFarClipPlanePoint(Camera camera)
        {
            Vector3[] points = new Vector3[4];
            GetCameraFarClipPlanePoint(camera, points);
            return points;
        }

        /// <summary>
        /// 获取摄像机视锥体远平面的四个点。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左下，右下，左上，右上四个点的世界坐标。</returns>
        public static Vector3[] GetCameraFarClipPlanePoint(Camera camera, Vector3[] points)
        {
            Transform transform = camera.transform;
            float distance = camera.farClipPlane;
            float halfFovRad = Mathf.Deg2Rad * camera.fieldOfView * 0.5f;
            float upLen = distance * Mathf.Tan(halfFovRad);
            float rightLen = upLen * camera.aspect;
            Vector3 farCenterPoint = transform.position + distance * transform.forward;
            Vector3 up = upLen * transform.up;
            Vector3 right = rightLen * transform.right;
            points[0] = farCenterPoint - up - right;//left-bottom
            points[1] = farCenterPoint - up + right;//right-bottom
            points[2] = farCenterPoint + up - right;//left-up
            points[3] = farCenterPoint + up + right;//right-up
            return points;
        }

        private static Vector3[] s_CachePoints = new Vector3[4];

        /// <summary>
        /// 获取摄像机视锥体的六个平面。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左右下上近远六个面方程数组。</returns>
        public static Vector4[] GetFrustumPlane(Camera camera)
        {
            Vector4[] planes = new Vector4[6];
            GetFrustumPlane(camera, planes);
            return planes;
        }

        /// <summary>
        /// 获取摄像机视锥体的六个平面。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左右下上近远六个面方程数组。</returns>
        public static Vector4[] GetFrustumPlane(Camera camera, Vector4[] planes)
        {
            Transform transform = camera.transform;
            Vector3 cameraPosition = transform.position;
            Vector3[] points = GetCameraFarClipPlanePoint(camera, s_CachePoints);

            //按顺时针传入点坐标。
            planes[0] = GetPlane(cameraPosition, points[0], points[2]);     //left
            planes[1] = GetPlane(cameraPosition, points[3], points[1]);     //right
            planes[2] = GetPlane(cameraPosition, points[1], points[0]);     //bottom
            planes[3] = GetPlane(cameraPosition, points[2], points[3]);     //up
            planes[4] = GetPlane(-transform.forward, transform.position + transform.forward * camera.nearClipPlane);//near
            planes[5] = GetPlane(transform.forward, transform.position + transform.forward * camera.farClipPlane);//far
            return planes;
        }

        /// <summary>
        /// 判断一个点是否在平面法线指向的那一侧(外侧)。
        /// </summary>
        /// <param name="plane">平面方程。</param>
        /// <param name="point">点坐标。</param>
        /// <returns>是否在外侧。</returns>
        public static bool IsOutsideThePlane(Vector4 plane, Vector3 point)
        {
            return Vector3.Dot(plane, point) + plane.w > 0;
        }

        /// <summary>
        /// 获取边框的8个点。
        /// </summary>
        /// <param name="center">边框中心。</param>
        /// <param name="size">边框大小。</param>
        /// <param name="points">保存边框八个点的数组。</param>
        /// <returns>保存边框八个点的数组。/returns>
        public static Vector4[] GetBoundPoints(Vector3 center, Vector3 size, Vector4[] points)
        {
            Vector3 boundMin = center - size / 2;
            Vector3 boundMax = center + size / 2;
            points[0] = new Vector4(boundMin.x, boundMin.y, boundMin.z, 1);
            points[1] = new Vector4(boundMax.x, boundMax.y, boundMax.z, 1);
            points[2] = new Vector4(boundMax.x, boundMax.y, boundMin.z, 1);
            points[3] = new Vector4(boundMax.x, boundMin.y, boundMax.z, 1);
            points[4] = new Vector4(boundMax.x, boundMin.y, boundMin.z, 1);
            points[5] = new Vector4(boundMin.x, boundMax.y, boundMax.z, 1);
            points[6] = new Vector4(boundMin.x, boundMax.y, boundMin.z, 1);
            points[7] = new Vector4(boundMin.x, boundMin.y, boundMax.z, 1);
            return points;
        }
    }

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
        /// 高中草。(会与风飘动，会产生阴影，会与角色交互)
        /// </summary>
        Height = 3,
    }


    /// <summary>
    /// 草皮块。
    /// </summary>
    public class GrassChunk
    {
        public static void UpdateHizDepthTexture()
        {
            if (s_HizDepthUpdateFrame == Time.frameCount || s_HizMipmapMat == null)
            {
                return;
            }

            RenderTexture depth_tex = HizDepthTexture;
            s_HizDepthUpdateFrame = Time.frameCount;

            int w = depth_tex.width;
            int mipmap_level = 0;
            RenderTexture last_rt = null;   //上一层的mipmap，即mipmapLevel-1对应的mipmap
            while (w >= 4)
            {
                RenderTexture cur_rt = RenderTexture.GetTemporary(w, w, 0, RenderTextureFormat.RHalf);
                cur_rt.filterMode = FilterMode.Point;
                if (last_rt == null)
                {
                    //Mipmap[0]，copy原始的深度图，需要先用Graphics.Blit将原色深度图拉伸到cur_rt中，再CopyTexture
                    Graphics.Blit(Shader.GetGlobalTexture(s_PID_CameraDepthTexture), cur_rt);
                }
                else
                {
                    //通过HizMipmap对应的材质将Mipmap[i]Blit到Mipmap[i+1]上
                    s_HizMipmapMat.SetTexture("_MainTex", last_rt);
                    Graphics.Blit(last_rt, cur_rt, s_HizMipmapMat);
                    RenderTexture.ReleaseTemporary(last_rt);
                }
                Graphics.CopyTexture(cur_rt, 0, 0, depth_tex, 0, mipmap_level);
                last_rt = cur_rt;

                w /= 2;
                mipmap_level++;
            }

            RenderTexture.ReleaseTemporary(last_rt);
            last_rt = null;
        }

        public static RenderTexture HizDepthTexture
        {
            get
            {
                if (s_HizDepthTexture == null)
                {
                    int size = Mathf.Min(256, Mathf.NextPowerOfTwo(Mathf.Max(Screen.width, Screen.height)));
                    s_HizDepthTexture = new RenderTexture(size, size, 0, RenderTextureFormat.RHalf);           //16的红色通道保持深度值
                    s_HizDepthTexture.autoGenerateMips = false;        //Mipmap手动生成
                    s_HizDepthTexture.useMipMap = true;
                    s_HizDepthTexture.filterMode = FilterMode.Point;
                    s_HizDepthTexture.Create();
                }
                return s_HizDepthTexture;
            }
        }

        public static Material HizMipmapMat
        {
            get { return s_HizMipmapMat; }
            set
            {
                s_HizMipmapMat = value;
                if (s_HizMipmapMat != null)
                {
                    s_PID_CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
                }
            }
        }

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
                    s_PID_HizDepthTextureSize = Shader.PropertyToID("_HizDepthTextureSize");
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
        private static Material s_HizMipmapMat;
        private static ComputeShader s_ViewFrustumCulling;
        private static int s_PID_CameraDepthTexture = 0;
        private static int s_Kernel = 0;
        private static int s_PID_CameraPlanes = 0;
        private static int s_PID_CameraVPMatrix = 0;
        private static int s_PID_BoundPoints = 0;
        private static int s_PID_HizDepthTexture = 0;
        private static int s_PID_HizDepthTextureSize = 0;
        private static int s_PID_Wind = 0;
        private static int s_PID_WindGap = 0;
        private static int s_PID_WindDir = 0;
        private static int s_PID_WindNoise = 0;
        private static int s_PID_InstanceCount = 0;        

        /// <summary>
        /// 带mipmap的深度图。
        /// </summary>
        private static RenderTexture s_HizDepthTexture = null;
        private static Texture s_WindNoise = null;

        private static int s_HizDepthUpdateFrame = -1;

        public void AddGrass(Vector3 pos, Matrix4x4 trs)
        {
            //m_Pos[m_Count] = pos;
            GrassData data;
            data.trs = trs;
            data.wind = Vector4.zero;
            data.bend = Vector4.zero;
            //data.rate2 = 0;
            m_Datas.Add(data);

            Vector3 p = new Vector3(trs.m03, trs.m13, trs.m23);
            if (m_Datas.Count == 1)
            {
                m_MinPos = pos;
                m_MaxPos = pos;
            }
            else
            {
                m_MinPos.x = Mathf.Min(m_MinPos.x, p.x);
                m_MinPos.y = Mathf.Min(m_MinPos.y, p.y);
                m_MinPos.z = Mathf.Min(m_MinPos.z, p.z);

                m_MaxPos.x = Mathf.Max(m_MaxPos.x, p.x);
                m_MaxPos.y = Mathf.Max(m_MaxPos.y, p.y);
                m_MaxPos.z = Mathf.Max(m_MaxPos.z, p.z);
            }
        }

        private static Vector4[] s_CachePlanes = new Vector4[6];
        private static Vector4[] s_BoundPoints = new Vector4[8];

        public void DrawGrass(float wind, float gap, Vector3 dir, Mesh mesh, int submeshIndex, Material material)
        {
            if (m_CurCount != m_Datas.Count)
            {
                UpdateBuffers(mesh);
            }
            if (m_CurCount > 0 && s_ViewFrustumCulling != null && s_HizDepthTexture != null)
            {
                //UpdateHizDepthTexture();

                //设置剔除参数
                Matrix4x4 vp = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, false) * Camera.main.worldToCameraMatrix;      //VP矩阵
                ViewFrustumCulling.SetMatrix(s_PID_CameraVPMatrix, vp);
                //ViewFrustumCulling.SetVectorArray(s_PID_CameraPlanes, GeometryUtil.GetFrustumPlane(Camera.main, s_CachePlanes));
                ViewFrustumCulling.SetVectorArray(s_PID_BoundPoints, GeometryUtil.GetBoundPoints(m_BoundCenter, m_BoundSize, s_BoundPoints));
                ViewFrustumCulling.SetTexture(s_Kernel, s_PID_HizDepthTexture, s_HizDepthTexture);
                ViewFrustumCulling.SetInt(s_PID_HizDepthTextureSize, s_HizDepthTexture.width);

                //设置风的参数
                float fm = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
                Vector4 wdir = new Vector4(dir.x, dir.y, dir.z, fm);
                //Vector3 rpos = s_RoleTarget == null ? Vector3.zero : s_RoleTarget.position;
                ViewFrustumCulling.SetFloat(s_PID_Wind, wind);
                ViewFrustumCulling.SetFloat(s_PID_WindGap, gap);
                ViewFrustumCulling.SetVector(s_PID_WindDir, wdir);
                ViewFrustumCulling.SetTexture(s_Kernel, s_PID_WindNoise, s_WindNoise);
                for (int i=0; i< s_RoleTargetCount; ++i)
                {
                    Vector3 p = s_RoleTargets[i].position;
                    s_RolePositions[i] = new Vector4(p.x, p.y, p.z, 1);
                }
                ViewFrustumCulling.SetInt("_RoleCount", s_RoleTargetCount);
                ViewFrustumCulling.SetVectorArray("_RoleInfos", s_RolePositions);

                //设置要准备绘制的实例参数
                m_CullResult.SetCounterValue(0);
                ViewFrustumCulling.SetInt(s_PID_InstanceCount, m_CurCount);
                ViewFrustumCulling.SetBuffer(s_Kernel, "_InstancingBuffer", m_LocalToWorldMatrixBuffer);
                ViewFrustumCulling.SetBuffer(s_Kernel, "_CullResult", m_CullResult);
                ViewFrustumCulling.Dispatch(s_Kernel, (int)Mathf.Ceil(m_CurCount / 128.0f), 1, 1);

                ComputeBuffer.CopyCount(m_CullResult, m_ArgsBuffer, sizeof(uint));
                material.SetBuffer("_InstancingBuffer", m_CullResult);
                material.EnableKeyword("SHADOWS_SCREEN");
                material.EnableKeyword("SHADOWS_DEPTH");

                Bounds bounds = new Bounds((m_MinPos + m_MaxPos) / 2, m_MaxPos - m_MinPos);
                Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, m_ArgsBuffer, 0, null, UnityEngine.Rendering.ShadowCastingMode.On, true);
            }
        }

        public void Release()
        {
            m_LocalToWorldMatrixBuffer?.Release();
            m_LocalToWorldMatrixBuffer = null;
            m_CullResult?.Release();
            m_CullResult = null;
            m_ArgsBuffer?.Release();
            m_ArgsBuffer = null;
            m_CurCount = 0;
        }

        private void UpdateBuffers(Mesh mesh)
        {
            m_LocalToWorldMatrixBuffer?.Release();
            m_LocalToWorldMatrixBuffer = null;
            m_CullResult?.Release();
            m_CullResult = null;

            m_CurCount = m_Datas.Count;
            m_LocalToWorldMatrixBuffer = new ComputeBuffer(m_CurCount, sizeof(float) * (16 + 8));
            m_LocalToWorldMatrixBuffer.SetData(m_Datas);
            m_CullResult = new ComputeBuffer(m_CurCount, sizeof(float) * (16 + 1), ComputeBufferType.Append);

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


        private List<GrassData> m_Datas = new List<GrassData>();
        private int m_CurCount = 0;
        private ComputeBuffer m_LocalToWorldMatrixBuffer;
        private ComputeBuffer m_CullResult;
        private uint[] m_Args = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer m_ArgsBuffer;

        private Vector3 m_BoundCenter = new Vector3(0, 0.5f, 0);
        private Vector3 m_BoundSize = new Vector3(1, 1, 0.2f);

        private Vector3 m_MinPos = Vector3.zero;
        private Vector3 m_MaxPos = Vector3.zero;
    }


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

        /// <summary>
        /// 移除草。
        /// </summary>
        /// <param name="map">地图编号。</param>
        public void RemoveGrass(int map)
        {
            m_GrassChunks.Remove(map);
        }

        public void DrawGrass(float wind, float gap, Vector3 dir, Mesh mesh, int submeshIndex, Material material)
        {
            foreach (var kvp in m_GrassChunks)
            {
                kvp.Value.DrawGrass(wind, gap, dir, mesh, submeshIndex, material);
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