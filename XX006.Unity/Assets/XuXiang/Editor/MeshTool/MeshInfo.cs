using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 顶点信息。
    /// </summary>
    public class VertexInfo
    {
        /// <summary>
        /// 更新最大夹角。
        /// </summary>
        /// <param name="minfo">网格信息。</param>
        public void UpdateMaxAngle(MeshInfo minfo)
        {
            var tinfos = minfo.FindTriangle(this);   
            MaxAngle = TriangleInfo.GetMaxAngle(tinfos);
        }

        /// <summary>
        /// 索引。
        /// </summary>
        public int Index { get; set; } = 0;

        /// <summary>
        /// 坐标。
        /// </summary>
        public Vector3 Position { get; set; } = Vector3.zero;

        /// <summary>
        /// 法线。
        /// </summary>
        public Vector3 Normal { get; set; } = Vector3.up;

        /// <summary>
        /// 纹理坐标。
        /// </summary>
        public Vector2 UV { get; set; } = Vector2.zero;

        /// <summary>
        /// 获取最大夹角。
        /// </summary>
        public float MaxAngle { get; private set; } = 0;

        /// <summary>
        /// 是否为边界点。
        /// </summary>
        public bool IsBoundary { get; set; } = false;
    }

    /// <summary>
    /// 三角形信息。
    /// </summary>
    public class TriangleInfo
    {
        /// <summary>
        /// 获取三角形之间最大夹角。
        /// </summary>
        /// <param name="tinfos">三角形列表。</param>
        /// <returns>最大角度。</returns>
        public static float GetMaxAngle(List<TriangleInfo> tinfos)
        {
            float max_angle = 0;
            List<Vector3> normals = new List<Vector3>(tinfos.Count);
            for (int i = 0; i < tinfos.Count; ++i)
            {
                normals.Add(tinfos[i].Normal);
            }
            for (int i = 0; i < normals.Count; ++i)
            {
                Vector3 vi = normals[i];
                for (int j = i + 1; j < normals.Count; ++j)
                {
                    Vector3 vj = normals[j];
                    Vector3 cross = Vector3.Cross(vi, vj);
                    float angle = Mathf.Asin(cross.magnitude) * Mathf.Rad2Deg;
                    float dot = Vector3.Dot(vi, vj);
                    if (dot < 0)
                    {
                        angle = 180 - angle;
                    }
                    max_angle = Mathf.Max(max_angle, angle);
                }
            }
            return max_angle;
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="v1">顺时针顺序分布的第一个点。</param>
        /// <param name="v2">顺时针顺序分布的第二个点。</param>
        /// <param name="v3">顺时针顺序分布的第三个点。</param>
        public TriangleInfo(VertexInfo v1, VertexInfo v2, VertexInfo v3)
        {
            //顶点
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;

            //法线 面积
            m_Normal = GeometryUtil.GetNormal(Vertex1.Position, Vertex2.Position, Vertex3.Position);
            m_Area = GeometryUtil.GetArea(Vertex1.Position, Vertex2.Position, Vertex3.Position);

            //最大角度
            float a1 = GeometryUtil.GetAngle((Vertex3.Position - Vertex1.Position).normalized, (Vertex2.Position - Vertex1.Position).normalized, m_Normal);
            float a2 = GeometryUtil.GetAngle((Vertex1.Position - Vertex2.Position).normalized, (Vertex3.Position - Vertex2.Position).normalized, m_Normal);
            float a3 = GeometryUtil.GetAngle((Vertex2.Position - Vertex3.Position).normalized, (Vertex1.Position - Vertex3.Position).normalized, m_Normal);
            m_MaxAngleVertex = 1;
            m_MaxAngle = a1;
            if (a2 > m_MaxAngle)
            {
                m_MaxAngle = a2;
                m_MaxAngleVertex = 2;
            }
            if (a3 > m_MaxAngle)
            {
                m_MaxAngle = a3;
                m_MaxAngleVertex = 3;
            }
        }

        /// <summary>
        /// 判断是否包含某个顶点。
        /// </summary>
        /// <param name="vinfo">顶点信息。</param>
        /// <returns>是否包含某个顶点。</returns>
        public bool ContainsVertex(VertexInfo vinfo)
        {
            return Vertex1 == vinfo || Vertex2 == vinfo || Vertex3 == vinfo;
        }

        /// <summary>
        /// 顺时针顺序分布的第一个点。
        /// </summary>
        public VertexInfo Vertex1 { get; private set; }

        /// <summary>
        /// 顺时针顺序分布的第二个点。
        /// </summary>
        public VertexInfo Vertex2 { get; private set; }

        /// <summary>
        /// 顺时针顺序分布的第三个点。
        /// </summary>
        public VertexInfo Vertex3 { get; private set; }

        /// <summary>
        /// 获取三角形的朝向。
        /// </summary>
        public Vector3 Normal
        {
            get
            { 
                return m_Normal;
            }
        }

        /// <summary>
        /// 获取最大夹角。
        /// </summary>
        public float MaxAngle
        {
            get
            {
                return m_MaxAngle;
            }
        }

        /// <summary>
        /// 获取最大角度顶点。
        /// </summary>
        public int MaxAngleVertex
        {
            get
            {
                return m_MaxAngleVertex;
            }
        }

        /// <summary>
        /// 获取三角形面积。
        /// </summary>
        public float Area
        {
            get { return m_Area; }
        }

        /// <summary>
        /// 朝向。
        /// </summary>
        private Vector3 m_Normal = Vector3.zero;

        /// <summary>
        /// 三角形面积。
        /// </summary>
        private float m_Area = 1;

        /// <summary>
        /// 最大角。
        /// </summary>
        private float m_MaxAngle = 0;

        /// <summary>
        /// 最大角度的顶点。
        /// </summary>
        private int m_MaxAngleVertex = 1;
    }

    /// <summary>
    /// 网格信息。
    /// </summary>
    public class MeshInfo
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 判断某个顶点是否为边界点。
        /// </summary>
        /// <param name="vinfo">顶点信息。</param>
        /// <returns>是否为边界点。</returns>
        public static bool IsBoundaryPoint(VertexInfo vinfo)
        {
            Vector2 uv = vinfo.UV;
            //return (uv.x == 0 || uv.x == 1) && (uv.y == 0 || uv.y == 1);
            return (uv.x == 0 || uv.x == 1 || uv.y == 0 || uv.y == 1);
        }

        /// <summary>
        /// 获取填满某个多边形区域的三角形列表。
        /// </summary>
        /// <param name="vinfos">顺时针排布的顶点列表。</param>
        /// <param name="dir">多边形大致朝向。</param>
        /// <returns>三角形列表。</returns>
        public static List<TriangleInfo> GetTriangles(List<VertexInfo> vinfos, Vector3 dir)
        {
            List<TriangleInfo> tinfos = new List<TriangleInfo>(vinfos.Count - 2);
            if (vinfos.Count == 3)
            {
                //三个顶点之间生成
                TriangleInfo tinfo = new TriangleInfo(vinfos[0], vinfos[1], vinfos[2]);
                tinfos.Add(tinfo);
                return tinfos;
            }

            //获取最大内角的顶点。
            int index = 0;
            float max_angle = 0;
            for (int i = 0; i < vinfos.Count; ++i)
            {
                int si = (i + vinfos.Count - 1) % vinfos.Count;
                int ei = (i + 1) % vinfos.Count;
                Vector3 from = (vinfos[si].Position - vinfos[i].Position).normalized;
                Vector3 to = (vinfos[ei].Position - vinfos[i].Position).normalized;
                float angle = GeometryUtil.GetAngle(from, to, dir);
                if (angle > max_angle)
                {
                    max_angle = angle;
                    index = i;
                }
            }

            //4个顶点从最大的角分开往对角拉线
            if (vinfos.Count == 4)
            {
                TriangleInfo tinfo = new TriangleInfo(vinfos[index], vinfos[(index + 1) % vinfos.Count], vinfos[(index + 2) % vinfos.Count]);
                tinfos.Add(tinfo);
                tinfo = new TriangleInfo(vinfos[(index + 2) % vinfos.Count], vinfos[(index + 3) % vinfos.Count], vinfos[index]);
                tinfos.Add(tinfo);
                return tinfos;
            }

            //根据最大内角确定生成方式
            if (max_angle < 180)
            {
                //凸多边形，直接按最小角进行边三角形构建
                while (vinfos.Count >= 3)
                {
                    float min_angle = 180;
                    index = 0;
                    for (int ii = 0; ii < vinfos.Count; ++ii)
                    {
                        int i = ii % vinfos.Count;
                        int si = (i == 0 ? vinfos.Count : i) - 1;
                        int ei = (i + 1) % vinfos.Count;
                        Vector3 from = (vinfos[si].Position - vinfos[i].Position).normalized;
                        Vector3 to = (vinfos[ei].Position - vinfos[i].Position).normalized;
                        float angle = GeometryUtil.GetAngle(from, to, dir);
                        if (angle < min_angle)
                        {
                            min_angle = angle;
                            index = i;
                        }
                    }

                    //构建一个边缘三角并移除对应顶点
                    int tsi = (index == 0 ? vinfos.Count : index) - 1;
                    int tei = (index + 1) % vinfos.Count;
                    TriangleInfo tinfo = new TriangleInfo(vinfos[tsi], vinfos[index], vinfos[tei]);
                    tinfos.Add(tinfo);
                    vinfos.RemoveAt(index);
                }
            }
            else
            {
                //凹多边形，需要切割后递归
                List<VertexInfo> vinfos_a = new List<VertexInfo>();
                List<VertexInfo> vinfos_b = new List<VertexInfo>();
                int hn = vinfos.Count / 2;
                int to_index = index + hn;

                //index->toinex作为分割线，要确保分割后所有顶点都在同一侧，且分割后两边的顶点数量尽量一致
                Vector4 plane = GeometryUtil.GetPlane(dir, vinfos[index].Position);
                List<Vector3> projections = new List<Vector3>(vinfos.Count);
                for (int i = 0; i < vinfos.Count; ++i)
                {
                    projections.Add(GeometryUtil.GetProjection(vinfos[i].Position, plane));
                }
                for (int i = 1; i <= hn; ++i)
                {
                    int left = to_index - i;
                    if (left <= index + 2)
                    {
                        to_index = Math.Max(index + 2, left);
                        break;
                    }

                    bool ok = true;
                    Vector3 a = projections[left % projections.Count] - vinfos[index].Position;
                    for (int j = index + 1; j <= left; ++j)
                    {
                        Vector3 b = projections[j % projections.Count] - vinfos[index].Position;
                        float angle = GeometryUtil.GetAngle(a, b, dir);
                        if (angle >= 180)
                        {
                            ok = false;
                        }
                    }
                    if (ok)
                    {
                        to_index = left;
                        break;
                    }

                    int right = to_index + i;
                    a = projections[right % projections.Count] - vinfos[index].Position;
                    ok = true;
                    for (int j = i; j <= hn - 1; ++j)
                    {
                        Vector3 b = projections[(right + j) % projections.Count] - vinfos[index].Position;
                        float angle = GeometryUtil.GetAngle(b, a, dir);
                        if (angle >= 180)
                        {
                            ok = false;
                        }
                    }
                    if (ok)
                    {
                        to_index = right;
                        break;
                    }
                }

                for (int i = index; i <= to_index; ++i)
                {
                    vinfos_a.Add(vinfos[i % vinfos.Count]);
                }
                for (int i = 0; i < vinfos.Count; ++i)
                {
                    int ii = (to_index + i) % vinfos.Count;
                    vinfos_b.Add(vinfos[ii]);
                    if (ii == index)
                    {
                        break;
                    }
                }
                tinfos.AddRange(GetTriangles(vinfos_a, dir));
                tinfos.AddRange(GetTriangles(vinfos_b, dir));
            }

            return tinfos;
        }

        /// <summary>
        /// 创建网格信息。
        /// </summary>
        /// <param name="mesh">网格对象。</param>
        public static MeshInfo Create(Mesh mesh)
        {
            //生成网格信息。
            MeshInfo mesh_info = new MeshInfo();
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;
            mesh_info.m_VertexInfos.Capacity = vertices.Length;
            for (int i = 0; i < vertices.Length; ++i)
            {
                VertexInfo vinfo = new VertexInfo();
                vinfo.Index = i;
                vinfo.Position = vertices[i];
                vinfo.Normal = normals[i];
                vinfo.UV = uvs[i];
                mesh_info.m_VertexInfos.Add(vinfo);
            }

            //三角形数据
            int[] triangles = mesh.triangles;
            int triangle_number = triangles.Length / 3;
            mesh_info.m_TriangleInfos.Capacity = triangle_number;
            for (int i = 0; i < triangle_number; ++i)
            {
                int index = i * 3;
                VertexInfo v1 = mesh_info.m_VertexInfos[triangles[index]];
                VertexInfo v2 = mesh_info.m_VertexInfos[triangles[index + 1]];
                VertexInfo v3 = mesh_info.m_VertexInfos[triangles[index + 2]];
                TriangleInfo tinfo = new TriangleInfo(v1, v2, v3);
                mesh_info.m_TriangleInfos.Add(tinfo);
            }

            //顶点数据刷新
            foreach (var tmp in mesh_info.m_VertexInfos)
            {
                tmp.IsBoundary = IsBoundaryPoint(tmp);
                tmp.UpdateMaxAngle(mesh_info);
            }
            return mesh_info;
        }

        /// <summary>
        /// 创建网格。
        /// </summary>
        /// <param name="mesh">要保存的网格，null则创建个新的。</param>
        /// <returns>网格对象。</returns>
        public Mesh GenerateMesh(Mesh mesh = null)
        {
            if (m_VertexInfos.Count < 3 || m_TriangleInfos.Count < 1)
            {
                return null;
            }

            //顶点数据
            Vector3[] vertices = new Vector3[m_VertexInfos.Count];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < m_VertexInfos.Count; ++i)
            {
                var vinfo = m_VertexInfos[i];
                vertices[i] = vinfo.Position;
                normals[i] = vinfo.Normal;
                uvs[i] = vinfo.UV;
            }

            //三角形数据
            int[] triangles = new int[m_TriangleInfos.Count * 3];
            for (int i = 0; i < m_TriangleInfos.Count; ++i)
            {
                int vindex = i * 3;
                var tinfo = m_TriangleInfos[i];
                triangles[vindex] = tinfo.Vertex1.Index;
                triangles[vindex + 1] = tinfo.Vertex2.Index;
                triangles[vindex + 2] = tinfo.Vertex3.Index;
            }

            //生成网格
            if (mesh == null)
            {
                mesh = new Mesh();
            }
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            return mesh;
        }

        /// <summary>
        /// 获取围绕着某个顶点的其它顶点列表。
        /// </summary>
        /// <param name="vinfo">顶点信息。</param>
        /// <param name="tinfos">顶点相关联的三角形。</param>
        /// <returns>逆时针围绕的顶点列表。</returns>
        public static List<VertexInfo> GetRingVertexInfos(VertexInfo vinfo, List<TriangleInfo> tinfos)
        {
            List<VertexInfo> vinfos = new List<VertexInfo>();
            List<TriangleInfo> tmps = new List<TriangleInfo>();
            tmps.AddRange(tinfos);            
            while (tinfos.Count > 0)
            {
                //找到一个与头或尾相连的三角形
                VertexInfo head = vinfos.Count == 0 ? null : vinfos[0];
                VertexInfo tail = vinfos.Count == 0 ? null : vinfos[vinfos.Count - 1];
                int index = 0;
                if (vinfos.Count > 0)
                {
                    for (; index < tinfos.Count; ++index)
                    {
                        TriangleInfo tmp = tinfos[index];
                        if (tmp.ContainsVertex(head) || tmp.ContainsVertex(tail))
                        {
                            break;
                        }
                    }
                }

                //获取三角形另外的两点AB
                TriangleInfo tinfo = tinfos[index];
                VertexInfo a = null, b = null;
                tinfos.RemoveAt(index);
                if (tinfo.Vertex1 == vinfo)
                {
                    a = tinfo.Vertex2;
                    b = tinfo.Vertex3;
                }
                else if (tinfo.Vertex2 == vinfo)
                {
                    a = tinfo.Vertex3;
                    b = tinfo.Vertex1;
                }
                else if (tinfo.Vertex3 == vinfo)
                {
                    a = tinfo.Vertex1;
                    b = tinfo.Vertex2;
                }

                //根据与首或尾相连添加到列表中
                if (head == null)
                {
                    vinfos.Add(a);
                    vinfos.Add(b);
                }
                else if (b == head)
                {
                    if (!vinfos.Contains(a))
                    {
                        vinfos.Insert(0, a);
                    }
                }
                else if (a == tail)
                {
                    if (!vinfos.Contains(b))
                    {
                        vinfos.Add(b);
                    }
                }
            }
            return vinfos;
        }

        /// <summary>
        /// 查找包含某个顶点的三角形。
        /// </summary>
        /// <param name="vinfo">顶点信息。</param>
        /// <returns>三角形列表。</returns>
        public List<TriangleInfo> FindTriangle(VertexInfo vinfo)
        {
            List<TriangleInfo> tinfos = new List<TriangleInfo>();
            foreach (var tinfo in m_TriangleInfos)
            {
                if (tinfo.ContainsVertex(vinfo))
                {
                    tinfos.Add(tinfo);
                }
            }

            return tinfos;
        }

        /// <summary>
        /// 移除三角形到一定数量。
        /// </summary>
        /// <param name="target">目标数量。</param>
        /// <param name="angle">顶点移除判定的夹角。（所有三角形面夹角都小于此夹角时将被移除）</param>
        public void RemoveTriangle(int target, float angle = 5)
        {
            int old_vcount = m_VertexInfos.Count;
            while (m_TriangleInfos.Count > 0)
            {
                var vindex = FindFlattestVertex();
                if (vindex == -1)
                {
                    break;
                }

                var vinfo = m_VertexInfos[vindex];
                if (vinfo.MaxAngle > angle)
                {
                    break;
                }
                RemoveVertexInfo(vindex);
            }
            while (m_TriangleInfos.Count > target)
            {
                var vindex = FindFlattestVertex();
                if (vindex == -1)
                {
                    break;
                }
                RemoveVertexInfo(vindex);
            }

            //顶点数量发生变化，需要刷新索引
            if (m_VertexInfos.Count != old_vcount)
            {
                for (int i = 0; i < m_VertexInfos.Count; ++i)
                {
                    var vinfo = m_VertexInfos[i];
                    vinfo.Index = i;
                }
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取顶点数量。
        /// </summary>
        public int VertexCount
        {
            get { return m_VertexInfos.Count; }
        }

        /// <summary>
        /// 获取三角形数量。
        /// </summary>
        public int TriangleCount
        {
            get { return m_TriangleInfos.Count; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 查找最平坦的顶点。
        /// </summary>
        /// <param name="tinfos">与其关联的三角形信息。</param>
        /// <returns>顶点再数组内的索引。(不一定等于自身Index，优化过程会移除顶点，但Index在优化完才修正)</returns>
        private int FindFlattestVertex()
        {
            int ret = -1;
            float min_angle = 180;
            for (int i=0; i<m_VertexInfos.Count; ++i)
            {
                var vinfo = m_VertexInfos[i];
                if (IsBoundaryPoint(vinfo))
                {
                    continue;
                }

                //计算最大夹角
                float angle = vinfo.MaxAngle;
                if (angle < min_angle)
                {
                    ret = i;
                    min_angle = angle;
                    if (min_angle < 0.001f)
                    {
                        //够小了，不用再继续找了
                        break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 移除顶点。
        /// </summary>
        /// <param name="index">顶点索引。</param>
        private void RemoveVertexInfo(int index)
        {
            var vinfo = m_VertexInfos[index];
            var tinfos = FindTriangle(vinfo);
            var vinfos = GetRingVertexInfos(vinfo, tinfos);
            var new_tinfos = GetTriangles(vinfos, vinfo.Normal);
            m_VertexInfos.RemoveAt(index);
            for (int i = m_TriangleInfos.Count - 1; i >= 0; --i)
            {
                var tinfo = m_TriangleInfos[i];
                if (tinfo.ContainsVertex(vinfo))
                {
                    m_TriangleInfos.RemoveAt(i);
                }
            }
            foreach (var tmp in vinfos)
            {
                tmp.UpdateMaxAngle(this);
            }
            m_TriangleInfos.AddRange(new_tinfos);
        }

        #endregion

        #region 数据成员----------------------------------------------------------------

        /// <summary>
        /// 顶点信息。
        /// </summary>
        private List<VertexInfo> m_VertexInfos = new List<VertexInfo>();

        /// <summary>
        /// 三角形信息。
        /// </summary>
        private List<TriangleInfo> m_TriangleInfos = new List<TriangleInfo>();

        #endregion
    }
}