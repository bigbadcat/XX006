using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XuXiang;

namespace XX006.UI
{
    /// <summary>
    /// 显示FPS。
    /// </summary>
    public class ShowFPS : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            DTValue.text = (m_CurDT / 10).ToString();
            FPSValue.text = m_CurFPS.ToString();
        }

        // Update is called once per frame
        void Update()
        {
            int dt = (int)(Time.deltaTime * 1000 * 10);
            m_DTCount.Add(dt);
            dt /= 10;
            if (dt != m_CurDT)
            {
                m_CurDT = dt;
                DTValue.text = m_CurDT.ToString();
            }
            int fps = (int)(10000.0f / m_DTCount.Average);
            if (m_CurFPS != fps)
            {
                m_CurFPS = fps;
                FPSValue.text = m_CurFPS.ToString();
            }
        }

        public Text DTValue;
        public Text FPSValue;

        private int m_CurDT = 0;
        private int m_CurFPS = 0;

        private NumberCountInt m_DTCount = new NumberCountInt(30);
    }
}