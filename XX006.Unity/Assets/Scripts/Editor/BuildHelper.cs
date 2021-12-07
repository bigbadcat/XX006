using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;
using XuXiang;
using XuXiang.EditorTools;

namespace XX006.EditorTools
{
    /// <summary>
    /// 资源类型。
    /// </summary>
    public enum AssetType
    {
        /// <summary>
        /// 包体内置。
        /// </summary>
        AppRes,

        /// <summary>
        /// 角色模型动作。
        /// </summary>
        Actor,

        /// <summary>
        /// 特效。
        /// </summary>
        Effect,

        /// <summary>
        /// Lua脚本。
        /// </summary>
        Lua,

        /// <summary>
        /// 场景。
        /// </summary>
        Scene,

        /// <summary>
        /// UI。
        /// </summary>
        UI,

        /// <summary>
        /// 资源数量。
        /// </summary>
        Max,
    }

    /// <summary>
    /// 平台信息。
    /// </summary>
    public class PlatformInfo
    {
        /// <summary>
        /// 平台编号。
        /// </summary>
        public int id;

        /// <summary>
        /// 平台名称。
        /// </summary>
        public string name;

        /// <summary>
        /// 包名。
        /// </summary>
        public string apk_name;

        /// <summary>
        /// 中控制地址。
        /// </summary>
        public string master;

        /// <summary>
        /// 平台更新地址。
        /// </summary>
        public string update_url;

        /// <summary>
        /// 设置值。
        /// </summary>
        /// <param name="line">文本行。</param>
        public void SetValue(string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                return;
            }
            int index = line.IndexOf(':');
            if (index == -1)
            {
                return;
            }
            string key = line.Substring(0, index);
            string value = line.Substring(index + 1);
            SetValue(key, value);
        }

        /// <summary>
        /// 设置值。
        /// </summary>
        /// <param name="key">键名称。</param>
        /// <param name="value">值内容。</param>
        public void SetValue(string key, string value)
        {
            if (key.CompareTo("id")==0)
            {
                id = int.Parse(value);
            }
            else if (key.CompareTo("name") == 0)
            {
                name = value;
            }
            else if (key.CompareTo("apk_name") == 0)
            {
                apk_name = value;
            }
            else if (key.CompareTo("update_url") == 0)
            {
                update_url = value;
            }
            else if (key.CompareTo("master") == 0)
            {
                master = value;
            }
        }
    }

    /// <summary>
    /// 打包操作类。
    /// </summary>
    public static class BuildHelper
    {
        /// <summary>
        /// 分包信息。
        /// </summary>
        public class SplitInfo
        {
            /// <summary>
            /// 分包序号。
            /// </summary>
            public int index;

            /// <summary>
            /// 文件列表。
            /// </summary>
            public List<string> files = new List<string>();

            /// <summary>
            /// 文件夹列表。
            /// </summary>
            public List<string> folders = new List<string>();
        }

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 初始化。
        /// </summary>
        static BuildHelper()
        {
            AssetDivideFunction.Add(AssetType.AppRes, DivideAppResBundle);
            AssetDivideFunction.Add(AssetType.Actor, DivideActorBundle);
            //AssetDivideFunction.Add(AssetType.Effect, DivideEffectBundle);
            //AssetDivideFunction.Add(AssetType.Lua, DivideLuaBundle);
            AssetDivideFunction.Add(AssetType.Scene, DivideSceneBundle);
            AssetDivideFunction.Add(AssetType.UI, DivideUIBundle);
        }

        /// <summary>
        /// 打包资源。
        /// </summary>
        /// <param name="scenes">要打包的场景列表。</param>
        public static void BuildAssetBundle(List<string> scenes)
        {
            //收集要打包的资源信息
            DateTime start = DateTime.Now;
            List<AssetBundleBuild> abbs_list = new List<AssetBundleBuild>();
            EditorUtility.DisplayProgressBar("BuildAssetBundle", "Collect resource info...", 0);
            AddBundleBuild(abbs_list, "ResourcesEx/AppRes", new string[] { PrefabExt, XMLExt }, "AppRes");      //APP基础资源
            AddBundleBuild(abbs_list, "ResourcesEx/Prefabs", new string[] { PrefabExt }, "Prefabs");
            //AddLuaBundleBuild(abbs_list, precomplie);       //Lua资源
            AddUIBundleBuild(abbs_list);                    //UI资源
            AddBundleBuild(abbs_list, "ResourcesEx/Scene", new string[] { SceneExt }, "Scene");      //场景资源
            AddActorBundleBuild(abbs_list);

            //开始打包资源
            string folder = BundleFolder;
            AssetBundleBuild[] abbs = abbs_list.ToArray();
            EditorUtility.DisplayProgressBar("BuildAssetBundle", "Start build asset bundle...", 0);
            EditorUtil.CheckOrCreateFolder(folder);
            BuildPipeline.BuildAssetBundles(BundleOutputFolder, abbs, BuildAssetBundleOptions.DisableWriteTypeTree, EditorUserBuildSettings.activeBuildTarget);            
            EditorUtility.DisplayProgressBar("BuildAssetBundle", "Remove redundant asset bundle...", 0);
            BuildTools.RemoveRedundantAssetBundle(folder);            
            BuildTools.BuildAssetBundleList(folder);

            //更新依赖
            EditorUtility.DisplayProgressBar("BuildAssetBundle", "Update asset bundle dependencies...", 0);
            UpdateAssetBundleDepend();
            EditorUtility.ClearProgressBar();
            Log.Info("Build asset bundle finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 加载分包信息。
        /// </summary>
        public static void LoadSplitInfo()
        {
            CurSplitInfos.Clear();
        }

        /// <summary>
        /// 划分资源。
        /// </summary>
        /// <param name="t">要划分的资源类型。</param>
        /// <param name="split">是否启用分包。</param>
        public static void DivideAsset(AssetType t, bool split)
        {
            //DivideBundle("prefabs", split);

            Action<bool> fun;
            if (AssetDivideFunction.TryGetValue(t, out fun))
            {
                fun(split);
            }
            else
            {
                Log.Error("Divide asset error! Unkown AssetType:{0}", t);
            }
        }

        /// <summary>
        /// 加载平台信息。
        /// </summary>
        /// <returns>平台信息列表。</returns>
        public static List<PlatformInfo> LoadPlatformInfo()
        {
            string path = Path.Combine(Application.dataPath, "../Build/Platform.txt");
            string[] lines = EditorUtil.LoadTextFileLines(path);
            List<PlatformInfo> infos = new List<PlatformInfo>();
            PlatformInfo cur_info = null;
            foreach (string str in lines)
            {
                string line = str.Trim();
                if (line.CompareTo("-Platform") == 0)
                {
                    //保存当前的，创建新的
                    if (cur_info != null)
                    {
                        infos.Add(cur_info);
                    }
                    cur_info = new PlatformInfo();
                }
                else if (cur_info != null)
                {
                    cur_info.SetValue(line);
                }
            }
            if (cur_info != null)
            {
                infos.Add(cur_info);
                cur_info = null;
            }

            return infos;
        }

        /// <summary>
        /// 设置打包版本相关信息。
        /// </summary>
        /// <param name="platform">生成的包平台。</param>
        /// <param name="res_ver">资源版本号。</param>
        public static void SetVersionInfo(PlatformInfo platform, int res_ver)
        {
            //写入Version.txt
            string version_file = Path.Combine(Application.dataPath, "Resources/Version.txt");
            FileStream stream = new FileStream(version_file, FileMode.Create);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write("-ver\n");
            writer.Write(string.Format("Platform:{0}\n", platform.name));
            writer.Write(string.Format("ResourceVersion:{0}\n", res_ver));
            writer.Write(string.Format("Master:{0}\n", platform.master));
            writer.Write(string.Format("UpdateURL:{0}", platform.update_url));
            writer.Flush();
            writer.Close();
            writer.Dispose();
            writer = null;
            stream = null;
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 生成APK包。
        /// </summary>
        /// <param name="platform">生成的包平台。</param>
        /// <param name="res_ver">资源版本号。</param>
        /// <param name="bo">打包选项。</param>
        public static void BuildAPK(PlatformInfo platform, int res_ver, BuildOptions bo)
        {
            var start = DateTime.Now;
            //string version = string.Format("{0}.{1}.{2}", VersionManager.VERSION_MAIN, VersionManager.VERSION_SCRIPT, res_ver);
            //string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //string apk_name = string.Format("{0}_{1}_{2}_{3}.apk", platform.apk_name, platform.name, version, timestamp);
            //Log.Info("APK file - {0}", apk_name);
            //string apk_file = Path.Combine(Directory.GetParent(Application.dataPath).FullName + "/Apk", apk_name);
            //BuildPipeline.BuildPlayer(BuildScenes, apk_file, EditorUserBuildSettings.activeBuildTarget, bo);
            Log.Info("Build apk finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 生成XCode工程。
        /// </summary>
        /// <param name="platform">生成的包平台。</param>
        /// <param name="res_ver">资源版本号。</param>
        /// <param name="bo">打包选项。</param>
        public static void BuildXCode(PlatformInfo platform, int res_ver, BuildOptions bo)
        {
            var start = DateTime.Now;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string xcode_name = string.Format("{0}_{1}_{2}", platform.apk_name, platform.name, timestamp);
            string xcode_folder = Path.Combine(Directory.GetParent(Application.dataPath).Parent.Parent.FullName, xcode_name);
            BuildPipeline.BuildPlayer(BuildScenes, xcode_folder, EditorUserBuildSettings.activeBuildTarget, bo);
            Log.Info("Build Xcode finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 生成资源分包。
        /// </summary>
        /// <param name="platform">生成的包平台。</param>
        /// <param name="res_ver">资源版本号。</param>
        public static void BuildSplit(PlatformInfo platform, int res_ver)
        {
            Log.Info("BuildSplit platform:{0} ResVer:{1}", platform.name, res_ver);
        }

        /// <summary>
        /// 生成更新包。
        /// </summary>
        /// <param name="platform">生成的包平台。</param>
        /// <param name="res_ver">资源版本号。</param>
        public static void BuildUpdate(PlatformInfo platform, int res_ver)
        {
            Log.Info("BuildUpdate platform:{0} ResVer:{1}", platform.name, res_ver);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取打包目录。
        /// </summary>
        public static string BundleFolder
        {
            get
            {
                string platform_folder = EditorUserBuildSettings.activeBuildTarget.ToString();
                string to_folder = string.Format("../Bundle/{0}", platform_folder);
                return Path.Combine(Application.dataPath, to_folder);
            }
        }

        /// <summary>
        /// 获取打包输出目录。
        /// </summary>
        public static string BundleOutputFolder
        {
            get
            {
                string platform_folder = EditorUserBuildSettings.activeBuildTarget.ToString();
                string to_folder = string.Format("Bundle/{0}", platform_folder);
                return to_folder;
            }
        }

        /// <summary>
        /// Lua脚本目录。
        /// </summary>
        public static string LuaScriptFolder = "ResourcesEx/Lua";

        /// <summary>
        /// Lua脚本后缀名。
        /// </summary>
        public static string LuaScriptExt = ".lua";

        /// <summary>
        /// Lua预编译输出目录。
        /// </summary>
        public static string LuaPrecompileFolder = "LuaPrecompile";

        /// <summary>
        /// Lua预编译后缀名。
        /// </summary>
        public static string LuaPrecompileExt = ".bytes";

        /// <summary>
        /// 预制后缀名。
        /// </summary>
        public static string PrefabExt = ".prefab";

        /// <summary>
        /// XML后缀名。
        /// </summary>
        public static string XMLExt = ".xml";

        /// <summary>
        /// 场景后缀名。
        /// </summary>
        public static string SceneExt = ".unity";

        /// <summary>
        /// 要打包的场景列表。
        /// </summary>
        public static string[] BuildScenes = new string[] { "Assets/Scene/AppRoot.unity", "Assets/Scene/SceneLoader.unity" };

        /// <summary>
        /// 获取当前的资源版本。
        /// </summary>
        public static int ResourceVersion
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取分包数量。
        /// </summary>
        public static int SpiteNumber
        {
            get
            {
                return CurSplitInfos.Count - 1;     //0号表示首包
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 添加打包设置。
        /// </summary>
        /// <param name="abbs">打包设置列表。</param>
        /// <param name="src">资源目录。</param>
        /// <param name="exts">后缀名集合。</param>
        /// <param name="to_prefix">输出子目录。</param>
        private static void AddBundleBuild(List<AssetBundleBuild> abbs, string src, string[] exts, string to_prefix)
        {
            string res_folder = Path.Combine(Application.dataPath, src);
            if (!Directory.Exists(res_folder))
            {
                return;
            }

            string ab_use_folder = Path.Combine("Assets", src);     //打AB包使用的路径包含Assets
            List<string> files = new List<string>();
            foreach (string ext in exts)
            {
                files.AddRange(Directory.GetFiles(res_folder, "*" + ext, SearchOption.AllDirectories));
            }
            foreach (string file in files)
            {
                string res = file.Substring(res_folder.Length + 1).Replace('\\', '/');
                string ab = Path.ChangeExtension(res, AssetBundleManager.AssetBundleExt);
                string asset = Path.Combine(ab_use_folder, res);
                AssetBundleBuild abb = new AssetBundleBuild();
                abb.assetBundleName = Path.Combine(to_prefix, ab); ;
                abb.assetBundleVariant = string.Empty;
                abb.assetNames = new string[1] { asset };
                abbs.Add(abb);
            }
        }

        ///// <summary>
        ///// 添加Lua打包信息。
        ///// </summary>
        ///// <param name="abbs_list">保存的列表。</param>
        ///// <param name="precomplie">是否重新预编译Lua。</param>
        //private static void AddLuaBundleBuild(List<AssetBundleBuild> abbs_list, bool precomplie)
        //{
        //    //先预编译，再打AB包
        //    EditorUtility.DisplayProgressBar("BuildAssetBundle", "PrecompileLua...", 0);
        //    PrecompileLua();
        //    EditorUtility.DisplayProgressBar("BuildAssetBundle", "Collect lua resource info...", 0);
        //    List<string>[] assetnames = new List<string>[3] { new List<string>(), new List<string>(), new List<string>() };
        //    string ab_use_folder = Path.Combine("Assets", LuaPrecompileFolder);     //打AB包使用的路径包含Assets
        //    List<string> files = GetLuaList();
        //    for (int i = 0; i < files.Count; ++i)
        //    {
        //        string file = files[i];
        //        int t = file.StartsWith("Data/") ? 0 : (file.StartsWith("Doc/") ? 1 : 2);
        //        assetnames[t].Add(Path.Combine(ab_use_folder, file));
        //    }

        //    //生成打包信息
        //    for (int i = 0; i < LuaManager.LuaAssetBundleNames.Length; ++i)
        //    {
        //        AssetBundleBuild abb = new AssetBundleBuild();
        //        abb.assetBundleName = "lua/" + LuaManager.LuaAssetBundleNames[i] + AssetBundleManager.AssetBundleExt;
        //        abb.assetBundleVariant = string.Empty;
        //        abb.assetNames = assetnames[i].ToArray();
        //        abbs_list.Add(abb);
        //    }
        //    EditorUtility.ClearProgressBar();
        //}

        /// <summary>
        /// 添加UI打包信息。
        /// </summary>
        /// <param name="abbs_list">保存的列表。</param>
        private static void AddUIBundleBuild(List<AssetBundleBuild> abbs_list)
        {
            //UI资源包括图集、背景图、字体和Prefab
            AddBundleBuild(abbs_list, "ResourcesEx/Atlas", new string[] { ".png" }, "atlas");
            AddBundleBuild(abbs_list, "ResourcesEx/Texture", new string[] { ".png", ".jpg" }, "texture");
            AddBundleBuild(abbs_list, "ResourcesEx/Fonts", new string[] { ".ttf", ".fontsettings" }, "fonts");
            AddBundleBuild(abbs_list, "ResourcesEx/UI", new string[] { PrefabExt }, "ui");
        }

        /// <summary>
        /// 添加角色打包信息。
        /// </summary>
        /// <param name="abbs_list">保存的列表。</param>
        private static void AddActorBundleBuild(List<AssetBundleBuild> abbs_list)
        {
            string path = Path.Combine(Application.dataPath, "ResourcesEx/Character");
            string[] folders = Directory.GetDirectories(path);
            foreach (var folder in folders)
            {
                string name = Path.GetFileNameWithoutExtension(folder);
                if (name.CompareTo("EmptyAnimation") == 0)
                {
                    AddEmptyAnimationBundleBuild(abbs_list);
                }
                else
                {
                    AddActorBundleBuild(abbs_list, name);
                }
            }
        }

        /// <summary>
        /// 添加空动画打包信息。
        /// </summary>
        /// <param name="abbs_list"></param>
        private static void AddEmptyAnimationBundleBuild(List<AssetBundleBuild> abbs_list)
        {
            int n = EditorUtil.ProjectPath.Length + 1;
            string path = Path.Combine(Application.dataPath, "ResourcesEx/Character/EmptyAnimation");
            string[] files = Directory.GetFiles(path, "*.anim", SearchOption.TopDirectoryOnly);
            List<string> assets = new List<string>();
            foreach (string file in files)
            {
                assets.Add(file.Substring(n).Replace('\\', '/'));
            }
            AssetBundleBuild abb = new AssetBundleBuild();
            abb.assetBundleName = "character/emptyanimation" + AssetBundleManager.AssetBundleExt;
            abb.assetBundleVariant = string.Empty;
            abb.assetNames = assets.ToArray();
            abbs_list.Add(abb);
        }

        /// <summary>
        /// 添加角色打包信息。
        /// </summary>
        /// <param name="abbs_list">保存的列表。</param>
        /// <param name="name">角色名称。</param>
        private static void AddActorBundleBuild(List<AssetBundleBuild> abbs_list, string name)
        {
            //模型prefab
            int n = EditorUtil.ProjectPath.Length + 1;
            string prefab_path = Path.Combine(Application.dataPath, string.Format("ResourcesEx/Character/{0}/{0}.prefab", name));
            AssetBundleBuild abb_prefab = new AssetBundleBuild();
            abb_prefab.assetBundleName = string.Format("Character/{0}/{0}{1}", name, AssetBundleManager.AssetBundleExt).ToLower();
            abb_prefab.assetBundleVariant = string.Empty;
            abb_prefab.assetNames = new string[1] { prefab_path.Substring(n) };
            abbs_list.Add(abb_prefab);

            //动作
            string path = Path.Combine(Application.dataPath, string.Format("ResourcesEx/Character/{0}/Animation", name));
            if (!Directory.Exists(path))
            {
                return;
            }
            string[] anim_files = Directory.GetFiles(path, "*.anim", SearchOption.TopDirectoryOnly);
            foreach (string file in anim_files)
            {
                string asset = file.Substring(n).Replace('\\', '/');
                string ab = Path.ChangeExtension(asset, AssetBundleManager.AssetBundleExt).Substring("Assets/ResourcesEx/".Length);
                AssetBundleBuild abb_anim = new AssetBundleBuild();
                abb_anim.assetBundleName = ab;
                abb_anim.assetBundleVariant = string.Empty;
                abb_anim.assetNames = new string[1] { asset };
                abbs_list.Add(abb_anim);
            }
        }

        /// <summary>
        /// 获取Lua文件列表。
        /// </summary>
        /// <returns>Lua文件列表。</returns>
        private static List<string> GetLuaList()
        {            
            string lua_out_folder = Path.Combine(Application.dataPath, LuaPrecompileFolder);
            string[] old_files = Directory.GetFiles(lua_out_folder, "*" + LuaPrecompileExt, SearchOption.AllDirectories);
            List<string> files = new List<string>();
            foreach (string f in old_files)
            {
                string file = f.Substring(lua_out_folder.Length + 1).Replace('\\', '/');
                files.Add(file);
            }
            return files;
        }

        /// <summary>
        /// 预编译Lua。
        /// </summary>
        private static void PrecompileLua()
        {
            //通过时间戳获取要预编译的Lua
            DateTime start = DateTime.Now;
            string lua_folder = Path.Combine(Application.dataPath, LuaScriptFolder);
            string lua_out_folder = Path.Combine(Application.dataPath, LuaPrecompileFolder);
            EditorUtil.CheckOrCreateFolder(lua_out_folder);
            Dictionary<string, long> src_time = BuildTools.GetFileLastWriteTime(lua_folder, "*" + LuaScriptExt);
            Dictionary<string, long> dst_time = BuildTools.GetFileLastWriteTime(lua_out_folder, "*" + LuaPrecompileExt);
            List<string> need_copy = BuildTools.GetNeedCopyFiles(src_time, dst_time, LuaPrecompileExt);

            //预编译
            bool use_chunk = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            Log.Info(string.Format("PrecompileLua Copy:{0} Remove:{1}", need_copy.Count, dst_time.Count));
            for (int i = 0; i < need_copy.Count; i++)
            {
                string str = need_copy[i];
                string file = lua_folder + str;
                string to_file = lua_out_folder + Path.ChangeExtension(str, LuaPrecompileExt);
                BuildTools.CopyLua(file, to_file, use_chunk);
                EditorUtility.DisplayProgressBar("CopyLua", str, (i + 1.0f) / need_copy.Count);
            }

            //删除已经不存在的Lua
            EditorUtility.DisplayProgressBar("CopyLua", "Remove redundant files...", 1);
            foreach (var kvp in dst_time)
            {
                string to_file = lua_out_folder + kvp.Key;
                File.Delete(to_file);
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Log.Info("Precompile lua finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 更新AB包依赖信息。
        /// </summary>
        private static void UpdateAssetBundleDepend()
        {
            //加载原来的->加载最近打包的->更新->保存
            Dictionary<string, List<string>> last_dep = LoadLastAssetBundleDepend();
            Dictionary<string, List<string>> new_dep = LoadNewAssetBundleDepend();
            foreach (var kvp in new_dep)
            {
                last_dep[kvp.Key] = kvp.Value;
            }
            SaveAssetBundleDepend(last_dep);
        }

        /// <summary>
        /// 加载最后生成的依赖信息。
        /// </summary>
        /// <returns>依赖信息。</returns>
        private static Dictionary<string, List<string>> LoadLastAssetBundleDepend()
        {
            string dep_file = Path.Combine(EditorUtil.ProjectPath, Path.Combine(BundleOutputFolder, "Depend.txt"));
            Dictionary<string, List<string>> depend = new Dictionary<string, List<string>>();
            string[] lines = EditorUtil.LoadTextFileLines(dep_file);
            List<string> cur_dep = new List<string>();
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i];
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.StartsWith("-"))
                    {
                        string dep_name = line.Substring(1);
                        cur_dep.Add(dep_name);
                    }
                    else
                    {
                        cur_dep = new List<string>();
                        depend.Add(line, cur_dep);
                    }
                }
            }
            return depend;
        }

        /// <summary>
        /// 加载新的依赖信息。
        /// </summary>
        /// <returns>依赖信息。</returns>
        private static Dictionary<string, List<string>> LoadNewAssetBundleDepend()
        {
            Dictionary<string, List<string>> new_dep = new Dictionary<string, List<string>>();
            string platform = EditorUserBuildSettings.activeBuildTarget.ToString();
            string ab_file = Path.Combine(EditorUtil.ProjectPath, Path.Combine(BundleOutputFolder, platform));
            AssetBundle ab = AssetBundle.LoadFromFile(ab_file);
            AssetBundleManifest abm = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] ab_names = abm.GetAllAssetBundles();
            foreach (var name in ab_names)
            {
                string[] ab_dependencies = abm.GetAllDependencies(name);
                List<string> abs = new List<string>();
                foreach (var dep in ab_dependencies)
                {
                    abs.Add(dep.Substring(0, dep.Length - AssetBundleManager.AssetBundleExt.Length));
                }
                new_dep.Add(name.Substring(0, name.Length - AssetBundleManager.AssetBundleExt.Length), abs);
            }
            ab.Unload(true);
            ab = null;
            return new_dep;
        }

        /// <summary>
        /// 保存AB依赖信息。
        /// </summary>
        /// <param name="depend">依赖信息。</param>
        private static void SaveAssetBundleDepend(Dictionary<string, List<string>> depend)
        {
            StringBuilder sb = new StringBuilder();
            List<string> keys = new List<string>();
            keys.AddRange(depend.Keys);
            keys.Sort();
            for (int i = 0; i < keys.Count; ++i)
            {
                string key = keys[i];
                List<string> dep = depend[key];
                if (dep.Count > 0)
                {
                    if (i > 0)
                    {
                        sb.AppendLine();
                    }

                    sb.AppendLine(key);
                    dep.Sort();
                    foreach (var d in dep)
                    {
                        sb.AppendLine("-" + d);
                    }
                }
            }

            string dep_file = Path.Combine(EditorUtil.ProjectPath, Path.Combine(BundleOutputFolder, "Depend.txt"));
            EditorUtil.SaveTextFile(dep_file, sb.ToString());
        }

        /// <summary>
        /// 划分内置资源。
        /// </summary>
        /// <param name="split">是否启用分包。</param>
        private static void DivideAppResBundle(bool split)
        {
            DateTime start = DateTime.Now;
            DivideBundle("appres", false);         //appres必须放到首包里
            AssetDatabase.Refresh();
            Log.Info("Divide appres bundle finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 划分角色模型动作资源。
        /// </summary>
        private static void DivideActorBundle(bool split)
        {
            DateTime start = DateTime.Now;
            DivideBundle("character", split);
            AssetDatabase.Refresh();
            Log.Info("Divide character bundle finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        ///// <summary>
        ///// 划分特效资源。
        ///// </summary>
        //private static void DivideEffectBundle(bool split)
        //{
        //    Log.Info("DivideEffectBundle split:{0}", split);
        //}

        ///// <summary>
        ///// 划分Lua脚本资源。
        ///// </summary>
        //private static void DivideLuaBundle(bool split)
        //{
        //    DateTime start = DateTime.Now;
        //    string src_folder = Path.Combine(BundleFolder, "Lua");
        //    string dst_folder = Path.Combine(Application.streamingAssetsPath, "lua");
        //    EditorUtil.CheckOrCreateFolder(dst_folder);
        //    foreach (var lua in LuaManager.LuaAssetBundleNames)
        //    {
        //        string lua_ab = lua + AssetBundleManager.AssetBundleExt;
        //        File.Copy(Path.Combine(src_folder, lua_ab), Path.Combine(dst_folder, lua_ab), true);
        //    }
        //    AssetDatabase.Refresh();
        //    Log.Info("Divide lua bundle finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        //}

        /// <summary>
        /// 划分场景资源。
        /// </summary>
        private static void DivideSceneBundle(bool split)
        {
            DateTime start = DateTime.Now;
            DivideBundle("scene", split);
            AssetDatabase.Refresh();
            Log.Info("Divide scene bundle finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 划分UI资源。
        /// </summary>
        private static void DivideUIBundle(bool split)
        {
            DateTime start = DateTime.Now;
            DivideBundle("atlas", split);
            DivideBundle("texture", split);
            DivideBundle("fonts", split);
            DivideBundle("ui", split);
            AssetDatabase.Refresh();
            Log.Info("Divide ui bundle finished! use {0} sec. ", (DateTime.Now - start).TotalSeconds);
        }

        /// <summary>
        /// 划分资源。
        /// </summary>
        /// <param name="src">AB包源目录。(相对于Bundle)</param>
        /// <param name="dst">AB包目标目录。(相对于StreamingAssets)</param>
        /// <param name="split">是否启用分包。</param>
        private static void DivideBundle(string folder, bool split)
        {
            string src_folder = Path.Combine(BundleFolder, folder);
            string dst_folder = Path.Combine(Application.streamingAssetsPath, folder);     //首包目录
            if (Directory.Exists(dst_folder))
            {
                Directory.Delete(dst_folder, true);       //AB包可以随意重新生成meta文件和分配GUID，所以简单粗暴清除文件夹所有内容
                File.Delete(dst_folder + ".meta");
            }            
            Dictionary<string, int> ab_info = GetBundleSpiltInfo(src_folder, split);
            foreach (var kvp in ab_info)
            {
                if (kvp.Value == 0)
                {
                    //首包
                    string sub_file = kvp.Key.Substring(src_folder.Length+1);
                    string to_file = Path.Combine(dst_folder, sub_file);
                    EditorUtil.CheckOrCreateFolder(Path.GetDirectoryName(to_file));
                    File.Copy(kvp.Key, to_file);
                }
                else
                {
                    //对应分包目录
                }
            }
        }

        /// <summary>
        /// 获取分包信息。
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="split"></param>
        private static Dictionary<string, int> GetBundleSpiltInfo(string folder, bool split)
        {
            //分配
            Dictionary<string, int> ret = new Dictionary<string, int>();
            string[] files = Directory.GetFiles(folder, "*" + AssetBundleManager.AssetBundleExt, SearchOption.AllDirectories);
            foreach (var f in files)
            {
                if (split)
                {
                    //通过资源相对路径获取分包号
                }
                else
                {
                    ret.Add(f, 0);
                }
            }
            return ret;
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 各个资源的划分函数。
        /// </summary>
        private static Dictionary<AssetType, Action<bool>> AssetDivideFunction = new Dictionary<AssetType, Action<bool>>();

        /// <summary>
        /// 当前的分包信息。
        /// </summary>
        private static List<SplitInfo> CurSplitInfos = new List<SplitInfo>();

        #endregion
    }
}