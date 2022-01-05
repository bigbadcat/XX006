using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 网格工具窗口。
    /// </summary>
    public class MeshToolWindow : EditorWindow
    {
        #region 对外操作----------------------------------------------------------------

        [MenuItem("Tools/MeshTool...")]
        public static void ShowAtlasPacker()
        {
            EditorWindow.GetWindow(typeof(MeshToolWindow), true, "Mesh Tool");
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public MeshToolWindow()
        {
            this.minSize = new Vector2(600, 500);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 窗口启用。
        /// </summary>
        private void OnEnable()
        {
            LoadSetting();
        }

        /// <summary>
        /// 窗口禁用。
        /// </summary>
        private void OnDisable()
        {
            SaveSetting();
        }

        /// <summary>
        /// 加载设置。
        /// </summary>
        private void LoadSetting()
        {
            m_GrassSplit = EditorPrefs.GetInt("MESH_TOOL_GRASS_SPLIT", m_GrassSplit);
            m_GrassRoot = EditorPrefs.GetBool("MESH_TOOL_GRASS_ROOT", m_GrassRoot);
            m_GrassSavePath = EditorPrefs.GetString("MESH_TOOL_GRASS_SAVE_PATH");

            m_Terrain = EditorUtil.LoadAssetFromPrefs<Terrain>("MESH_TOOL_TERRAIN_PATH");
            m_TerrainGridX = EditorPrefs.GetInt("MESH_TOOL_TERRAIN_GRID_X", m_TerrainGridX);
            m_TerrainGridZ = EditorPrefs.GetInt("MESH_TOOL_TERRAIN_GRID_Z", m_TerrainGridZ);
            m_TerrainMeshSavePath = EditorPrefs.GetString("MESH_TOOL_TERRAIN_SAVE_PATH");

            m_SourceTerrainMesh = EditorUtil.LoadAssetFromPrefs<Mesh>("MESH_TOOL_OPTIMIZE_SOURCE");
            m_OptimizeAngle = EditorPrefs.GetFloat("MESH_TOOL_OPTIMIZE_ANGLE", m_OptimizeAngle);
            m_OptimizeTargetTriangle = EditorPrefs.GetInt("MESH_TOOL_OPTIMIZE_TARGET_TRIANGLE", m_OptimizeTargetTriangle);
            m_OptimizeMeshSavePath = EditorPrefs.GetString("MESH_TOOL_OPTIMIZE_SAVE_PATH");
        }

        /// <summary>
        /// 保存设置。
        /// </summary>
        private void SaveSetting()
        {
            EditorPrefs.SetInt("MESH_TOOL_GRASS_SPLIT", m_GrassSplit);
            EditorPrefs.SetBool("MESH_TOOL_GRASS_ROOT", m_GrassRoot);
            EditorPrefs.SetString("MESH_TOOL_GRASS_SAVE_PATH", m_GrassSavePath);

            EditorUtil.SaveAssetToPrefs("MESH_TOOL_TERRAIN_PATH", m_Terrain);
            EditorPrefs.SetInt("MESH_TOOL_TERRAIN_GRID_X", m_TerrainGridX);
            EditorPrefs.SetInt("MESH_TOOL_TERRAIN_GRID_Z", m_TerrainGridZ);
            EditorPrefs.SetString("MESH_TOOL_TERRAIN_SAVE_PATH", m_TerrainMeshSavePath);

            EditorUtil.SaveAssetToPrefs("MESH_TOOL_OPTIMIZE_SOURCE", m_SourceTerrainMesh);
            EditorPrefs.SetFloat("MESH_TOOL_OPTIMIZE_ANGLE", m_OptimizeAngle);
            EditorPrefs.SetInt("MESH_TOOL_OPTIMIZE_TARGET_TRIANGLE", m_OptimizeTargetTriangle);
            EditorPrefs.SetString("MESH_TOOL_OPTIMIZE_SAVE_PATH", m_OptimizeMeshSavePath);
        }

        /// <summary>
        /// 界面绘制。
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawGrassMeshBuilder();
            DrawTerrainMeshBuilder();
            DrawTerrainMeshOptimize();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制草网格生成界面。
        /// </summary>
        private void DrawGrassMeshBuilder()
        {
            GUILayout.BeginVertical(GUILayout.MinHeight(100));
            GUILayout.BeginHorizontal("TimeAreaToolbar");
            GUILayout.Label("Grass mesh builder");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            m_GrassSplit = Mathf.Clamp(EditorGUILayout.IntField("Split", m_GrassSplit), 1, 10);
            GUILayout.FlexibleSpace();
            m_GrassRoot = EditorGUILayout.Toggle("Root", m_GrassRoot);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            m_GrassSavePath = EditorGUILayout.TextField("SavePath", m_GrassSavePath);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(m_GrassSavePath));
            if (GUILayout.Button("Build", GUILayout.MaxWidth(100)))
            {
                EditorApplication.update += DoBuildGrassMesh;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 生成草网格。
        /// </summary>
        private void DoBuildGrassMesh()
        {
            EditorApplication.update -= DoBuildGrassMesh;
            if (string.IsNullOrEmpty(m_GrassSavePath))
            {
                return;
            }

            MeshTool.BuildGrassMesh(m_GrassSplit, m_GrassRoot, m_GrassSavePath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 绘制地形网格生成界面。
        /// </summary>
        private void DrawTerrainMeshBuilder()
        {
            GUILayout.BeginVertical(GUILayout.MinHeight(100));
            GUILayout.BeginHorizontal("TimeAreaToolbar");
            GUILayout.Label("Terrain mesh builder");
            GUILayout.EndHorizontal();

            m_Terrain = EditorGUILayout.ObjectField("Terrain", m_Terrain, typeof(Terrain), false) as Terrain;
            GUILayout.BeginHorizontal();            
            m_TerrainGridZ = Mathf.Clamp(EditorGUILayout.IntField("Row", m_TerrainGridZ), 4, 1024);
            GUILayout.FlexibleSpace();
            m_TerrainGridX = Mathf.Clamp(EditorGUILayout.IntField("Col", m_TerrainGridX), 4, 1024);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            m_TerrainMeshSavePath = EditorGUILayout.TextField("SavePath", m_TerrainMeshSavePath);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(m_Terrain == null || string.IsNullOrEmpty(m_TerrainMeshSavePath));
            if (GUILayout.Button("Build", GUILayout.MaxWidth(100)))
            {
                EditorApplication.update += DoBuildTerrainMesh;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 生成地形网格。
        /// </summary>
        private void DoBuildTerrainMesh()
        {
            EditorApplication.update -= DoBuildTerrainMesh;
            if (m_Terrain == null || string.IsNullOrEmpty(m_TerrainMeshSavePath))
            {
                return;
            }

            MeshTool.BuildTerrainMesh(m_Terrain.terrainData, m_TerrainGridX, m_TerrainGridZ, m_TerrainMeshSavePath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 绘制地形网格优化界面。
        /// </summary>
        private void DrawTerrainMeshOptimize()
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal("TimeAreaToolbar");
            GUILayout.Label("Terrain mesh optimize");
            GUILayout.EndHorizontal();

            m_SourceTerrainMesh = EditorGUILayout.ObjectField("Source", m_SourceTerrainMesh, typeof(Mesh), false) as Mesh;
            m_OptimizeAngle = Mathf.Clamp(EditorGUILayout.FloatField("MaxAngle", m_OptimizeAngle), 0, 90);
            m_OptimizeTargetTriangle = Mathf.Clamp(EditorGUILayout.IntField("TargetTriangle", m_OptimizeTargetTriangle), 1, 200000);
            m_OptimizeMeshSavePath = EditorGUILayout.TextField("SavePath", m_OptimizeMeshSavePath);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(m_Terrain == null || string.IsNullOrEmpty(m_OptimizeMeshSavePath));
            if (GUILayout.Button("Optimize", GUILayout.MaxWidth(100)))
            {
                EditorApplication.update += DoOptimizeTerrainMesh;
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 优化地形网格。
        /// </summary>
        private void DoOptimizeTerrainMesh()
        {
            EditorApplication.update -= DoOptimizeTerrainMesh;
            if (m_SourceTerrainMesh == null || string.IsNullOrEmpty(m_OptimizeMeshSavePath))
            {
                return;
            }

            MeshTool.OptimizeTerrainMesh(m_SourceTerrainMesh, m_OptimizeTargetTriangle, m_OptimizeAngle, m_OptimizeMeshSavePath);
            AssetDatabase.Refresh();
        }

        #endregion

        #region 成员变量----------------------------------------------------------------

        /// <summary>
        /// 草要水平分层多少格。
        /// </summary>
        private int m_GrassSplit = 4;

        /// <summary>
        /// 是否带草根。
        /// </summary>
        private bool m_GrassRoot = false;

        /// <summary>
        /// 草网格保存路径。
        /// </summary>
        private string m_GrassSavePath = string.Empty;


        /// <summary>
        /// 地形数据。
        /// </summary>
        private Terrain m_Terrain = null;

        /// <summary>
        /// 地形X轴格子数。
        /// </summary>
        private int m_TerrainGridX = 100;

        /// <summary>
        /// 地形Z轴格子数。
        /// </summary>
        private int m_TerrainGridZ = 100;

        /// <summary>
        /// 地形网格保存位置。
        /// </summary>
        private string m_TerrainMeshSavePath = string.Empty;


        /// <summary>
        /// 要优化的地形网格。
        /// </summary>
        private Mesh m_SourceTerrainMesh = null;

        /// <summary>
        /// 优化角度。
        /// </summary>
        private float m_OptimizeAngle = 5;

        /// <summary>
        /// 优化到的目标三角形数量。
        /// </summary>
        private int m_OptimizeTargetTriangle = 10000;

        /// <summary>
        /// 优化网格保存位置。
        /// </summary>
        private string m_OptimizeMeshSavePath = string.Empty;

        #endregion
    }
}