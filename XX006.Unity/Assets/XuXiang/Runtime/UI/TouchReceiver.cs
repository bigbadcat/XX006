using System;
using System.Collections;
using System.Collections.Generic;
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
            m_TouchDownCount = LongPressTime;
            m_MoveClick = false;
#if !UNITY_EDITOR
            m_TouchDown = true;
            m_OldDistance = 0;
#endif
        }

        /// <summary>
        /// 触摸拖拽。
        /// </summary>
        /// <param name="data">触摸数据。</param>
        public void OnDrag(BaseEventData data)
        {
            //取消点击的判断距离
            PointerEventData ped = data as PointerEventData;
            if (m_CheckClick)
            {
                if ((ped.position - m_TouchPosition).sqrMagnitude >= CancelClickDistance * CancelClickDistance)
                {
                    m_CheckClick = false;
                    m_TouchDownCount = 0;
                    m_MoveClick = false;
                }
            }
            else if (m_MoveClick)
            {
                //GameManager.Instance.Click(ped.position.x, ped.position.y);
            }

#if UNITY_EDITOR

            if (!m_CheckClick && !m_MoveClick)
            {
                Vector2 delta = ped.delta;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    //缩放
                    float d = (delta.x + delta.y) / 2;
                    Scale(d);
                }
                //else if (Input.GetKey(KeyCode.D))
                //{
                //    //平移                
                //    Move(delta.x, delta.y);
                //}
                else
                {
                    //旋转
                    Rotate(delta.x, delta.y);
                }
            }
#endif
        }

        /// <summary>
        /// 触摸弹起。
        /// </summary>
        /// <param name="data">触摸数据。</param>
        public void OnPointUp(BaseEventData data)
        {
            PointerEventData ped = data as PointerEventData;
            if (m_CheckClick)           //游戏模式才可以点方块
            {
                //GameManager.Instance.Click(ped.position.x, ped.position.y);
                m_Processer?.OnClick(ped.position.x, ped.position.y);
                m_CheckClick = false;
            }
            if (m_MoveClick)
            {
                m_MoveClick = false;
            }
#if !UNITY_EDITOR
            m_TouchDown = false;
#endif
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 长按时间。
        /// </summary>
        public float LongPressTime = 0.5f;

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
        /// 帧更新。
        /// </summary>
        public void Update()
        {
            if (m_TouchDownCount > 0)
            {
                m_TouchDownCount -= Time.deltaTime;
                if (m_TouchDownCount <= 0)
                {
                    m_CheckClick = false;
                    m_MoveClick = true;
                    m_TouchDownCount = 0;
                    //GameManager.Instance.Click(m_TouchPosition.x, m_TouchPosition.y);
                }
            }

#if !UNITY_EDITOR
            if (m_TouchDown)
            {
                CheckTouch();
            }            
#endif
        }

#if !UNITY_EDITOR

        /// <summary>
        /// 触摸检测。
        /// </summary>
        private void CheckTouch()
        {
            if (Input.touchCount == 1)
            {
                CheckSingle(Input.GetTouch(0));
                m_OldDistance = 0;
            }
            else if (Input.touchCount == 2)
            {
                m_TouchDownCount = 0;
                m_MoveClick = false;
                m_CheckClick = false;
                CheckTwo(Input.GetTouch(0), Input.GetTouch(1));
            }
            else
            {
                m_OldDistance = 0;
            }
        }

        /// <summary>
        /// 检查单个手指的触摸情况。
        /// </summary>
        /// <param name="touch">触摸数据。</param>
        private void CheckSingle(Touch touch)
        {
            if (touch.phase == TouchPhase.Began)
            {
                m_CheckClick = true;
                m_TouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (!m_CheckClick && !m_MoveClick)
                {
                    //旋转
                    Vector2 delta = touch.deltaPosition;
                    Rotate(delta.x, delta.y);
                }
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                m_CheckClick = false;
            }
        }

        /// <summary>
        /// 检查两个手指的触摸情况。
        /// </summary>
        /// <param name="touch1">手指1触摸数据。</param>
        /// <param name="touch2">手指2触摸数据。</param>
        private void CheckTwo(Touch touch1, Touch touch2)
        {
            //至少有一个点在移动
            if (touch1.phase != TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                return;
            }

            float dis = Vector2.Distance(touch1.position, touch2.position);
            if (m_OldDistance <= 0)
            {
                m_OldDistance = dis;
            }
            else
            {
                //根据移动夹角判断操作
                if (Vector2.Angle(touch1.deltaPosition, touch2.deltaPosition) < 90)
                {
                    //两点的移动量控制平移
                    Vector2 deltamove = (touch1.deltaPosition + touch2.deltaPosition) / 2;
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

#endif


        /// <summary>
        /// 旋转操作。
        /// </summary>
        /// <param name="dx">X屏幕移动量。</param>
        /// <param name="dy">Y屏幕移动量。</param>
        private void Rotate(float dx, float dy)
        {
            //GameManager.Instance.Rotate(-dy * RotateRatio, dx * RotateRatio);
            //Log.Info("Rotate:({0},{1})", dx, dy);
            m_Processer?.OnRotate(dx * RotateRatio, dy * RotateRatio);
        }

        /// <summary>
        /// 移动操作。
        /// </summary>
        /// <param name="dx">X屏幕移动量。</param>
        /// <param name="dy">Y屏幕移动量。</param>
        private void Move(float dx, float dy)
        {
            //GameManager.Instance.Move(dx * MoveRatio, dy * MoveRatio);
        }

        /// <summary>
        /// 缩放操作。
        /// </summary>
        /// <param name="d">屏幕移动量。</param>
        private void Scale(float d)
        {
            //GameManager.Instance.Scale(d * ScaleRatio);
            //Log.Info("Scale:{0}", d);
            m_Processer?.OnScale(d * ScaleRatio);
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 取消点击的距离。
        /// </summary>
        private static float CancelClickDistance = 25;

#if !UNITY_EDITOR
        
        /// <summary>
        /// 缩放操作的两点距离。
        /// </summary>
        private float m_OldDistance = 0;
        
        /// <summary>
        /// 是否按下，用于只检测区域内的Touch。
        /// </summary>
        private bool m_TouchDown = false;
#endif

        /// <summary>
        /// 按下位置。
        /// </summary>
        private Vector2 m_TouchPosition;

        /// <summary>
        /// 是否检测点击。
        /// </summary>
        private bool m_CheckClick = false;

        /// <summary>
        /// 是否在移动的过程中点击。
        /// </summary>
        private bool m_MoveClick = false;

        /// <summary>
        /// 按下的时间计数。
        /// </summary>
        private float m_TouchDownCount = 0;

        /// <summary>
        /// 触摸处理者。
        /// </summary>
        private ITouchProcesser m_Processer = null;

        #endregion
    }
}