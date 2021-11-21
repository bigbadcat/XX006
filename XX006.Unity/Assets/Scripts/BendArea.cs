using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// ѹ�����򣬻�Գ����ڵĲݲ���Ӱ�졣(Ŀǰ��֧��Բ��)
    /// </summary>
    public class BendArea : MonoBehaviourCache
    {
        /// <summary>
        /// ����ѹ������
        /// </summary>
        public static int MAX_BEND_AREA = 8;

        /// <summary>
        /// ��ȡ��ǰ��ѹ������
        /// </summary>
        public static List<BendArea> CurAreas
        {
            get { return s_CurAreas; }
        }

        /// <summary>
        /// ��ǰѹ������
        /// </summary>
        public static List<BendArea> s_CurAreas = new List<BendArea>();

        /// <summary>
        /// ��ȡ������ѹ�䷶Χ��(0-0.5���ѹ��ֵ��0.5-1�ݼ�)
        /// </summary>
        public float Range
        {
            get { return m_Range; }
            set { m_Range = Mathf.Max(0, value); }
        }

        /// <summary>
        /// ��ȡѹ����Ϣ�������ύ��ComputeShader�С�
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
        /// ѹ�䷶Χ��
        /// </summary>
        [Range(0, 10)]
        [SerializeField]
        private float m_Range = 1;
    }
}