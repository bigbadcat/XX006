using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XuXiang;

namespace XX006
{
    /// <summary>
    /// ��ͼ�顣
    /// </summary>
    public class MapChunk
    {
        /// <summary>
        /// ���캯����
        /// </summary>
        /// <param name="id"></param>
        public MapChunk(int id)
        {
            m_ID = id;
        }

        /// <summary>
        /// ��Ӷ���
        /// </summary>
        /// <param name="res">��Դ·����</param>
        /// <param name="positon">����λ�á�</param>
        /// <param name="scale">�������š�</param>
        /// <param name="rotate">������ת��</param>
        /// <returns>�����ʶ��</returns>
        public GameObject AddObject(string res, Vector3 positon, Vector3 scale, Vector3 rotate)
        {
            GameObject obj = ResourceManager.Instance.LoadObject(res);
            obj.transform.localPosition = positon;
            obj.transform.localScale = scale;
            obj.transform.localEulerAngles = rotate;
            return obj;
        }

        /// <summary>
        /// �ͷŵ�ͼ�顣
        /// </summary>
        public void Release()
        {
            foreach (var kvp in m_Objects)
            {
                GameObject.Destroy(kvp.Value);
            }
            m_Objects.Clear();
        }

        /// <summary>
        /// ��ȡ����ID��
        /// </summary>
        public int ID
        {
            get { return m_ID; }
        }

        /// <summary>
        /// ��ͼ�����š�
        /// </summary>
        private int m_ID = 0;

        /// <summary>
        /// ��ͼ���󼯺ϡ�
        /// </summary>
        private Dictionary<int, GameObject> m_Objects = new Dictionary<int, GameObject>();
    }
}