using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace XuXiang
{
    /// <summary>
    /// 用于触摸接收触摸操作。(编辑器中使用UI事件实现操作，移动设备通过在Update中检查Touch数据实现)
    /// </summary>
    public class TouchReceiver : MonoBehaviour
    {
        public interface ITouchProcesser
        {
            /// <summary>
            /// 旋转操作。
            /// </summary>
            /// <param name="dx">X屏幕移动量。</param>
            /// <param name="dy">Y屏幕移动量。</param>
            void OnRotate(float dx, float dy);

            /// <summary>
            /// 移动操作。
            /// </summary>
            /// <param name="dx">X屏幕移动量。</param>
            /// <param name="dy">Y屏幕移动量。</param>
            void OnMove(float dx, float dy);

            /// <summary>
            /// 缩放操作。
            /// </summary>
            /// <param name="d">屏幕移动量。</param>
            void OnScale(float d);

            /// <summary>
            /// 点击。
            /// </summary>
            /// <param name="x">点击X坐标。</param>
            /// <param name="y">点击Y坐标。</param>
            void OnClick(float x, float y);
        }

        /// <summary>
        /// 触摸信息。
        /// </summary>
        public class TouchInfo
        {
            public TouchInfo()
            {
                pointerId = 0;
                cache = false;
            }

            public int pointerId;
            public Vector2 delta;
            public Vector2 position;

            public override string ToString()
            {
                return string.Format("pid:{0} pos:{1} delta:{2}", pointerId, position, delta);
            }

            private bool cache;

            public static TouchInfo Get()
            {
                TouchInfo info = s_CacheInfo.Count > 0 ? s_CacheInfo.Pop() : new TouchInfo();
                info.cache = false;
                return info;
            }

            public static void Recycle(TouchInfo info)
            {
                if (info.cache || s_CacheInfo.Count >= MaxCache)
                {
                    return;
                }

                info.pointerId = 0;
                info.delta = Vector2.zero;
                info.position = Vector2.zero;
                info.cache = true;
                s_CacheInfo.Push(info);
            }

            public static int MaxCache = 10;

            private static Stack<TouchInfo> s_CacheInfo = new Stack<TouchInfo>();
        }

        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 触摸按下。
        /// </summary>
        /// <param name="data">触摸数据。</param>
        public void OnPointDown(BaseEventData data)
        {
            PointerEventData ped = data as PointerEventData;
            m_CheckClick = true;
            m_TouchPosition = ped.position;

            //m_TouchDown = true;
            m_OldDistance = 0;

            if (!m_TouchInfos.ContainsKey(ped.pointerId))
            {
                TouchInfo info = TouchInfo.Get();
                info.pointerId = ped.pointerId;
                info.position = ped.position;
                m_TouchInfos.Add(info.pointerId, info);
                //UpdateTouchInfo();
            }
        }

        //void UpdateTouchInfo()
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.AppendFormat("TouchCount:{0}", m_TouchInfos.Count);
            
        //    foreach (var kvp in m_TouchInfos)
        //    {
        //        sb.AppendLine();
        //        sb.Append(kvp.Value.ToString());
        //    }
        //    InfoText.text = sb.ToString();
        //}

        /// <summary>
        /// 触摸拖拽。
        /// </summary>
        /// <param name="data">触摸数据。</param>
        public void OnDrag(BaseEventData data)
        {
            //取消点击的判断距离
            PointerEventData ped = data as PointerEventData;
            TouchInfo info;
            if (m_TouchInfos.TryGetValue(ped.pointerId, out info))
            {
                info.position = ped.position;
                info.delta = ped.delta;
                //UpdateTouchInfo();
            }

            if (m_CheckClick)
            {
                if (m_TouchInfos.Count >= 2 || (ped.position - m_TouchPosition).sqrMagnitude >= CancelClickDistance * CancelClickDistance)
                {
                    m_CheckClick = false;
                }
            }

            if (m_TouchInfos.Count == 1)
            {
                foreach (var kvp in m_TouchInfos)
                {
                    CheckSingle(kvp.Value);
                }
            }
            else if (m_TouchInfos.Count == 2)
            {
                TouchInfo info1 = null;
                TouchInfo info2 = null;
                foreach (var kvp in m_TouchInfos)
                {
                    if (info1 == null)
                    {
                        info1 = kvp.Value;
                    }
                    else
                    {
                        info2 = kvp.Value;
                    }                    
                }
                CheckTwo(info1, info2);
            }   
            else
            {
                m_OldDistance = 0;
            }
        }

        /// <summary>
        /// 触摸弹起。
        /// </summary>
        /// <param name="data">触摸数据。</param>
        public void OnPointUp(BaseEventData data)
        {
            PointerEventData ped = data as PointerEventData;
            if (m_CheckClick)
            {
                m_Processer?.OnClick(ped.position.x, ped.position.y);
                m_CheckClick = false;
            }
            //m_TouchDown = false;

            TouchInfo info;
            if (m_TouchInfos.TryGetValue(ped.pointerId, out info))
            {
                m_TouchInfos.Remove(ped.pointerId);
                TouchInfo.Recycle(info);
                info = null;
                //UpdateTouchInfo();
            }
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        //public TMPro.TextMeshProUGUI InfoText;

        /// <summary>
        /// 旋转系数。
        /// </summary>
        public float RotateRatio = 0.25f;

        /// <summary>
        /// 移动系数。
        /// </summary>
        public float MoveRatio = 0.05f;

        /// <summary>
        /// 缩放系数。
        /// </summary>
        public float ScaleRatio = 0.1f;

        /// <summary>
        /// 触摸处理者。
        /// </summary>
        public ITouchProcesser Processer
        {
            get { return m_Processer; }
            set { m_Processer = value; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 检查单个手指的触摸情况。
        /// </summary>
        /// <param name="touch">触摸数据。</param>
        private void CheckSingle(TouchInfo touch)
        {
            if (!m_CheckClick)
            {
                Vector2 delta = touch.delta;
#if UNITY_EDITOR
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    //缩放
                    float d = (delta.x + delta.y) / 2;
                    Scale(d);
                }
                else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //平移                
                    Move(delta.x, delta.y);
                }
                else
                {
                    //旋转
                    Rotate(delta.x, delta.y);
                }
#else
                //旋转
                Rotate(delta.x, delta.y);
#endif
            }
        }

        /// <summary>
        /// 检查两个手指的触摸情况。
        /// </summary>
        /// <param name="touch1">手指1触摸数据。</param>
        /// <param name="touch2">手指2触摸数据。</param>
        private void CheckTwo(TouchInfo touch1, TouchInfo touch2)
        {
            float dis = Vector2.Distance(touch1.position, touch2.position);
            if (m_OldDistance <= 0)
            {
                m_OldDistance = dis;
            }
            else
            {
                //根据移动夹角判断操作
                if (Vector2.Angle(touch1.delta, touch2.delta) < 90)
                {
                    //两点的移动量控制平移
                    Vector2 deltamove = (touch1.position + touch2.position) / 2;
                    Move(deltamove.x, deltamove.y);
                    m_OldDistance = dis;
                }        
                else
                {
                    //两点距离控制缩放
                    float deltadis = dis - m_OldDistance;
                    Scale(deltadis);
                    m_OldDistance = dis;
                }
            }
        }

        /// <summary>
        /// 旋转操作。
        /// </summary>
        /// <param name="dx">X屏幕移动量。</param>
        /// <param name="dy">Y屏幕移动量。</param>
        private void Rotate(float dx, float dy)
        {
            m_Processer?.OnRotate(dx * RotateRatio, dy * RotateRatio);
        }

        /// <summary>
        /// 移动操作。
        /// </summary>
        /// <param name="dx">X屏幕移动量。</param>
        /// <param name="dy">Y屏幕移动量。</param>
        private void Move(float dx, float dy)
        {
            m_Processer?.OnMove(dx * MoveRatio, dy * MoveRatio);
        }

        /// <summary>
        /// 缩放操作。
        /// </summary>
        /// <param name="d">屏幕移动量。</param>
        private void Scale(float d)
        {
            m_Processer?.OnScale(d * ScaleRatio);
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 取消点击的距离。
        /// </summary>
        private static float CancelClickDistance = 25;

        /// <summary>
        /// 缩放操作的两点距离。
        /// </summary>
        private float m_OldDistance = 0;
        
        ///// <summary>
        ///// 是否按下，用于只检测区域内的Touch。
        ///// </summary>
        //private bool m_TouchDown = false;

        /// <summary>
        /// 按下位置。
        /// </summary>
        private Vector2 m_TouchPosition;

        /// <summary>
        /// 是否检测点击。
        /// </summary>
        private bool m_CheckClick = false;

        /// <summary>
        /// 触摸处理者。
        /// </summary>
        private ITouchProcesser m_Processer = null;

        /// <summary>
        /// 当前触摸的信息。
        /// </summary>
        private Dictionary<int, TouchInfo> m_TouchInfos = new Dictionary<int, TouchInfo>();

        #endregion
    }
}