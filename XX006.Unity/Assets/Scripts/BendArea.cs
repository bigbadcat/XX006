using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// 压弯区域，会对场景内的草产生影响。(目前仅支持圆形)
    /// </summary>
    public class BendArea : MonoBehaviourCache
    {
        /// <summary>
        /// 最大的压弯区域。
        /// </summary>
        public static int MAX_BEND_AREA = 8;

        /// <summary>
        /// 获取当前的压弯区域。
        /// </summary>
        public static List<BendArea> CurAreas
        {
            get { return s_CurAreas; }
        }

        /// <summary>
        /// 当前压弯区域。
        /// </summary>
        public static List<BendArea> s_CurAreas = new List<BendArea>();

        /// <summary>
        /// 获取或设置压弯范围。(0-0.5最大压弯值，0.5-1递减)
        /// </summary>
        public float Range
        {
            get { return m_Range; }
            set { m_Range = Mathf.Max(0, value); }
        }

        /// <summary>
        /// 获取压弯信息。用于提交到ComputeShader中。
        /// </summary>
        public Vector4 Info
        {
            get { return this.CacheTransform.position.ToVector4(m_Range); }
        }

        private void OnEnable()
        {
            if (!s_CurAreas.Contains(this))
            {
                s_CurAreas.Add(this);
            }            
        }

        private void OnDisable()
        {
            s_CurAreas.Remove(this);
        }

        /// <summary>
        /// 压弯范围。
        /// </summary>
        [Range(0, 10)]
        [SerializeField]
        private float m_Range = 1;
    }
}