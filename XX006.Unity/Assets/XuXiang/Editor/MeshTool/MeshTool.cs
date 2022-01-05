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
        /// 通过地形创建网格信息。
        /// </summary>
        /// <param name="tdata">地形数据。</param>
        /// <param name="grid_x">水平格子数。</param>
        /// <param name="grid_y">竖直格子数。</param>
        /// <param name="path">要保存的路径。</param>
        /// <returns>地形网格信息。</returns>
        public static void CreateTerrainMesh(TerrainData tdata, int grid_x, int grid_y, string path)
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
            SaveMeshToAsset(mesh, path + ".asset");
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
            SaveMeshToAsset(minfo.GenerateMesh(), path + ".asset");
            Log.Info("Optimize terrain mesh finished in {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 保存网格到资产。
        /// </summary>
        /// <param name="mesh">网格对象。(不能是asset自身)</param>
        /// <param name="path">资产路径。</param>
        private static void SaveMeshToAsset(Mesh mesh, string path)
        {
            Mesh to_mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (to_mesh == null)
            {
                AssetDatabase.CreateAsset(mesh, path);
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