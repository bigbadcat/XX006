using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// AssetBundle管理。
    /// </summary>
    public class AssetBundleManager : Singleton<AssetBundleManager>
    {
        /// <summary>
        /// AB包信息。
        /// </summary>
        public class AssetBundleInfo
        {
            /// <summary>
            /// 构造函数。
            /// </summary>
            /// <param name="path">AB包路径。</param>
            /// <param name="ab">AB包内容。</param>
            public AssetBundleInfo(string path, AssetBundle ab)
            {
                Path = path;
                Count = 1;
                Bundle = ab;
                OnFinish = new List<Action<AssetBundle>>();
            }

            /// <summary>
            /// 构造一个异步加载AB包的信息。
            /// </summary>
            /// <param name="path">AB包路径。</param>
            /// <param name="req">异步加载请求。</param>
            public AssetBundleInfo(string path, AssetBundleCreateRequest req)
            {
                Path = path;
                Count = 1;
                Request = req;
                OnFinish = new List<Action<AssetBundle>>();
            }

            /// <summary>
            /// 检测加载情况。
            /// </summary>
            public void CheckLoad()
            {
                if (Request == null)
                {
                    return;
                }

                if (Request.isDone)
                {
                    //加载完成
                    Bundle = Request.assetBundle;
                    Request = null;
                }
            }

            /// <summary>
            /// 设置AB包。
            /// </summary>
            public void SetBundle(AssetBundle ab)
            {
                //已经有AB包了不能再设置不同AB包。
                if (Bundle != null || Bundle != ab)
                {
                    Log.Error("The Bundle is not null. path:{0}", Path);
                    return;
                }

                //取消原来的异步加载
                if (Request != null)
                {
                    Request = null;
                }
                Bundle = ab;
            }

            /// <summary>
            /// AB包路径。
            /// </summary>
            public string Path { get; private set; }

            /// <summary>
            /// 引用计数。
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// 是否加载中。
            /// </summary>
            public bool IsLoading { get { return Request != null; } }

            /// <summary>
            /// 加载过程。
            /// </summary>
            private AssetBundleCreateRequest Request { get; set; }

            /// <summary>
            /// 完成回调列表。
            /// </summary>
            public List<Action<AssetBundle>> OnFinish {get; private set;}

            /// <summary>
            /// AB包。
            /// </summary>
            public AssetBundle Bundle { get; private set; }

            /// <summary>
            /// 添加使用计数。
            /// </summary>
            public void AddCount()
            {
                ++Count;
            }

            /// <summary>
            /// 减少使用计数。若为0将会卸载AB包。
            /// </summary>
            public void SubCount()
            {
                --Count;
                if (Count == 0 && Bundle != null)
                {
                    Bundle.Unload(true);
                    Bundle = null;
                }
            }
        }

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 加载AB包依赖信息。
        /// </summary>
        public void LoadDepend()
        {
            string text = LoadDependText();
            string[] lines = text.Replace("\r\n", "\n").Split('\n');            
            List<string> cur_dep = new List<string>();
            m_Depend.Clear();
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
                        m_Depend.Add(line, cur_dep);
                    }
                }
            }
        }

        /// <summary>
        /// 加载AB包。
        /// </summary>
        /// <param name="path">AB包不带后缀名的路径。(相对StreamingAssets或Application.temporaryCachePath)</param>
        /// <returns>AB包对象。</returns>
        public AssetBundle LoadAssetBundle(string path)
        {
            AssetBundleInfo info;
            if (m_AssetBundleInfos.TryGetValue(path, out info))
            {
                info.AddCount();
                if (info.IsLoading)
                {
                    //取消异步加载，直接同步加载完成，不确定是否可以这么干，待测试验证
                    info.SetBundle(Load(path));
                }
                return info.Bundle;
            }

            //加载依赖的
            List<string> dependencies;
            if (m_Depend.TryGetValue(path, out dependencies))
            {
                foreach (var dep in dependencies)
                {
                    LoadAssetBundle(dep);
                }
            }

            //加载新的
            AssetBundle ab = Load(path);
            info = new AssetBundleInfo(path, ab);
            m_AssetBundleInfos.Add(path, info);
            return info.Bundle;
        }

        /// <summary>
        /// 异步加载AB包。
        /// </summary>
        /// <param name="path">AB包不带后缀名的路径。(相对StreamingAssets或Application.temporaryCachePath)</param>
        /// <param name="on_finish">加载AB完成回调，若失败则为null。</param>
        public void LoadAssetBundleAsync(string path, Action<AssetBundle> on_finish)
        {
            //加载依赖的
            List<string> dependencies;
            if (m_Depend.TryGetValue(path, out dependencies))
            {
                foreach (var dep in dependencies)
                {
                    LoadAssetBundleAsync(dep, null);
                }
            }

            AssetBundleInfo info;
            if (!m_AssetBundleInfos.TryGetValue(path, out info))
            {
                //开启一个异步加载
                info = new AssetBundleInfo(path, LoadAsync(path));
                m_AssetBundleInfos.Add(path, info);
            }
            else
            {
                //已经在加载中或者已经完成，添加引用计数，等着队列处理就行
                info.AddCount();
            }
            if (on_finish != null)
            {
                info.OnFinish.Add(on_finish);
            }
            if (!m_AsynLoadList.Contains(info))
            {
                m_AsynLoadList.Add(info);
            }
        }

        /// <summary>
        /// 卸载AB包。
        /// </summary>
        /// <param name="path">LoadAssetBundle使用的参数。</param>
        public void UnloadAssetBundle(string path)
        {
            AssetBundleInfo info;
            if (!m_AssetBundleInfos.TryGetValue(path, out info))
            {
                return;
            }

            info.SubCount();
            if (info.Count == 0)
            {
                m_AssetBundleInfos.Remove(path);

                //移除依赖的
                List<string> dependencies;
                if (m_Depend.TryGetValue(path, out dependencies))
                {
                    foreach (var dep in dependencies)
                    {
                        UnloadAssetBundle(dep);
                    }
                }
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// AB包后缀名。
        /// </summary>
        public static string AssetBundleExt = ".unity3d";

        /// <summary>
        /// 当前AB包使用情况。
        /// </summary>
        public Dictionary<string, AssetBundleInfo> AssetBundleInfos
        {
            get
            {
                return m_AssetBundleInfos;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 获取AB包的实际路径。
        /// </summary>
        /// <param name="path">AB资源路径。</param>
        /// <returns>AB包的实际路径。</returns>
        private static string GetAssetBundleFile(string path)
        {
            //启用热更先检测下载目录有没有更新的资源

            //包内读取
            string folder = string.Empty;
#if UNITY_EDITOR
            folder = Application.streamingAssetsPath;
#elif UNITY_ANDROID
            folder = Application.dataPath + "!assets";
#elif UNITY_IPHONE
            folder = Application.streamingAssetsPath;
#else
            folder = Application.streamingAssetsPath;
#endif
            string ab_file = Path.Combine(folder, path + AssetBundleExt);
            return ab_file;
        }

        /// <summary>
        /// 加载AB包。
        /// </summary>
        /// <param name="path">AB包不带后缀名的路径。(相对StreamingAssets或Application.temporaryCachePath)</param>
        /// <returns>AB包对象。</returns>
        private static AssetBundle Load(string path)
        {
            string ab_file = GetAssetBundleFile(path);
            AssetBundle ab = AssetBundle.LoadFromFile(ab_file);
            return ab;
        }

        /// <summary>
        /// 异步加载AB包。
        /// </summary>
        /// <param name="path">AB包不带后缀名的路径。(相对StreamingAssets或Application.temporaryCachePath)</param>
        /// <returns>AB包加载对象。</returns>
        private static AssetBundleCreateRequest LoadAsync(string path)
        {
            string ab_file = GetAssetBundleFile(path);
            AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(ab_file);
            return req;
        }

        /// <summary>
        /// 加载依赖信息文本。
        /// </summary>
        /// <returns>依赖信息文本。</returns>
        private static string LoadDependText()
        {
            //判断是否有更新文件，优先加载更新

            //没有就加载Resource下的
            TextAsset asset = Resources.Load<TextAsset>("Depend");
            string text = asset.text;
            Resources.UnloadAsset(asset);
            asset = null;
            return text;
        }

        /// <summary>
        /// 帧更新。
        /// </summary>
        private void Update()
        {
            //先所有AB包都检查一遍加载情况
            for (int i = 0; i < m_AsynLoadList.Count; ++i)
            {
                AssetBundleInfo info = m_AsynLoadList[i];
                info.CheckLoad();
            }

            //处理已经加载完成的AB包
            m_AsynNeedRemove.Clear();
            for (int i=0;i<m_AsynLoadList.Count; ++i)
            {
                //自身加载完毕和依赖的AB包也加载完毕
                AssetBundleInfo info = m_AsynLoadList[i];
                if (info.IsLoading || !IsDependFinish(info.Path))
                {
                    continue;
                }

                //判断是否已经无用了
                if (info.Count > 0)
                {
                    //触发回调
                    foreach (var call in info.OnFinish)
                    {
                        call(info.Bundle);
                    }
                    info.OnFinish.Clear();
                }
                else
                {
                    //重新触发移除
                    info.AddCount();
                    info.SubCount();
                }
                m_AsynNeedRemove.Add(i);
            }

            //移除已经完成的，倒着遍历保证索引一直有效
            for (int i=m_AsynNeedRemove.Count-1; i>=0; --i)
            {
                m_AsynLoadList.RemoveAt(i);
            }
        }

        /// <summary>
        /// 判断依赖的AB包是否加载完成。
        /// </summary>
        /// <param name="path">AB包路径。</param>
        /// <returns>是否加载完成。</returns>
        private bool IsDependFinish(string path)
        {
            //获取依赖信息，没有就返回完成
            List<string> dependencies;
            if (!m_Depend.TryGetValue(path, out dependencies))
            {
                return true;
            }

            //逐个判断依赖信息，必须所有完成
            bool ok = true;
            foreach (var dep in dependencies)
            {
                AssetBundleInfo dep_info;
                if (m_AssetBundleInfos.TryGetValue(dep, out dep_info))
                {
                    if (dep_info.IsLoading)
                    {
                        ok = false;
                        break;
                    }
                }
                else
                {
                    //依赖的资源包信息没找到，不应该
                    Log.Error("Can not find depend AssetBundleInfo. path:{0} dep:{1}", path, dep);
                }
            }

            return ok;
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 当前AB包使用情况。
        /// </summary>
        private Dictionary<string, AssetBundleInfo> m_AssetBundleInfos = new Dictionary<string, AssetBundleInfo>();

        /// <summary>
        /// 异步加载列表。
        /// </summary>
        private List<AssetBundleInfo> m_AsynLoadList = new List<AssetBundleInfo>();

        /// <summary>
        /// 需要从异步列表
        /// </summary>
        private List<int> m_AsynNeedRemove = new List<int>();

        /// <summary>
        /// AB依赖信息。
        /// </summary>
        private Dictionary<string, List<string>> m_Depend = new Dictionary<string, List<string>>();

        #endregion
    }
}