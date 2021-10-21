using UnityEngine;
using System.Collections;

namespace XuXiang
{
    /// <summary>
    /// 带有部分常用对象缓存的基类。
    /// </summary>
    public class MonoBehaviourCache : MonoBehaviour
    {
        #region 对外操作----------------------------------------------------------------
        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 通过缓存获取GameObject。
        /// </summary>
        public GameObject CacheGameObject
        {
            get
            {
                if (m_CacheGameObject == null && this != null)
                {
                    m_CacheGameObject = this.gameObject;
                }
                return m_CacheGameObject;
            }
        }

        /// <summary>
        /// 通过缓存获取Transform。
        /// </summary>
        public Transform CacheTransform
        {
            get
            {
                if (m_CacheTransform == null && this != null)
                {
                    m_CacheTransform = this.transform;
                }
                return m_CacheTransform;
            }
        }

        /// <summary>
        /// 通过缓存获取RectTransform，非UI对象为null。
        /// </summary>
        public RectTransform CacheRectTransform
        {
            get
            {
                if (m_CacheRectTransform == null && this != null)
                {
                    m_CacheRectTransform = this.GetComponent<RectTransform>();
                }
                return m_CacheRectTransform;
            }
        }
              
        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// GameObject缓存。
        /// </summary>
        private GameObject m_CacheGameObject;

        /// <summary>
        /// Transform缓存。
        /// </summary>
        private Transform m_CacheTransform;

        /// <summary>
        /// RectTransform。
        /// </summary>
        private RectTransform m_CacheRectTransform;
              
        #endregion
    }
}
