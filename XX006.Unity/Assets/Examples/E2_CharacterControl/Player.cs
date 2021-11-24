using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XX006;
using XX006.Fight;

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
        if (m_State == 3)
        {
            m_WantState = 1;
            return;
        }

        m_WantState = -1;
        m_State = 1;        
        if (m_Ani != null)
        {
            PlayAnimtion("run", 0.1f);
            //m_Ani.CrossFade("skill02", 0.1f);
        }
    }

    public void TryIdle()
    {
        if (m_State == 0)
        {
            return;
        }
        if (m_State == 3)
        {
            m_WantState = 0;
            return;
        }

        if (m_State == 1)
        {
            m_WantState = -1;
            m_State = 0;
            if (m_Ani != null)
            {
                PlayAnimtion("stand", 0.1f);
            }
        }
    }

    public bool DoSkill(int id)
    {
        if (m_CurSkill != null)
        {
            return false;
        }

        //切换技能状态，创建技能对象(实际是从配置创建)
        m_State = 3;
        m_CurSkill = new SkillRunTime();
        m_CurSkill.m_Owner = this;
        if (id == 1)
        {
            SkillActionMove a_move = new SkillActionMove();
            a_move.Duration = 0.4f;
            a_move.SpeedRate = 3;
            m_CurSkill.AddAction(a_move, 0);

            SkillActionAnimation a_ani = new SkillActionAnimation();
            a_ani.AniName = "skill02";
            a_ani.Duration = 0.4f;
            m_CurSkill.AddAction(a_ani, 0);

        }
        else if (id == 2)
        {

        }
        else if (id == 3)
        {

        }
        return true;
    }

    public void StartEffect(SkillEffect effect, float delay, float duration)
    {
        m_Actions.Add(effect);
        effect.Start(this, delay, duration);
    }

    public void PlayAnimtion(string name, float mix = 0.1f)
    {
        m_Ani.CrossFade(name, mix);
    }

    private void Update()
    {
        if (m_State == 1)
        {
            float dis = m_MoveSpeed * Time.deltaTime;
            transform.Translate(0, 0, dis);
        }

        if (m_CurSkill != null)
        {
            m_CurSkill.Update(Time.deltaTime);
            if (m_CurSkill.IsEnd)
            {
                m_State = m_WantState == -1 ? 0 : m_WantState;
                PlayAnimtion(m_State == 0 ? "stand" : "run", 0.2f);
                m_WantState = -1;
                m_CurSkill = null;
            }
        }
        UpdateSkillAction();
    }

    private void UpdateSkillAction()
    {
        if (m_Actions.Count <= 0)
        {
            return;
        }

        float dt = Time.deltaTime;
        s_CacheIndex.Clear();
        for (int i=0; i<m_Actions.Count; ++i)
        {
            SkillEffect eft = m_Actions[i];
            eft.Update(dt);
            if (eft.IsEnd)
            {
                s_CacheIndex.Add(i);
            }
        }

        //移除掉已结束的行为
        for (int i= s_CacheIndex.Count-1; i>=0; --i)
        {
            m_Actions.RemoveAt(s_CacheIndex[i]);
        }
    }

    private static List<int> s_CacheIndex = new List<int>();

    [SerializeField]
    public float m_MoveSpeed = 1;

    private int m_WantState = -1;

    private int m_State = -1;        //0:idle 1:Move 2:Skill

    private Animator m_Ani = null;

    private SkillRunTime m_CurSkill = null;

    /// <summary>
    /// 技能行为列表。
    /// </summary>
    private List<SkillEffect> m_Actions = new List<SkillEffect>();
}
