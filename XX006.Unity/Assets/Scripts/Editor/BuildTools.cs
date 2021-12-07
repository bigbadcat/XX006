using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using XuXiang;
using XuXiang.EditorTools;

namespace XX006.EditorTools
{
    /// <summary>
    /// 通用操作函数。
    /// </summary>
    public static class BuildTools
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 移除指定目录多余的AB包。
        /// </summary>
        /// <param name="folder">生成AB包的目录。</param>
        public static void RemoveRedundantAssetBundle(string folder)
        {
            string[] old_files = Directory.GetFiles(folder, "*" + AssetBundleManager.AssetBundleExt, SearchOption.AllDirectories);
            Dictionary<string, bool> old_files_set = new Dictionary<string, bool>();
            foreach (string f in old_files)
            {
                string file = f.Substring(folder.Length + 1).Replace('\\', '/');
                old_files_set.Add(file, true);
            }

            string ab_name = folder.Substring(folder.LastIndexOf('/')+1);
            string ab_file = Path.Combine(folder, ab_name);
            AssetBundle ab_main = AssetBundle.LoadFromFile(ab_file);
            if (ab_main == null)
            {
                return;
            }

            AssetBundleManifest ab_manifest = ab_main.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            string[] ab_files = ab_manifest.GetAllAssetBundles();
            foreach (string ab in ab_files)
            {
                old_files_set.Remove(ab);
            }
            ab_main.Unload(true);

            //移除Lua不存在的文件
            if (old_files_set.Count > 0)
            {
                foreach (var kvp in old_files_set)
                {
                    string f = Path.Combine(folder, kvp.Key);
                    File.Delete(f);
                    File.Delete(f + ".manifest");
                }
            }
        }

        /// <summary>
        /// 拷贝AB包依赖总表。
        /// </summary>
        /// <param name="folder">生成AB包的目录。</param>
        /// <param name="to_folder">要拷贝到的AB包目录。</param>
        public static void CopyManifest(string folder, string to_folder)
        {
            string ab_name = folder.Substring(folder.LastIndexOf('/') + 1);
            string ab_file = Path.Combine(folder, ab_name);
            string to_file = Path.Combine(to_folder, "bundle" + AssetBundleManager.AssetBundleExt);
            EditorUtil.CheckOrCreateFolder(to_folder);
            File.Copy(ab_file, to_file, true);
        }

        /// <summary>
        /// 拷贝依赖文件。
        /// </summary>
        /// <param name="folder">依赖文件所在文件夹。</param>
        public static void CopyAssetBundleDepend(string folder)
        {
            string src_file = Path.Combine(folder, "Depend.txt");
            string dst_file = Path.Combine(Application.dataPath, "Resources/Depend.txt");
            File.Copy(src_file, dst_file, true);
        }

        /// <summary>
        /// 生成AB包清单。
        /// </summary>
        /// <param name="folder">生成AB包的目录。</param>
        public static void BuildAssetBundleList(string folder)
        {

        }

        /// <summary>
        /// 预编译Lua文件。
        /// </summary>
        /// <param name="file">源文件。</param>
        /// <param name="to_file">目标文件。</param>
        public static void PrecompileLua(string file, string to_file)
        {
            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = Path.Combine(Application.dataPath, LuaPrecompileExe);
            info.UseShellExecute = false;
            info.CreateNoWindow = true;
            info.Arguments = string.Format("-s -o {0} {1}", to_file, file);
            Process p = Process.Start(info);
            p.WaitForExit();
        }

        /// <summary>
        /// 拷贝Lua文件。
        /// </summary>
        /// <param name="file">源文件。</param>
        /// <param name="to_file">目标文件。</param>
        /// <param name="use_chunk">是否启用预编译。</param>
        public static void CopyLua(string file, string to_file, bool use_chunk)
        {
            if (use_chunk)
            {
                PrecompileLua(file, to_file);
                return;
            }

            //拷贝，要去掉Bom，根据有无签名确定长度和读取起始位置
            FileStream lua = File.OpenRead(file);
            byte[] bytes;
            if (lua.Length > 0)
            {
                bool widthbom = lua.ReadByte() == 239;
                int count = (int)lua.Length - (widthbom ? 3 : 0);
                lua.Seek(widthbom ? 3 : 0, SeekOrigin.Begin);
                //读取字节
                bytes = new byte[count];         //返回的数组末尾不能有多余的0
                lua.Read(bytes, 0, bytes.Length);
            }
            else
            {
                bytes = new byte[0];
            }
            lua.Dispose();
            lua = null;
            string to_folder = Path.GetDirectoryName(to_file);
            EditorUtil.CheckOrCreateFolder(to_folder);
            FileStream to_lua = File.Open(to_file, FileMode.Create);
            to_lua.Write(bytes, 0, bytes.Length);
            to_lua.Flush();
            to_lua.Dispose();
            to_lua = null;
        }

        /// <summary>
        /// 获取文件的最后修改时间。
        /// </summary>
        /// <param name="folder">文件夹。</param>
        /// <param name="sp">搜索匹配符。</param>
        /// <returns>文件和修改时间。</returns>
        public static Dictionary<string, long> GetFileLastWriteTime(string folder, string sp)
        {
            Dictionary<string, long> dic = new Dictionary<string, long>();
            string[] files = Directory.GetFiles(folder, sp, SearchOption.AllDirectories);
            foreach (string file in files)
            {
                FileInfo finfo = new FileInfo(file);
                dic.Add(file.Substring(folder.Length), finfo.LastWriteTime.Ticks);
            }
            return dic;
        }

        /// <summary>
        /// 获取需要拷贝的文件。
        /// </summary>
        /// <param name="src_time">源文件时间戳。</param>
        /// <param name="dst_time">目标文件时间戳。(函数调用后只留下src_time中不存在的文件，若不想修改则拷贝一份再传入)</param>
        /// <param name="dst_ext">目标文件扩展名。</param>
        /// <returns>需要拷贝的文件。</returns>
        public static List<string> GetNeedCopyFiles(Dictionary<string, long> src_time, Dictionary<string, long> dst_time, string dst_ext)
        {
            List<string> need_copy = new List<string>();        //需要拷贝的文件
            foreach (var kvp in src_time)
            {
                long d;
                bool copy = true;
                string dst_key = Path.ChangeExtension(kvp.Key, dst_ext);
                if (dst_time.TryGetValue(dst_key, out d))
                {
                    copy = kvp.Value > d;                       //最后修改的时间戳判断
                    dst_time.Remove(dst_key);                   //一边判断一边移除，剩下的就是要删除的多余文件
                }
                if (copy)
                {
                    need_copy.Add(kvp.Key);
                }
            }
            return need_copy;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// Lua预编译程序路径。
        /// </summary>
        public static string LuaPrecompileExe = "../MyTools/luac.exe";

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------
        #endregion
    }
}