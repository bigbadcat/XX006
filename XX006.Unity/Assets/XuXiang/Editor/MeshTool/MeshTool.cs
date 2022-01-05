using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 网格工具。
    /// </summary>
    public static class MeshTool
    {
        /// <summary>
        /// 生成草网格。
        /// </summary>
        /// <param name="split">水平分的格子数。</param>
        /// <param name="root">是否带草根。</param>
        /// <param name="path">网格保存路径。</param>
        public static void BuildGrassMesh(int split, bool root, string path)
        {
            var start = DateTime.Now;            
            int vcount = 2 + split * 2;
            Vector3[] vertices = new Vector3[vcount];
            Vector2[] uv = new Vector2[vcount];
            Vector3[] normals = new Vector3[vcount];
            Vector4[] tangents = new Vector4[vcount];
            int[] triangles = new int[split * 2 * 3];
            for (int i = 0; i < vcount; ++i)
            {
                normals[i] = new Vector3(0, 0, -1);
                tangents[i] = new Vector4(1, 0, 0, -1);
            }

            float sh = 1.0f / split;
            for (int i = 0; i <= split; ++i)
            {
                float y = i * sh;
                Vector3 left = new Vector3(-0.5f, y, 0);
                Vector3 right = new Vector3(0.5f, y, 0);
                vertices[i * 2] = left;
                vertices[i * 2 + 1] = right;
                uv[i * 2] = new Vector2(0, y);
                uv[i * 2 + 1] = new Vector2(1, y);
            }
            for (int i = 0; i < split; ++i)
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

            Mesh mesh = new Mesh();
            if (root)
            {
                Vector3[] r_vertices = new Vector3[vcount + 4];
                Vector2[] r_uv = new Vector2[vcount + 4];
                Vector3[] r_normals = new Vector3[vcount + 4];
                Vector4[] r_tangents = new Vector4[vcount + 4];
                int[] r_triangles = new int[triangles.Length + 12];

                for (int i = 0; i < vcount; ++i)
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

                for (int i = 0; i < 4; ++i)
                {
                    r_normals[i] = new Vector3(0, 0, -1);
                    r_tangents[i] = new Vector4(1, 0, 0, -1);
                }

                for (int i = 0; i < triangles.Length; ++i)
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
            }
            else
            {
                mesh.vertices = vertices;
                mesh.uv = uv;
                mesh.normals = normals;
                mesh.tangents = tangents;
                mesh.triangles = triangles;
            }

            SaveMeshToAsset(mesh, path);
            Log.Info("Build terrain mesh finished in {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 通过地形创建网格信息。
        /// </summary>
        /// <param name="tdata">地形数据。</param>
        /// <param name="grid_x">水平格子数。</param>
        /// <param name="grid_y">竖直格子数。</param>
        /// <param name="path">要保存的路径。</param>
        /// <returns>地形网格信息。</returns>
        public static void BuildTerrainMesh(TerrainData tdata, int grid_x, int grid_y, string path)
        {
            var start = DateTime.Now;
            int row = grid_y + 1;
            int col = grid_x + 1;
            Vector3[] vertices = new Vector3[row * col];
            Vector2[] uvs = new Vector2[vertices.Length];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector3 size = tdata.size;
            for (int y = 0; y < row; ++y)
            {
                int row_index = col * y;
                for (int x = 0; x < col; ++x)
                {
                    int vindex = row_index + x;
                    Vector2 uv = new Vector2(x * 1.0f / grid_x, y * 1.0f / grid_y);
                    vertices[vindex] = new Vector3(uv.x * size.x, tdata.GetInterpolatedHeight(uv.x, uv.y), uv.y * size.z);
                    uvs[vindex] = uv;
                }
            }

            //计算顶点法线
            for (int y = 0; y < row; ++y)
            {
                int row_index = col * y;
                for (int x = 0; x < col; ++x)
                {
                    Vector3 dir = Vector3.zero;
                    int vindex = row_index + x;
                    if (x > 0)
                    {
                        //左边两个格子
                        if (y > 0)
                        {
                            //左下格子
                            dir += GeometryUtil.GetNormal(vertices[vindex], vertices[vindex - col], vertices[vindex - col - 1], vertices[vindex - 1]);
                        }
                        if (y < grid_y)
                        {
                            //左上格子
                            dir += GeometryUtil.GetNormal(vertices[vindex], vertices[vindex - 1], vertices[vindex + col - 1], vertices[vindex + col]);
                        }
                    }
                    if (x < grid_x)
                    {
                        //右边两个格子
                        if (y > 0)
                        {
                            //右下格子
                            dir += GeometryUtil.GetNormal(vertices[vindex], vertices[vindex + 1], vertices[vindex - col + 1], vertices[vindex - col]);
                        }
                        if (y < grid_y)
                        {
                            //右上格子
                            dir += GeometryUtil.GetNormal(vertices[vindex], vertices[vindex + col], vertices[vindex + col + 1], vertices[vindex + 1]);
                        }
                    }

                    normals[vindex] = dir.normalized;
                }
            }

            //生成三角形索引
            int triangle_number = grid_x * grid_y * 2;
            int[] triangles = new int[triangle_number * 3];
            for (int y = 0; y < grid_y; ++y)
            {
                for (int x = 0; x < grid_x; ++x)
                {
                    int tindex = (y * grid_x + x) * 6;
                    int lb = y * col + x;
                    int lt = lb + col;
                    int rt = lt + 1;
                    int rb = lb + 1;
                    triangles[tindex] = lb;
                    triangles[tindex + 1] = lt;
                    triangles[tindex + 2] = rt;
                    triangles[tindex + 3] = rt;
                    triangles[tindex + 4] = rb;
                    triangles[tindex + 5] = lb;
                }
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            SaveMeshToAsset(mesh, path);
            Log.Info("Build terrain mesh finished in {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 移除三角形到一定数量。
        /// </summary>
        /// <param name="target">目标数量。</param>
        /// <param name="angle">顶点移除判定的夹角。（所有三角形面夹角都小于此夹角时将被移除）</param>
        public static void OptimizeTerrainMesh(Mesh mesh, int target, float angle, string path)
        {
            var start = DateTime.Now;
            MeshInfo minfo = MeshInfo.Create(mesh);
            minfo.RemoveTriangle(target, angle);
            SaveMeshToAsset(minfo.GenerateMesh(), path);
            Log.Info("Optimize terrain mesh finished in {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 保存网格到资产。
        /// </summary>
        /// <param name="mesh">网格对象。(不能是asset自身)</param>
        /// <param name="path">资产路径。(不带asset后缀)</param>
        private static void SaveMeshToAsset(Mesh mesh, string path)
        {
            string asset_path = path + ".asset";
            Mesh to_mesh = AssetDatabase.LoadAssetAtPath<Mesh>(asset_path);
            if (to_mesh == null)
            {
                AssetDatabase.CreateAsset(mesh, asset_path);
            }
            else
            {
                to_mesh.Clear();
                to_mesh.vertices = mesh.vertices;
                to_mesh.normals = mesh.normals;
                to_mesh.uv = mesh.uv;
                to_mesh.triangles = mesh.triangles;
                AssetDatabase.SaveAssets();
            }
        }
    }
}