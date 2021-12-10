using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 资源管理类。
    /// 除了LoadObject接口以外，其它LoadXXX接口加载的资源，需要手动调用UnloadAsset才会释放。
    /// </summary>
    public class ResourceManager : Singleton<ResourceManager>
    {
        /// <summary>
        /// 资源信息。
        /// </summary>
        public class AssetInfo
        {
            /// <summary>
            /// 构造函数。
            /// </summary>
            /// <param name="path">资源路径。</param>
            /// <param name="load_all">是否加载全部资源。</param>
            /// <param name="async">是否异步加载。</param>
            public AssetInfo(string path, bool load_all, bool async)
            {
                Count = 1;
                Path = path;
                m_LoadAll = load_all;
                Count = 1;
                if (ResourceManager.Instance.IsReadAssetBundle)
                {
                    if (async)
                    {
                        IsLoading = true;
                        AssetBundleManager.Instance.LoadAssetBundleAsync(Path.ToLower(), OnAssetBundleLoad);
                    }
                    else
                    {
                        m_AssetBundle = AssetBundleManager.Instance.LoadAssetBundle(Path.ToLower());
                    }
                    
                }
            }

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
                if (Count == 0 && !IsLoading)
                {
                    Unload();
                }
            }

            /// <summary>
            /// 卸载资源。
            /// </summary>
            private void Unload()
            {
                //AB包的资源由AB管理负责卸载
                if (ResourceManager.Instance.IsReadAssetBundle)
                {
                    AssetBundleManager.Instance.UnloadAssetBundle(Path.ToLower());
                }
                else
                {
                    if (m_AllAssets != null)
                    {
                        foreach (var asset in m_AllAssets)
                        {
                            if (!(asset is GameObject))
                            {
                                Resources.UnloadAsset(asset);
                            }
                        }
                    }
                    else if (!(m_Asset is GameObject))
                    {
                        Resources.UnloadAsset(m_Asset);
                    }
                }
                m_OnFinish.Clear();
                m_Request = null;
                m_AssetBundle = null;
                m_Asset = null;
                m_AllAssets = null;
            }

            /// <summary>
            /// 检测加载情况。
            /// </summary>
            public void CheckLoad()
            {
                if (!IsLoading)
                {
                    return;
                }

                //等待加载完成
                if (m_Request != null && m_Request.isDone)
                {
                    if (m_LoadAll)
                    {
                        m_AllAssets = m_Request.allAssets;
                        m_Asset = (m_AllAssets != null && m_AllAssets.Length > 0) ? m_AllAssets[0] : null;      //顺便把主资源也赋值了
                    }
                    else
                    {
                        m_Asset = m_Request.asset;
                    }
                    m_Request = null;
                    IsLoading = false;
                }
            }

            /// <summary>
            /// AB包加载完成通知。
            /// </summary>
            /// <param name="ab">要加载的AB包。</param>
            public void OnAssetBundleLoad(AssetBundle ab)
            {
                if (ab == null)
                {
                    IsLoading = false;
                    return;
                }

                if (m_LoadAll)
                {
                    m_Request = ab.LoadAllAssetsAsync();
                }
                else
                {
                    string asset_path = ab.GetAllAssetNames()[0];       //加载第一个资源
                    m_Request = ab.LoadAssetAsync(asset_path, typeof(UnityEngine.Object));
                }                
            }

            /// <summary>
            /// 资源路径。
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// 是否加载中。
            /// </summary>
            public bool IsLoading { get; set; }

            /// <summary>
            /// 是否要加载所有资源。
            /// </summary>
            private bool m_LoadAll;

            /// <summary>
            /// 引用计数。
            /// </summary>
            public int Count { get; private set; }

            /// <summary>
            /// 完成回调列表。
            /// </summary>
            public List<Action<UnityEngine.Object>> OnFinish
            {
                get { return m_OnFinish; }
            }
            private List<Action<UnityEngine.Object>> m_OnFinish = new List<Action<UnityEngine.Object>>();

            /// <summary>
            /// 资源AB包。
            /// </summary>
            private AssetBundle m_AssetBundle;

            /// <summary>
            /// 资源加载请求。
            /// </summary>
            private AssetBundleRequest m_Request;

            /// <summary>
            /// 资源对象。
            /// </summary>
            internal UnityEngine.Object Asset
            {
                get
                {
                    if (IsLoading)
                    {
                        Log.Error("The asset is loading in async. path:{0}", Path);
                        return null;
                    }

                    if (m_Asset == null)
                    {
                        if (ResourceManager.Instance.IsReadAssetBundle)
                        {
                            string asset_path = m_AssetBundle.GetAllAssetNames()[0];       //加载第一个资源
                            m_Asset = m_AssetBundle.LoadAsset(asset_path, typeof(UnityEngine.Object));
                        }
                        else
                        {
#if UNITY_EDITOR
                            string file = "Assets/ResourcesEx/" + Path + GetAssetExt(Path);
                            m_Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file);
                            if (m_Asset == null)
                            {
                                Log.Warning("Load asset failed at path({0}).", file);
                            }
#endif
                        }
                    }
                    return m_Asset;
                }
            }
            private UnityEngine.Object m_Asset;

            /// <summary>
            /// 所有资源对象。
            /// </summary>
            internal UnityEngine.Object[] AllAssets
            {
                get
                {
                    if (IsLoading)
                    {
                        Log.Error("The asset is loading in async. path:{0}", Path);
                        return null;
                    }

                    if (m_AllAssets == null)
                    {
                        if (ResourceManager.Instance.IsReadAssetBundle)
                        {
                            m_AllAssets = m_AssetBundle.LoadAllAssets();
                        }
                        else
                        {
#if UNITY_EDITOR
                            string file = "Assets/ResourcesEx/" + Path + GetAssetExt(Path);
                            m_AllAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(file);
#endif
                        }
                    }
                    return m_AllAssets;
                }
            }
            private UnityEngine.Object[] m_AllAssets;
        }

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 加载游戏对象。
        /// </summary>
        /// <param name="path">资源路径。(仅限于不带后缀的Prefab或AssetBundle文件)</param>
        /// <returns>游戏对象。</returns>
        public GameObject LoadObject(string path)
        {
            GameObject prefab = LoadAsset<GameObject>(path);
            if (prefab == null)
            {
                return null;
            }

            GameObject obj = Instantiate(prefab);
            if (obj != null)
            {
                ResourceMark mark = obj.AddComponent<ResourceMark>();
                mark.ResPath = path;

#if UNITY_EDITOR
                UpdateShaderForEditorAB(obj);
#endif
            }

            return obj;
        }

        /// <summary>
        /// 加载游戏对象。
        /// </summary>
        /// <typeparam name="T">返回特定组件。</typeparam>
        /// <param name="path">资源路径。</param>
        /// <returns>组件对象。</returns>
        public T LoadObject<T>(string path) where T : Component
        {
            //加载对象
            GameObject obj = LoadObject(path);
            if (obj == null)
            {
                return null;
            }

            //获取组件，没对应组件则销毁对象
            T t = obj.GetComponent<T>();
            if (t == null)
            {
                Destroy(obj);
                UnloadAsset(path);
            }

            return t;
        }

        /// <summary>
        /// 加载游戏对象。
        /// </summary>
        /// <param name="path">资源路径。(仅限于不带后缀的Prefab或AssetBundle文件)</param>
        /// <param name="on_finish">加载回调。</param>
        public void LoadObjectAsync(string path, Action<GameObject> on_finish)
        {
            Action<GameObject> call = (prefab) =>
            {
                if (prefab == null)
                {
                    if (on_finish != null)
                    {
                        on_finish(null);
                    }                    
                    return;
                }

                GameObject obj = Instantiate(prefab);
                if (obj != null)
                {
                    ResourceMark mark = obj.AddComponent<ResourceMark>();
                    mark.ResPath = path;
#if UNITY_EDITOR
                    UpdateShaderForEditorAB(obj);
#endif
                }
                if (on_finish != null)
                {
                    on_finish(obj);
                }
            };
            LoadAssetAsync<GameObject>(path, ".prefab", call);
        }

        /// <summary>
        /// 加载动画。
        /// </summary>
        /// <param name="path">动画路径。</param>
        /// <returns>动画对象。</returns>
        public AnimationClip LoadAnimation(string path)
        {
            AnimationClip clip = LoadAsset<AnimationClip>(path);
            return clip;
        }

        /// <summary>
        /// 加载纹理。
        /// </summary>
        /// <param name="path">动画纹理，编辑器下仅支持png和jpg后缀。</param>
        /// <returns>纹理对象。</returns>
        public Texture LoadTexture(string path)
        {
            Texture tex = LoadAsset<Texture>(path);
            return tex;
        }

        /// <summary>
        /// 加载文本。
        /// </summary>
        /// <param name="path">文本路径。</param>
        /// <returns>文本对象。</returns>
        public TextAsset LoadText(string path)
        {
            TextAsset text = LoadAsset<TextAsset>(path);
            return text;
        }

        /// <summary>
        /// 加载图集。
        /// </summary>
        /// <param name="path">图集路径。</param>
        /// <returns>精灵集合。</returns>
        public Dictionary<string, Sprite> LoadAtlas(string path)
        {
            Dictionary<string, Sprite> ret = new Dictionary<string, Sprite>();
            UnityEngine.Object[] assets = LoadAllAsset(path);
            if (assets == null)
            {
                return ret;
            }
            foreach (var asset in assets)
            {
                Sprite sp = asset as Sprite;
                if (sp != null)
                {
                    ret.Add(sp.name, sp);
                }
            }
            return ret;
        }

        /// <summary>
        /// 添加资源的引用计数。
        /// </summary>
        /// <param name="path">资源路径。</param>
        public void AddAssetCount(string path)
        {
            AssetInfo info;
            if (m_AssetInfos.TryGetValue(path, out info))
            {
                info.AddCount();
            }
        }

        /// <summary>
        /// 卸载资源。
        /// </summary>
        /// <param name="path">资源路径。</param>
        public void UnloadAsset(string path)
        {
            //Log.Info("UnloadAsset:{0}", path);
            AssetInfo info;
            if (m_AssetInfos.TryGetValue(path, out info))
            {
                if (info.Count > 0)
                {
                    info.SubCount();
                    if (info.Count <= 0)
                    {
                        m_AssetInfos.Remove(path);
                    }
                }
                else
                {
                    Log.Warning("The asset has already unload. path:{0}", path);
                }
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置是否读取AB包。
        /// </summary>
        public bool IsReadAssetBundle
        {
            get { return m_IsReadAssetBundle; }
            set { m_IsReadAssetBundle = value; }
        }

        /// <summary>
        /// 获取资源信息。(谨慎操作，建议只读不写)
        /// </summary>
        public Dictionary<string, AssetInfo> AssetInfos
        {
            get { return m_AssetInfos; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

#if UNITY_EDITOR

        /// <summary>
        /// 获取资源的后缀名。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>资源名称。</returns>
        private static string GetAssetExt(string path)
        {
            string folder = "Assets/ResourcesEx/" + Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path).ToLower();
            var files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var f in files)
            {
                if (!f.EndsWith("meta") && Path.GetFileNameWithoutExtension(f).ToLower() == name)
                {
                    return Path.GetExtension(f);
                }
            }
            Log.Error("Can not find asset. path:{0}", path);
            return string.Empty;
        }

        /// <summary>
        /// 更新Eiditor下AB模式使用的shader。
        /// </summary>
        /// <param name="obj">游戏对象。</param>
        private void UpdateShaderForEditorAB(GameObject obj)
        {
            if (IsReadAssetBundle)
            {
                var rds = obj.GetComponentsInChildren<Renderer>();
                foreach (var rd in rds)
                {
                    if (rd != null)
                    {
                        foreach (var mat in rd.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                mat.shader = Shader.Find(mat.shader.name);
                            }
                        }
                    }
                }
            }
        }

#endif

        /// <summary>
        /// 加载资源。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>对象资源。</returns>
        private UnityEngine.Object LoadAsset(string path)
        {
            //Log.Info("LoadAsset:{0}", path);
            AssetInfo info;
            if (m_AssetInfos.TryGetValue(path, out info))
            {
                if (info.IsLoading)
                {
                    Log.Warning("The asset is loading in async. path:{0}", path);
                    return null;
                }
                info.AddCount();
                return info.Asset;
            }

            info = new AssetInfo(path, false, false);
            m_AssetInfos.Add(path, info);
            return info.Asset;
        }

        /// <summary>
        /// 加载资源。
        /// </summary>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <param name="path">资源路径。</param>
        /// <returns>对象资源。</returns>
        private T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return LoadAsset(path) as T;
        }

        /// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <param name="ext">资源后缀名。</param>
        /// <param name="on_finish">加载完成回调，若失败则为null。</param>
        private void LoadAssetAsync(string path, Action<UnityEngine.Object> on_finish)
        {
            Log.Info("LoadAssetAsync:{0}", path);
            AssetInfo info;
            if (!m_AssetInfos.TryGetValue(path, out info))
            {
                //开启一个异步加载
                info = new AssetInfo(path, false, true);
                m_AssetInfos.Add(path, info);
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
        /// 异步加载资源。
        /// </summary>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <param name="path">资源路径。</param>
        /// <param name="on_finish">加载完成回调，若失败则为null。</param>
        private void LoadAssetAsync<T>(string path, string ext, Action<T> on_finish) where T : UnityEngine.Object
        {
            Action<UnityEngine.Object> call = (asset) =>
            {
                if (on_finish != null)
                {
                    on_finish(asset as T);
                }
            };
            LoadAssetAsync(path, ext, call);
        }

        /// <summary>
        /// 加载资源。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>对象资源。</returns>
        private UnityEngine.Object[] LoadAllAsset(string path)
        {
            Log.Info("LoadAllAsset:{0}", path);
            AssetInfo info;
            if (m_AssetInfos.TryGetValue(path, out info))
            {
                if (info.IsLoading)
                {
                    Log.Warning("The asset is loading in async. path:{0}", path);
                    return null;
                }
                info.AddCount();
                return info.AllAssets;
            }

            info = new AssetInfo(path, true, false);
            m_AssetInfos.Add(path, info);
            return info.AllAssets;
        }

        /// <summary>
        /// 帧更新。
        /// </summary>
        private void Update()
        {
            //处理已经加载完成的资源
            m_AsynNeedRemove.Clear();
            for (int i = 0; i < m_AsynLoadList.Count; ++i)
            {
                //自身加载完毕和依赖的AB包也加载完毕
                AssetInfo info = m_AsynLoadList[i];
                info.CheckLoad();
                if (info.IsLoading)
                {
                    continue;
                }

                //判断是否已经无用了
                if (info.Count > 0)
                {
                    //触发回调
                    foreach (var call in info.OnFinish)
                    {
                        call(info.Asset);
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
            for (int i = m_AsynNeedRemove.Count - 1; i >= 0; --i)
            {
                m_AsynLoadList.RemoveAt(i);
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 是否读取AB包。
        /// </summary>
        private bool m_IsReadAssetBundle = false;

        /// <summary>
        /// 资源信息。
        /// </summary>
        private Dictionary<string, AssetInfo> m_AssetInfos = new Dictionary<string, AssetInfo>();

        /// <summary>
        /// 异步加载列表。
        /// </summary>
        private List<AssetInfo> m_AsynLoadList = new List<AssetInfo>();

        /// <summary>
        /// 需要从异步列表
        /// </summary>
        private List<int> m_AsynNeedRemove = new List<int>();

        #endregion
    }
}