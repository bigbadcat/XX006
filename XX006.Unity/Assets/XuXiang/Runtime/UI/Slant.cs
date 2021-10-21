using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 倾斜效果。
    /// </summary>
    [AddComponentMenu("UI/Effects/Slant")]
    public class Slant : BaseMeshEffect
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 修改网格。
        /// </summary>
        /// <param name="vh">顶点数据访问接口。</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }

            List<UIVertex> vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);
            int count = vertexList.Count;
            for (int i = 0; i < vertexList.Count; ++i)
            {
                UIVertex v = vertexList[i];
                Vector3 p = v.position;
                p.x += p.y * m_Ratio;     //X按Y高度进行偏移
                v.position = p;
                vertexList[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertexList);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置倾斜度。
        /// </summary>
        public float Ratio
        {
            get { return m_Ratio; }
            set
            {
                m_Ratio = value;
                graphic.SetVerticesDirty();
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 倾斜程度。
        /// </summary>
        [SerializeField]
        private float m_Ratio = 1;

        #endregion
    }
}