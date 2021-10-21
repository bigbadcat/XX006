using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 基于MonoBehaviour的单实例基类。
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviourCache where T : Singleton<T>
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 手动初始化。
        /// </summary>
        public virtual void ManualInit() { }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取单实例。
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    //程序已退出则返回空
                    if (_quit)
                    {
                        return null;
                    }

                    //查找挂接对象
                    GameObject go = GameObject.Find("Singleton");
                    if (go == null)
                    {
                        go = new GameObject("Singleton");                        
                    }
                    _instance = go.AddComponent<T>();
                }

                return _instance;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 唤醒。
        /// </summary>
        protected void Awake()
        {
            DontDestroyOnLoad(CacheGameObject);
            if (_instance != null)
            {
                Log.Warning("The Singleton type of {0} is already exists.", typeof(T));
            }
            _instance = this as T;
            _instance.Init();
        }

        /// <summary>
        /// 程序退出。
        /// </summary>
        private void OnApplicationQuit()
        {
            _quit = true;
            Release();
        }

        /// <summary>
        /// 初始化。
        /// </summary>
        protected virtual void Init() { }

        /// <summary>
        /// 释放。
        /// </summary>
        protected virtual void Release() { }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 单实例对象。
        /// </summary>
        private static T _instance = null;

        /// <summary>
        /// 标记程序是否已退出。
        /// </summary>
        private static bool _quit = false;

        #endregion
    }
}