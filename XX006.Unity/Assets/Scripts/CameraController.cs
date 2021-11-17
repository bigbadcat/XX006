using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// CameraMove。
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 当前摄像机。
        /// </summary>
        public static CameraController CurCamera;

        public void ChangeRotate(float delta)
        {
            m_Longitude += delta;
        }

        public void ChangePitch(float delta)
        {
            m_Latitude = Mathf.Clamp(m_Latitude + delta, 20, 80);
        }

        public void ChangeZoom(float delta)
        {
            m_Radius = Mathf.Clamp(m_Radius + delta, 3, 10);
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 跟随目标。
        /// </summary>
        public Transform FollowTarget;

        #endregion

        #region 内部操作----------------------------------------------------------------

        private void Awake()
        {
            CurCamera = this;
        }

        private void LateUpdate()
        {
            if (FollowTarget != null)
            {
                Vector3 tpos = FollowTarget.position;
                transform.position = tpos + Util.GetSpherePosition(m_Longitude, m_Latitude, m_Radius);
                transform.LookAt(tpos + m_LookOffset + new Vector3(0, m_UpOffset, 0));
            }
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 经度。
        /// </summary>
        [SerializeField]
        [Range(-180, 180)]
        private float m_Longitude = -90;

        /// <summary>
        /// 纬度。
        /// </summary>
        [Range(20, 80)]
        [SerializeField]
        private float m_Latitude = 50;

        /// <summary>
        /// 向上偏移。
        /// </summary>
        [Range(0, 20)]
        [SerializeField]
        private float m_UpOffset = 0;

        /// <summary>
        /// 半径。
        /// </summary>
        [Range(3, 10)]
        [SerializeField]
        private float m_Radius = 6;

        /// <summary>
        /// 摄像机观看目标的偏移。
        /// </summary>
        [SerializeField]
        private Vector3 m_LookOffset = new Vector3(0, 1, 0);

        #endregion
    }
}