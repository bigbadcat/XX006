using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace XuXiang.EditorTools
{
    /// <summary>
    /// 自动调用TexturePacker打包图片并导入到Unity中。
    /// 要求tps文件名、图集文件夹名、导出的png文件名和xml文件名一样。
    /// </summary>
    public class AtlasPackerWindow : EditorWindow
    {
        /// <summary>
        /// 图集信息。
        /// </summary>
        public class AtlasInfo
        {
            /// <summary>
            /// 名称。
            /// </summary>
            public string Name = string.Empty;

            /// <summary>
            /// 贴图数。
            /// </summary>
            public int SpriteCount;

            /// <summary>
            /// 贴图名称列表。
            /// </summary>
            public List<string> TileNames = null;

            /// <summary>
            /// 加载贴图列表。
            /// </summary>
            public void LoadTitleNames(string tile_folder)
            {
                //按png文件的数量来确定精灵数量
                string folder = Path.Combine(tile_folder, Name);
                string[] files = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly);
                SpriteCount = files.Length;
                TileNames = new List<string>(SpriteCount);
                for (int i = 0; i < files.Length; ++i)
                {
                    TileNames.Add(Path.GetFileNameWithoutExtension(files[i]));
                }
                TileNames.Sort();
            }
        }

        #region 对外操作----------------------------------------------------------------

        [MenuItem("Tools/Atlas/Packer...")]
        public static void ShowAtlasPacker()
        {
            EditorWindow.GetWindow(typeof(AtlasPackerWindow), true, "Atlas Packer");
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public AtlasPackerWindow()
        {
            this.minSize = new Vector2(700, 500);
        }

        /// <summary>
        /// 获取UI的预设列表。
        /// </summary>
        /// <returns>预设路径列表。</returns>
        public List<string> GetUIPrefabs()
        {
            List<string> prefabs = new List<string>();
            GetPrefabs(prefabs, "ResourcesEx/UI");
            return prefabs;
        }

        /// <summary>
        /// 获取文件夹下的预算列表。
        /// </summary>
        /// <param name="prefabs"></param>
        /// <param name="folder"></param>
        public void GetPrefabs(List<string> prefabs, string folder)
        {
            string path = Path.Combine(Application.dataPath, folder);
            string[] files = System.IO.Directory.GetFiles(path, "*.prefab", System.IO.SearchOption.AllDirectories);
            int index = path.IndexOf("Assets");
            for (int i = 0; i < files.Length; ++i)
            {
                prefabs.Add(files[i].Substring(index));
            }
        }

        /// <summary>
        /// 将UI的sprite替换。
        /// </summary>
        /// <param name="path">UI所在的prefab路径。</param>
        /// <param name="from">原sprite。</param>
        /// <param name="to">要替换的新sprite。</param>
        /// <returns>是否发生了替换。</returns>
        public bool ReplaceSprite(string path, Sprite from, Sprite to)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            Image[] images = prefab.GetComponentsInChildren<Image>(true);
            bool isneed = false;
            foreach (Image img in images)
            {
                if (img.sprite == from)
                {
                    img.sprite = to;
                    isneed = true;
                }
            }
            if (isneed)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }

            return isneed;
        }

        /// <summary>
        /// 获取图集里的sprite。
        /// </summary>
        /// <param name="atlas">图集名称。</param>
        /// <param name="tile">贴图名称。</param>
        /// <returns>贴图对应的sprite。</returns>
        public static Sprite GetSprite(string atlas, string tile)
        {
            string path = Path.Combine(AtlasFolder, string.Format("{0}.png", atlas));
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in objs)
            {
                Sprite sp = obj as Sprite;
                if (sp != null && string.CompareOrdinal(sp.name, tile) == 0)
                {
                    return sp;
                }
            }
            return null;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 打包程序路径。
        /// </summary>
        public const string PACKER_EXE_PATH = "PACKER_EXE_PATH";

        /// <summary>
        /// 散图资源位置。(相对于Unity项目文件夹)
        /// </summary>
        public static string AtlasTileFolder = "AtlasImages";

        /// <summary>
        /// 图集存放路径。(相对于Unity项目文件夹)
        /// </summary>
        public static string AtlasFolder = "Assets/ResourcesEx/Atlas";

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 窗口启用。
        /// </summary>
        private void OnEnable()
        {
            m_TexturePackerPath = EditorPrefs.GetString(PACKER_EXE_PATH, string.Empty);
            InitAtlasInfo();
        }

        /// <summary>
        /// 窗口禁用。
        /// </summary>
        private void OnDisable()
        {

        }

        /// <summary>
        /// 初始化图集列表。
        /// </summary>
        private void InitAtlasInfo()
        {
            //碎图文件夹路径
            string path = Directory.GetParent(Application.dataPath).FullName;
            string atlas_folder = Path.Combine(path, AtlasTileFolder);
            string[] tps_files = Directory.GetFiles(atlas_folder, "*.tps", SearchOption.TopDirectoryOnly);
            for (int i = 0; i < tps_files.Length; ++i)
            {
                string file = tps_files[i];
                AtlasInfo info = new AtlasInfo();
                info.Name = Path.GetFileNameWithoutExtension(file);
                info.LoadTitleNames(atlas_folder);
                m_AtlasList.Add(info);
            }

            //按名称排序
            m_AtlasList.Sort((a, b) =>
            {
                //return a.Name.CompareTo(b.Name);
                return string.CompareOrdinal(a.Name, b.Name);
            });
        }

        /// <summary>
        /// 界面绘制。
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawPackerPath();
            DrawAtlasList();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// 绘制指定打包程序部分。
        /// </summary>
        private void DrawPackerPath()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("TexturePacker:", GUILayout.ExpandWidth(false));
            string path = GUILayout.TextField(m_TexturePackerPath);
            if (path.CompareTo(m_TexturePackerPath) != 0)
            {
                m_TexturePackerPath = path;
                EditorPrefs.SetString(PACKER_EXE_PATH, m_TexturePackerPath);
            }
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 绘制图集列表。
        /// </summary>
        private void DrawAtlasList()
        {
            GUILayout.BeginHorizontal("Toolbar");
            GUILayout.Label("AtlasName", GUILayout.Width(230));
            GUILayout.Label("SpriteCount", GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            m_WantOperate = EditorGUILayout.ObjectField(m_WantOperate, typeof(Texture), false) as Texture;
            GUILayout.EndHorizontal();
            m_SceneScroll = GUILayout.BeginScrollView(m_SceneScroll);
            for (int i = 0; i < m_AtlasList.Count; ++i)
            {
                AtlasInfo info = m_AtlasList[i];
                if (i % 2 == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal("IN BigTitle Inner");
                }
                bool expand = m_ExpandIndex == i;
                if (GUILayout.Button(expand ? "-" : "+", GUILayout.Width(20)))
                {
                    m_ExpandIndex = expand ? -1 : i;
                }
                GUILayout.Label(info.Name, GUILayout.Width(230));
                GUILayout.Label(info.SpriteCount.ToString(), GUILayout.Width(80));
                GUILayout.FlexibleSpace();
                if (m_WantOperate != null && GUILayout.Button("Add", GUILayout.MaxWidth(60)))
                {
                    m_AtlasIndex = i;
                    EditorApplication.update += DoAddTile;
                }
                if (GUILayout.Button("Pack", GUILayout.MaxWidth(60)))
                {
                    m_AtlasIndex = i;
                    EditorApplication.update += DoPackAtlas;
                }
                GUILayout.EndHorizontal();

                if (expand)
                {
                    GUILayout.BeginVertical();
                    for (int j = 0; j < info.TileNames.Count; ++j)
                    {
                        EditorGUILayout.BeginHorizontal("GameViewBackground");
                        GUILayout.Space(30);
                        GUILayout.Label(info.TileNames[j]);
                        if (m_WantOperate != null && GUILayout.Button("Replace", GUILayout.MaxWidth(60)))
                        {
                            m_TileName = info.TileNames[j];
                            m_AtlasIndex = i;
                            EditorApplication.update += DoReplaceTile;
                        }
                        if (GUILayout.Button("Delete", GUILayout.MaxWidth(60)))
                        {
                            m_TileName = info.TileNames[j];
                            m_AtlasIndex = i;
                            EditorApplication.update += DoDeleteTile;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 进行图集打包操作。
        /// </summary>
        private void DoPackAtlas()
        {
            EditorApplication.update -= DoPackAtlas;
            if (m_AtlasIndex < 0 || m_AtlasIndex >= m_AtlasList.Count)
            {
                return;
            }

            if (string.IsNullOrEmpty(m_TexturePackerPath))
            {
                UnityEngine.Debug.LogErrorFormat("Need set TexturePacker path.");
                return;
            }
            if (!File.Exists(m_TexturePackerPath))
            {
                UnityEngine.Debug.LogErrorFormat("TexturePacker is not found! path:{0}", m_TexturePackerPath);
                return;
            }

            AtlasInfo info = m_AtlasList[m_AtlasIndex];
            PackAtlas(m_TexturePackerPath, info.Name);
            m_AtlasIndex = -1;
        }

        /// <summary>
        /// 进行贴图删除操作。
        /// </summary>
        private void DoDeleteTile()
        {
            EditorApplication.update -= DoDeleteTile;
            if (m_AtlasIndex < 0 || m_AtlasIndex >= m_AtlasList.Count || string.IsNullOrEmpty(m_TileName))
            {
                return;
            }

            AtlasInfo info = m_AtlasList[m_AtlasIndex];
            string path = Directory.GetParent(Application.dataPath).FullName;
            string title_file = Path.Combine(path, AtlasTileFolder, info.Name, string.Format("{0}.png", m_TileName));
            info.TileNames.Remove(m_TileName);
            info.SpriteCount = info.TileNames.Count;
            File.Delete(title_file);
            UnityEngine.Debug.LogFormat("Delete Atlas:{0} Tile:{1}", info.Name, m_TileName);
            m_AtlasIndex = -1;
            m_TileName = string.Empty;
        }

        /// <summary>
        /// 进行贴图替换操作。
        /// </summary>
        private void DoReplaceTile()
        {
            EditorApplication.update -= DoReplaceTile;
            if (m_AtlasIndex < 0 || m_AtlasIndex >= m_AtlasList.Count || string.IsNullOrEmpty(m_TileName))
            {
                return;
            }

            AtlasInfo info = m_AtlasList[m_AtlasIndex];
            if (m_WantOperate == null)
            {
                return;
            }
            string texpath = AssetDatabase.GetAssetPath(m_WantOperate);
            TextureImporter textureImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
            if (textureImporter == null || textureImporter.textureType != TextureImporterType.Sprite || textureImporter.spriteImportMode != SpriteImportMode.Single)
            {
                UnityEngine.Debug.LogErrorFormat("Need select Single Sprite.");
                return;
            }

            //sprite替换
            List<string> prefabs = GetUIPrefabs();
            int count = 0;
            Sprite from = GetSprite(info.Name, m_TileName);
            Sprite to = AssetDatabase.LoadAssetAtPath<Sprite>(texpath);
            foreach (string path in prefabs)
            {
                if (ReplaceSprite(path, from, to))
                {
                    ++count;
                }
            }

            AssetDatabase.Refresh();
            UnityEngine.Debug.LogFormat("Replace Atlas:{0} Tile:{1} Modify:{2}", info.Name, m_TileName, count);
            m_AtlasIndex = -1;
            m_TileName = string.Empty;
        }

        /// <summary>
        /// 进行贴图添加操作。
        /// </summary>
        private void DoAddTile()
        {
            EditorApplication.update -= DoAddTile;
            if (m_AtlasIndex < 0 || m_AtlasIndex >= m_AtlasList.Count)
            {
                return;
            }

            AtlasInfo info = m_AtlasList[m_AtlasIndex];
            if (m_WantOperate == null)
            {
                return;
            }
            string texpath = AssetDatabase.GetAssetPath(m_WantOperate);
            TextureImporter textureImporter = AssetImporter.GetAtPath(texpath) as TextureImporter;
            if (textureImporter == null || textureImporter.textureType != TextureImporterType.Sprite || textureImporter.spriteImportMode != SpriteImportMode.Single)
            {
                UnityEngine.Debug.LogErrorFormat("Need select Single Sprite.");
                return;
            }

            //将png拷贝到图集文件夹，重新打图集
            string path = Directory.GetParent(Application.dataPath).FullName;
            string tile_folder = Path.Combine(path, AtlasTileFolder);
            string atlas_folder = Path.Combine(tile_folder, info.Name);
            string png_file = Path.Combine(path, texpath);
            string to_file = Path.Combine(atlas_folder, m_WantOperate.name + ".png");
            if (!File.Exists(to_file))
            {
                //添加进来重新打包
                File.Copy(png_file, to_file);
                info.LoadTitleNames(tile_folder);
                PackAtlas(m_TexturePackerPath, info.Name);
            }
            
            //sprite替换
            List<string> prefabs = GetUIPrefabs();
            int count = 0;
            Sprite from = AssetDatabase.LoadAssetAtPath<Sprite>(texpath);
            Sprite to = GetSprite(info.Name, m_WantOperate.name);
            foreach (string ui in prefabs)
            {
                if (ReplaceSprite(ui, from, to))
                {
                    ++count;
                }
            }

            AssetDatabase.Refresh();
            UnityEngine.Debug.LogFormat("Add Atlas:{0} Tile:{1} Modify:{2}", info.Name, m_WantOperate.name, count);
            m_AtlasIndex = -1;
        }

        /// <summary>
        /// 打包图集。
        /// </summary>
        /// <param name="exe">打包程序路径。</param>
        /// <param name="name">要打包的图集名称。</param>
        private static void PackAtlas(string exe, string name)
        {
            //先打包
            DateTime start = DateTime.Now;
            EditorUtility.DisplayProgressBar("PackAtlas", "Packing...", 0);
            string path = Directory.GetParent(Application.dataPath).FullName;
            string atlas_folder = Path.Combine(path, AtlasTileFolder);
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = exe;
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.Arguments = Path.Combine(atlas_folder, string.Format("{0}.tps", name));
            Process p = Process.Start(info);
            p.WaitForExit();
            AssetDatabase.Refresh();

            //再导入
            EditorUtility.DisplayProgressBar("PackAtlas", "Importing...", 0.5f);

            string atlas_asset = Path.Combine(AtlasFolder, string.Format("{0}.png", name));
            AtlasImporter.ImportAtlasAsset(atlas_asset);
            EditorUtility.ClearProgressBar();
            UnityEngine.Debug.LogFormat("PackAtlas finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 打包程序路径。
        /// </summary>
        private string m_TexturePackerPath = string.Empty;

        /// <summary>
        /// 图集列表。
        /// </summary>
        private List<AtlasInfo> m_AtlasList = new List<AtlasInfo>();

        /// <summary>
        /// 当前要操作的图集索引。
        /// </summary>
        private int m_AtlasIndex = -1;

        /// <summary>
        /// 当前要操作的贴图名称。
        /// </summary>
        private string m_TileName = string.Empty;

        /// <summary>
        /// 需要操作的图片。
        /// </summary>
        public Texture m_WantOperate = null;

        /// <summary>
        /// 当前展开索引。
        /// </summary>
        private int m_ExpandIndex = -1;

        /// <summary>
        /// 列表滚动位置。
        /// </summary>
        private Vector2 m_SceneScroll;

        #endregion
    }
}