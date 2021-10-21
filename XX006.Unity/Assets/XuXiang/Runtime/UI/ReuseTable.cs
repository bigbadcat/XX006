using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XuXiang;

namespace XuXiang
{
    /// <summary>
    /// 滑动列表单元格复用表格。PS:单元格和容器的锚点都要设置成(0,1)，对齐点为父节点的左上角。
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class ReuseTable : MonoBehaviourCache
    {
        /// <summary>
        /// 更新单元项委托。
        /// </summary>
        /// <param name="index">单元格索引。</param>
        /// <param name="item">单元格对象。</param>
        public delegate void UpdateItemDelegate(int index, RectTransform item);

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 滑动方向。
        /// </summary>
        public enum ScrollDirection
        {
            /// <summary>
            /// 水平。
            /// </summary>
            Horizontal,

            /// <summary>
            /// 竖直。
            /// </summary>
            Vertical
        }

        /// <summary>
        /// 添加更新监听者。
        /// </summary>
        /// <param name="listener">监听者。</param>
        public void AddItemUpdateListener(UpdateItemDelegate listener)
        {
            OnItemUpdate += listener;
        }

        /// <summary>
        /// 移除更新监听者。
        /// </summary>
        /// <param name="listener">监听者。</param>
        public void RemoveItemUpdateListener(UpdateItemDelegate listener)
        {
            OnItemUpdate -= listener;
        }

        /// <summary>
        /// 设置滚动位置。
        /// </summary>
        /// <param name="normalized">滚动比例。</param>
        public void SetNormalized(float normalized)
        {
            if (m_Direction == ScrollDirection.Horizontal)
            {
                ScrollRectUtil.SetHorizontalNormalized(m_ScrollView, normalized);
            }
            else if (m_Direction == ScrollDirection.Vertical)
            {
                ScrollRectUtil.SetVerticalNormalized(m_ScrollView, normalized);
            }
        }

        /// <summary>
        /// 重置滑动位置。水平滑动为最左，竖直滑动为最上。
        /// </summary>
        public void ResetPosition()
        {
            SetNormalized(m_Direction == ScrollDirection.Horizontal ? 0 : 1);
        }

        /// <summary>
        /// 调整滑动位置，将某项完整显示出来。
        /// </summary>
        /// <param name="index">数据线索引。</param>
        public void FoucsOnItem(int index)
        {
            if (index <0 || index>=m_Count)
            {
                return;
            }

            //只有在数量超过一页时才有调整的需要
            int show_line = (int)(m_Direction == ScrollDirection.Horizontal ? (m_CurViewSize.x - m_Space * 2) / m_CellSize.x : (m_CurViewSize.y - m_Space * 2) / m_CellSize.y);
            int line = (int)Math.Ceiling(m_Count * 1.0f / m_BasicNumber);
            if (show_line >= line)
            {
                return;
            }

            //根据聚焦位置，计算滑动位置
            int foucs_line = index / m_BasicNumber;
            float r = foucs_line * 1.0f / (line - show_line);
            r = Mathf.Clamp01(r);
            if (m_Direction == ScrollDirection.Vertical)
            {
                r = 1 - r;
            }
            SetNormalized(r);
        }

        /// <summary>
        /// 设置单元格数量。
        /// </summary>
        /// <param name="n">单元格数量。</param>
        /// <param name="resetpos">是否重置位置。</param>
        public void SetCount(int n, bool resetpos)
        {
            //设置容器尺寸                    
            int line = (int)Math.Ceiling(n * 1.0f / m_BasicNumber);
            RectTransform content = m_ScrollView.content;
            Vector2 oldpos = content.anchoredPosition;
            Vector2 vs = content.sizeDelta;
            Vector2 viewsize = m_ScrollView.viewport.rect.size;
            m_Count = 0;            //设置滑动位置时会触发ScrollView的滚动事件，先将Count设置成0已屏蔽Update回调
            if (m_Direction == ScrollDirection.Horizontal)
            {
                vs.x = Mathf.Max(line * m_CellSize.x + m_Space * 2, viewsize.x);
                content.anchoredPosition = new Vector2(resetpos ? 0 : -Math.Min(vs.x - viewsize.x, -oldpos.x), oldpos.y);
            }
            else if (m_Direction == ScrollDirection.Vertical)
            {
                vs.y = Mathf.Max(line * m_CellSize.y + m_Space * 2, viewsize.y);
                content.anchoredPosition = new Vector2(oldpos.x, resetpos ? 0 : Math.Min(vs.y - viewsize.y, oldpos.y));
            }
            content.sizeDelta = vs;

            //刷新格子
            m_Count = n;
            m_FirstIndex = GetCurStartIndex();
            for (int i = 0; i < m_CellItems.Count; ++i)
            {
                RectTransform rt = m_CellItems[i];
                int index = m_FirstIndex + i;
                rt.gameObject.SetActive(index < m_Count);
                rt.anchoredPosition3D = GetPosition(index);
            }
            RefreshAllItem();
        }

        /// <summary>
        /// 刷新所有格子。
        /// </summary>
        public void RefreshAllItem()
        {
            for (int i = 0; i < m_CellItems.Count; ++i)
            {
                UpdateItem(m_FirstIndex + i, m_CellItems[i]);
            }
        }

        /// <summary>
        /// 刷新部分格子。
        /// </summary>
        /// <param name="index">刷新的起始索引。</param>
        public void RefreshPartItem(int index)
        {
            int i = Math.Max(0, index - m_FirstIndex);
            for (; i < m_CellItems.Count; ++i)
            {
                UpdateItem(m_FirstIndex + i, m_CellItems[i]);
            }
        }

        /// <summary>
        /// 刷新某个格子。
        /// </summary>
        /// <param name="index">格子索引。</param>
        public void RefreshItem(int index)
        {
            int i = index - m_FirstIndex;
            if (i < 0 || i >= m_CellItems.Count)
            {
                return;
            }
            UpdateItem(index, m_CellItems[i]);
        }

        /// <summary>
        /// 当前尺寸发生改变时调用。
        /// </summary>
        public void OnSizeChanged()
        {
            Vector2 vs = m_ScrollView.viewport.rect.size;
            if ((int)m_CurViewSize.x != (int)vs.x || (int)m_CurViewSize.y != (int)vs.y)
            {
                InitGridCache();
                SetCount(m_Count, false);
            }            
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 单元格更新事件。
        /// </summary>
        public event UpdateItemDelegate OnItemUpdate;

        /// <summary>
        /// 获取表格单元格数量。
        /// </summary>
        public int Count
        {
            get { return m_Count; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 唤醒。
        /// </summary>
        protected void Awake()
        {
            m_ScrollView.horizontal = m_Direction == ScrollDirection.Horizontal;
            m_ScrollView.vertical = m_Direction == ScrollDirection.Vertical;
            m_ScrollView.onValueChanged.AddListener(OnScrollViewChanged);
            m_ScrollView.content.ClearChild(true);
            m_CellItems.Clear();
            InitGridCache();
            if (m_Count > 0)
            {
                SetCount(m_Count, false);
            }

            //禁用辅助编辑的布局组建
            GridLayoutGroup glg = m_ScrollView.content.GetComponent<GridLayoutGroup>();
            if (glg != null)
            {
                glg.enabled = false;
            }
        }

        /// <summary>
        /// UI适配后尺寸会变。
        /// </summary>
        private void Start()
        {
            OnSizeChanged();
        }

        /// <summary>
        /// 滑动区域改变。
        /// </summary>
        /// <param name="v"></param>
        private void OnScrollViewChanged(Vector2 v)
        {
            if (m_Count <= 0)
            {
                return;
            }
            OnScroll();
        }

        /// <summary>
        /// 初始化格子缓存。
        /// </summary>
        private void InitGridCache()
        {
            //计算出缓存数量
            m_CurViewSize = m_ScrollView.viewport.rect.size;
            int line = (int)(m_Direction == ScrollDirection.Horizontal ? (m_CurViewSize.x - m_Space * 2) / m_CellSize.x : (m_CurViewSize.y - m_Space * 2) / m_CellSize.y);
            int n = m_BasicNumber * (line + 2);

            //不足则创建
            for (int i = m_CellItems.Count; i < n; ++i)
            {
                GameObject obj = Instantiate(m_ItemPrefab) as GameObject;
                RectTransform rt = obj.GetComponent<RectTransform>();
                obj.SetActive(false);
                m_ScrollView.content.AddChild(rt);
                m_CellItems.Add(rt);
            }

            //多余则删除
            while (m_CellItems.Count > n)
            {
                RectTransform rt = m_CellItems[m_CellItems.Count - 1];
                m_CellItems.RemoveAt(m_CellItems.Count - 1);
                Destroy(rt.gameObject);
            }
        }

        /// <summary>
        /// 发生滚动。
        /// </summary>
        private void OnScroll()
        {
            int startindex = GetCurStartIndex();
            if (m_FirstIndex < startindex)
            {
                OnScrollForward(startindex);
            }
            else if (m_FirstIndex > startindex)
            {
                OnScrollBackward(startindex);
            }
        }

        /// <summary>
        /// 获取当前起始索引。
        /// </summary>
        /// <returns>当前起始索引。</returns>
        private int GetCurStartIndex()
        {
            if (m_Count <= 0)
            {
                return 0;
            }

            int line = 0;
            if (m_Direction == ScrollDirection.Horizontal)
            {
                float x = -m_ScrollView.content.anchoredPosition.x - m_Space;
                line = (int)(x / m_CellSize.x);
            }
            else if (m_Direction == ScrollDirection.Vertical)
            {
                float y = m_ScrollView.content.anchoredPosition.y - m_Space;
                line = (int)(y / m_CellSize.y);
            }
            int index = Math.Max(0, line) * m_BasicNumber;
            return Mathf.Clamp(index, 0, m_Count - 1);
        }

        /// <summary>
        /// 向序号大的方向滚动。
        /// </summary>
        /// <param name="startindex">新的起始索引。</param>
        private void OnScrollForward(int startindex)
        {
            int n = Math.Min(m_CellItems.Count, startindex - m_FirstIndex);
            List<RectTransform> items = m_CellItems;
            m_CellItems = new List<RectTransform>();
            for (int i = n; i < items.Count; ++i)
            {
                m_CellItems.Add(items[i]);
            }

            int updateindex = startindex + (items.Count - n);
            for (int i = 0; i < n; ++i)
            {
                RectTransform rt = items[i];
                m_CellItems.Add(rt);

                //更新位置和刷新信息
                int ii = updateindex + i;
                rt.anchoredPosition = GetPosition(ii);
                rt.gameObject.SetActive(ii < m_Count);
                if (ii < m_Count)
                {
                    UpdateItem(ii, rt);
                }
            }
            m_FirstIndex = startindex;
        }

        /// <summary>
        /// 向序号小的方向滚动。
        /// </summary>
        /// <param name="startindex">新的起始索引。</param>
        private void OnScrollBackward(int startindex)
        {
            int n = Math.Min(m_CellItems.Count, m_FirstIndex - startindex);             //要移动的数量
            int ln = m_CellItems.Count - n;                                         //剩余不需要移动的数量
            List<RectTransform> items = m_CellItems;
            m_CellItems = new List<RectTransform>();

            for (int i = 0; i < n; ++i)
            {
                RectTransform rt = items[ln + i];
                m_CellItems.Add(rt);

                //更新位置和刷新信息
                int ii = startindex + i;
                rt.anchoredPosition = GetPosition(ii);
                rt.gameObject.SetActive(true);
                UpdateItem(ii, rt);
            }

            for (int i = 0; i < ln; ++i)
            {
                m_CellItems.Add(items[i]);
            }
            m_FirstIndex = startindex;
        }

        /// <summary>
        /// 更新某个格子。
        /// </summary>
        /// <param name="index">格子索引。</param>
        /// <param name="item">格子对象。</param>
        private void UpdateItem(int index, RectTransform item)
        {
            if (OnItemUpdate != null && index < m_Count)
            {
                OnItemUpdate(index, item);
            }
        }

        /// <summary>
        /// 获取单元格的位置。
        /// </summary>
        /// <param name="index">单元格索引。</param>
        /// <returns>单元格的位置。</returns>
        private Vector2 GetPosition(int index)
        {
            Vector2 v = Vector2.zero;
            if (m_Direction == ScrollDirection.Horizontal)
            {
                v.x = (index / m_BasicNumber) * m_CellSize.x + m_Space;
                v.y = -(index % m_BasicNumber) * m_CellSize.y;
            }
            else if (m_Direction == ScrollDirection.Vertical)
            {
                v.x = (index % m_BasicNumber) * m_CellSize.x;
                v.y = -((index / m_BasicNumber) * m_CellSize.y + m_Space);
            }            
            return v;
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 单元格原型。
        /// </summary>
        [SerializeField]
        private GameObject m_ItemPrefab = null;

        /// <summary>
        /// 滚动视图。
        /// </summary>
        [SerializeField]
        private ScrollRect m_ScrollView = null;

        /// <summary>
        /// 滑动方向。
        /// </summary>
        [SerializeField]
        private ScrollDirection m_Direction = ScrollDirection.Horizontal;

        /// <summary>
        /// 边缘预留多大尺寸。
        /// </summary>
        [SerializeField]
        private float m_Space = 0;

        /// <summary>
        /// 摆放基数。
        /// </summary>
        [SerializeField]
        private int m_BasicNumber = 1;

        /// <summary>
        /// 单元格尺寸。
        /// </summary>
        [SerializeField]
        private Vector2 m_CellSize = new Vector2(50, 50);

        /// <summary>
        /// 当前的窗口大小。
        /// </summary>
        private Vector2 m_CurViewSize;

        /// <summary>
        /// 单元格数量。
        /// </summary>
        private int m_Count;

        /// <summary>
        /// 单元格的起始索引。
        /// </summary>
        private int m_FirstIndex;

        /// <summary>
        /// 单元格列表。
        /// </summary>
        private List<RectTransform> m_CellItems = new List<RectTransform>();

        #endregion
    }
}