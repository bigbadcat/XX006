using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 资源标记。
    /// </summary>
    public class ResourceMark : MonoBehaviourCache
    {
        #region 对外操作----------------------------------------------------------------

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置资源路径。
        /// </summary>
        public string ResPath
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        private void Awake()
        {
            //复制到的时候，m_Path已经被赋值，需要对引用计数加1。 
            if (string.IsNullOrEmpty(m_Path))
            {
                ResourceManager.Instance.AddAssetCount(m_Path);
            }
        }

        private void OnDestroy()
        {
            ResourceManager mgr = ResourceManager.Instance;
            if (mgr != null)
            {
                mgr.UnloadAsset(m_Path);
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 资源路径。
        /// </summary>
        [SerializeField]
        private string m_Path = string.Empty;

        #endregion
    }
}