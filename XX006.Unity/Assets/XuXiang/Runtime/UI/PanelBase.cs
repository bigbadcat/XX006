using System;
using UnityEngine;
using System.Collections;

namespace XuXiang
{
    /// <summary>
    /// UI面板基类。
    /// </summary>
    public abstract class PanelBase : MonoBehaviourCache
    {
        #region 对外操作----------------------------------------------------------------
        
        /// <summary>
        /// 关闭自身UI。
        /// </summary>
        /// <param name="clearcache">是否从缓存中清除。</param>
        public void Close(bool clearcache = false)
        {
            m_Manager?.Close(this.m_Code);
        }

        /// <summary>
        /// UI被加载时。
        /// </summary>
        /// <param name="param">加载的参数。</param>
        public virtual void OnLoad(string param)
        {
        }

        /// <summary>
        /// 当UI被打开时。
        /// </summary>
        /// <param name="param">打开参数。</param>
        public virtual void OnOpen(object param)
        {
        }

        /// <summary>
        /// 当UI被关闭时。
        /// </summary>
        public virtual void OnClose()
        {
        }

        /// <summary>
        /// 放回按键处理。
        /// </summary>
        /// <returns>是否继续传递给下一个UI处理。</returns>
        public virtual bool OnKeyBack()
        {
            Close();
            return false;
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// UI画布。
        /// </summary>
        public Canvas Canvas
        {
            get
            {
                if (m_Canvas == null)
                {
                    m_Canvas = this.CacheGameObject.GetComponent<Canvas>();
                }
                return m_Canvas;
            }
        }

        /// <summary>
        /// 获取或设置UI编号。
        /// </summary>
        public int Code
        {
            get { return m_Code; }
            set { m_Code = value; }
        }

        /// <summary>
        /// 获取或设置是否为全屏UI。
        /// </summary>
        public bool IsFull
        {
            get { return m_Full; }
            set { m_Full = value; }
        }

        /// <summary>
        /// 获取或设置管理器。
        /// </summary>
        public PanelManager Manager
        {
            get { return m_Manager; }
            set { m_Manager = value; }
        }

        /// <summary>
        /// 获取UI是否为显示状态。
        /// </summary>
        public bool IsShow
        {
            get
            {
                return CacheGameObject.activeSelf;
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 唤醒。
        /// </summary>
        protected virtual void Awake()
        {
            m_Canvas = this.CacheGameObject.GetComponent<Canvas>();
            if (m_Canvas == null)
            {
                Log.Warning("Not found Canvas component at ui. code:{0}", m_Code);
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 画布。
        /// </summary>
        private Canvas m_Canvas;

        /// <summary>
        /// UI编号。
        /// </summary>
        private int m_Code;

        /// <summary>
        /// 是否全屏UI。
        /// </summary>
        private bool m_Full;

        /// <summary>
        /// 管理器。
        /// </summary>
        private PanelManager m_Manager = null;

        #endregion
    }
}