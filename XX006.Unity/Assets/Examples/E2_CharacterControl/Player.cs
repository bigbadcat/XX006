using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XX006;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        m_Ani = this.GetComponentInChildren<Animator>();
        ControllerCenter.Instance.Target = this;
    }

    public void TryMove(float d)
    {
        transform.localRotation = Quaternion.Euler(0, d, 0);
        if (m_State == 1)
        {
            return;
        }

        m_State = 1;        
        if (m_Ani != null)
        {
            m_Ani.CrossFade("run", 0.1f);
        }
    }

    public void TryIdle()
    {
        if (m_State == 0)
        {
            return;
        }

        m_State = 0;
        if (m_Ani != null)
        {
            m_Ani.CrossFade("stand", 0.1f);
        }
    }

    private void Update()
    {
        if (m_State == 1)
        {
            float dis = m_MoveSpeed * Time.deltaTime;
            transform.Translate(0, 0, dis);
        }
    }

    [SerializeField]
    public float m_MoveSpeed = 1;

    private int m_State = -1;        //0:idle 1:Move

    private Animator m_Ani = null;
}
