using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XuXiang;

namespace XX006.UI
{
    /// <summary>
    /// 玩家移动控制器。
    /// </summary>
    public class MoveController : MonoBehaviour
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 按下。
        /// </summary>
        /// <param name="data">按下的信息。</param>
        public void OnPointDown(BaseEventData data)
        {
            if (m_TouchControl)
            {
                return;
            }

            PointerEventData pdata = data as PointerEventData;
            //Vector3 wv = PanelManager.Instance.UICamera.ScreenToWorldPoint(pdata.position);
            Vector3 wv = this.GetComponentInParent<Canvas>().worldCamera.ScreenToWorldPoint(pdata.position);
            Vector3 v = transform.InverseTransformPoint(wv);
            v.z = 0;            
            if (v.sqrMagnitude > 40000)     //按下点距离不得超过中心点200的距离
            {
                return;
            }
            //Log.Info("Down:{0}", v);
            m_TouchControl = true;
            Wheel.anchoredPosition = v;
            Rocker.anchoredPosition = v;
            Rocker.gameObject.SetActive(true);
        }

        /// <summary>
        /// 拖拽。
        /// </summary>
        /// <param name="data">拖拽的信息。</param>
        public void OnDrag(BaseEventData data)
        {
            if (!m_TouchControl)
            {
                return;
            }

            PointerEventData pdata = data as PointerEventData;
            //Vector3 wv = PanelManager.Instance.UICamera.ScreenToWorldPoint(pdata.position);
            Vector3 wv = this.GetComponentInParent<Canvas>().worldCamera.ScreenToWorldPoint(pdata.position);
            Vector2 v = transform.InverseTransformPoint(wv);
            Vector2 pos = v - Wheel.anchoredPosition;
            if (pos.sqrMagnitude > MAX_MOVE_DISTANCE * MAX_MOVE_DISTANCE)
            {
                pos.Normalize();
                pos.x *= MAX_MOVE_DISTANCE;
                pos.y *= MAX_MOVE_DISTANCE;
            }
            Rocker.anchoredPosition = pos + Wheel.anchoredPosition;
        }

        /// <summary>
        /// 弹起。
        /// </summary>
        /// <param name="data">弹起的信息。</param>
        public void OnPointUp(BaseEventData data)
        {
            if (!m_TouchControl)
            {
                return;
            }

            m_TouchControl = false;
            Wheel.anchoredPosition = Vector3.zero;
            Rocker.gameObject.SetActive(false);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 最小移动距离，超过此距离算移动。
        /// </summary>
        public const float MIN_MOVE_DISTANCE = 20;

        /// <summary>
        /// 最大移动距离，虚拟摇杆最多离开原点的距离。
        /// </summary>
        public const float MAX_MOVE_DISTANCE = 150;

        /// <summary>
        /// 摇杆背景图片。
        /// </summary>
        public RectTransform Wheel;

        /// <summary>
        /// 摇杆图像。
        /// </summary>
        public RectTransform Rocker;

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 唤醒。
        /// </summary>
        protected void Awake()
        {
            Wheel.gameObject.SetActive(true);
            Rocker.gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新，用于移动控制。
        /// </summary>
        private void Update()
        {
            bool move = false;
            Vector3 dir = Vector3.zero;
            if (m_TouchControl)
            {
                //使用虚拟摇杆控制
                Vector2 v = Rocker.anchoredPosition - Wheel.anchoredPosition;
                if (v.sqrMagnitude >= MIN_MOVE_DISTANCE * MIN_MOVE_DISTANCE)
                {
                    //有激活的摄像机
                    if (CameraController.CurCamera != null)
                    {
                        v.Normalize();
                        Transform ct = CameraController.CurCamera.transform;
                        Vector3 forward = Vector3.Scale(ct.forward, new Vector3(1, 0, 1)).normalized;
                        dir = forward * v.y + ct.right * v.x;
                        move = true;
                    }
                }
            }
            else
            {
                //外部设备控制
                float h = Input.GetAxis("Horizontal");
                float v = Input.GetAxis("Vertical");
                if (Mathf.Abs(h) >= 0.0001f || Mathf.Abs(v) >= 0.0001f)
                {
                    //有激活的摄像机
                    if (CameraController.CurCamera != null)
                    {
                        Transform ct = CameraController.CurCamera.transform;
                        Vector3 forward = Vector3.Scale(ct.forward, new Vector3(1, 0, 1)).normalized;
                        dir = forward * v + ct.right * h;
                        move = true;
                    }
                }
            }
            
            //移动或待机操作
            if (move)
            {
                dir.y = 0;
                if ((m_LastDirection - dir).sqrMagnitude > 0.0001f)
                {
                    ControllerCenter.Instance.DoMove(dir);
                    m_LastMove = true;
                    m_LastDirection = dir;
                }                
            }
            else if (m_LastMove)
            {
                ControllerCenter.Instance.DoIdle();
                m_LastMove = false;
                m_LastDirection = Vector3.zero;
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 是否触摸了摇杆。
        /// </summary>
        private bool m_TouchControl = false;

        /// <summary>
        /// 上次碰到是否移动了。
        /// </summary>
        private bool m_LastMove = false;

        /// <summary>
        /// 上次移动朝向
        /// </summary>
        private Vector3 m_LastDirection = Vector3.zero;

        #endregion
    }
}