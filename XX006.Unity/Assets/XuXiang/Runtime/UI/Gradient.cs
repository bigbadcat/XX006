using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 文本渐变效果。
    /// </summary>
    [AddComponentMenu("UI/Effects/Gradient")]
    public class Gradient : BaseMeshEffect
    {
        #region 对外操作----------------------------------------------------------------
                
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive())
            {
                return;
            }

            var vertexList = new List<UIVertex>();
            vh.GetUIVertexStream(vertexList);
            ApplyGradient(vertexList, 0, vertexList.Count);
            vh.Clear();
            vh.AddUIVertexTriangleStream(vertexList);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置顶部的颜色。
        /// </summary>
        public Color32 TopColor
        {
            get { return m_TopColor; }
            set
            {
                m_TopColor = value;
                graphic.SetVerticesDirty();
            }
        }

        /// <summary>
        /// 获取或设置底部的颜色。
        /// </summary>
        public Color32 BottomColor
        {
            get { return m_BottomColor; }
            set
            {
                m_BottomColor = value;
                graphic.SetVerticesDirty();
            }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 修改Mesh数据。
        /// </summary>
        /// <param name="vertexList">顶点数据列表。</param>
        /// <param name="start">要修改的起始索引。(包含)</param>
        /// <param name="end">要修改的结束索引。(不包含)</param>
        private void ApplyGradient(List<UIVertex> vertexList, int start, int end)
        {
            if (vertexList.Count <= 0)
            {
                return;
            }

#if UNITY_EDITOR
            if (start < 0 || start >= vertexList.Count)
            {
                Log.Error("Wrong start index {0}", start);
                return;
            }
            if (end <= 0 || end > vertexList.Count)
            {
                Log.Error("Wrong end index {0}", end);
                return;
            }
#endif

            //算出最高和最低的Y值
            float bottomY = vertexList[start].position.y;
            float topY = vertexList[start].position.y;
            for (int i = start; i < end; ++i)
            {
                float y = vertexList[i].position.y;
                if (y > topY)
                {
                    topY = y;
                }
                else if (y < bottomY)
                {
                    bottomY = y;
                }
            }

            //根据每个顶点的Y值在最高和最低的范围内对颜色进行差值
            float uiElementHeight = topY - bottomY;
            for (int i = start; i < end; ++i)
            {
                UIVertex uiVertex = vertexList[i];
                uiVertex.color = Color32.Lerp(m_BottomColor, m_TopColor, (uiVertex.position.y - bottomY) / uiElementHeight);
                vertexList[i] = uiVertex;
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 顶部的颜色。
        /// </summary>
        [SerializeField]
        private Color32 m_TopColor = Color.white;

        /// <summary>
        /// 底部的颜色。
        /// </summary>
        [SerializeField]
        private Color32 m_BottomColor = Color.black;

        #endregion
    }
}