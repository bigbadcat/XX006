using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuXiang
{
    /// <summary>
    /// 常驻信息页面。
    /// </summary>
    public class UILongCanvas : MonoBehaviourCache
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 层次类型。
        /// </summary>
        public enum LayerType
        {
            /// <summary>
            /// 底层。
            /// </summary>
            Bottom,

            /// <summary>
            /// 中层。
            /// </summary>
            Middle,

            /// <summary>
            /// 顶层。
            /// </summary>
            Top,
        }

        #endregion

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 添加对象。
        /// </summary>
        /// <param name="path">对象原型路径。</param>
        /// <param name="layer">添加到的层级。</param>
        /// <returns>被添加的对象。</returns>
        public GameObject AddObject(string path, LayerType layer = LayerType.Middle)
        {
            //加载
            GameObject obj = ResourceManager.Instance.LoadObject(path) as GameObject;
            if (obj == null)
            {
                Log.Error("Can not load object. path:{0}", path);
                return null;
            }
            RectTransform rt = obj.GetComponent<RectTransform>();
            if (rt == null)
            {
                Log.Error("The object can not found RectTransform:{0}", path);
                return obj;
            }

            AddObject(rt, layer);
            return obj;
        }

        /// <summary>
        /// 添加对象。
        /// </summary>
        /// <param name="rt">要添加的对象。</param>
        /// <param name="layer">添加到的层级。</param>
        public void AddObject(RectTransform rt, LayerType layer = LayerType.Middle)
        {
            RectTransform parent = layer == LayerType.Bottom ? m_BottomLayer : (layer == LayerType.Middle ? m_MiddleLayer : m_TopLayer);
            rt.SetParent(parent, false);
            rt.Reset();
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
        /// 获取底部层。
        /// </summary>
        public RectTransform BottomLayer
        {
            get { return m_BottomLayer; }
        }

        /// <summary>
        /// 获取中间层。
        /// </summary>
        public RectTransform MiddleLayer
        {
            get { return m_MiddleLayer; }
        }

        /// <summary>
        /// 获取顶部层。
        /// </summary>
        public RectTransform TopLayer
        {
            get { return m_TopLayer; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 画布。
        /// </summary>
        private Canvas m_Canvas;

        /// <summary>
        /// 底部层。
        /// </summary>
        [SerializeField]
        private RectTransform m_BottomLayer;

        /// <summary>
        /// 中间层。
        /// </summary>
        [SerializeField]
        private RectTransform m_MiddleLayer;

        /// <summary>
        /// 顶部层。
        /// </summary>
        [SerializeField]
        private RectTransform m_TopLayer;

        #endregion
    }
}