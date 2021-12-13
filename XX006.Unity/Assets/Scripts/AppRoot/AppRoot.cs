using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 应用启动根节点。
    /// </summary>
    public class AppRoot : MonoBehaviourCache
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 退出游戏
        /// </summary>
        public static void QuitGame()
        {
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;
            }
            Application.Quit();
        }

        /// <summary>
        /// 重启游戏。
        /// </summary>
        public static void RestartGame()
        {
            //目前只支持退出
            QuitGame();
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 是否已经运行起来了。
        /// </summary>
        public static bool IsRun { get; private set; }

        /// <summary>
        /// 获取UI根节点对象。
        /// </summary>
        public static PanelManager UIRoot 
        { 
            get
            {
                if (s_UIRoot == null)
                {
                    s_UIRoot = ResourceManager.Instance.LoadObject<PanelManager>("AppRes/UIRoot");
                    s_UIRoot.transform.localPosition = new Vector3(0, 50, 0);
                    DontDestroyOnLoad(s_UIRoot.gameObject);
                }

                return s_UIRoot;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 初始化。
        /// </summary>
        private void Start()
        {

#if UNITY_EDITOR
            ResourceManager.Instance.IsReadAssetBundle = m_IsPackMode;
#else
            ResourceManager.Instance.IsReadAssetBundle = true;      //非编辑器只能读取AB包
            Application.targetFrameRate = 60;
#endif

            //预加载字体
            IsRun = true;
            if (ResourceManager.Instance.IsReadAssetBundle)
            {
                AssetBundleManager.Instance.LoadDepend();
                ResourceManager.Instance.InitShaders("shaders/shader");
            }

            //切换场景
            string name = "BigWorld";
            if (ResourceManager.Instance.IsReadAssetBundle)
            {
                string ab_file = "scene/" + name.ToLower();
                AssetBundleManager.Instance.LoadAssetBundle(ab_file);
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(name);
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

#if UNITY_EDITOR

        /// <summary>
        /// 是否为打包模式。
        /// </summary>
        [SerializeField]
        private bool m_IsPackMode = false;

#endif


        /// <summary>
        /// UI面板管理。
        /// </summary>
        private static PanelManager s_UIRoot = null;

        #endregion
    }
}