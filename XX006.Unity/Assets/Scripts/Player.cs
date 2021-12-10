using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XX006;
using XX006.Fight;
using Avatar = XuXiang.Avatar;

public class Player : MonoBehaviour
{
    void Start()
    {
        m_Avatar = this.GetComponentInChildren<Avatar>();
        ControllerCenter.Instance.Target = this;
        CameraController.CurCamera.FollowTarget = this.transform;
        TryIdle();
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
        PlayAnimtion("run");
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

        m_WantState = -1;
        m_State = 0;
        PlayAnimtion("stand");
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
            SkillActionAnimation a_ani = new SkillActionAnimation();
            a_ani.AniName = "skill04";
            a_ani.Duration = 2.2f;
            m_CurSkill.AddAction(a_ani, 0);

            SkillActionChangeBend a_cb = new SkillActionChangeBend();
            a_cb.ChangeCurve = new AnimationCurve(new Keyframe(0.0f, 0), new Keyframe(0.6f, 0), new Keyframe(0.8f, 8));
            a_cb.Duration = 2.0f;
            m_CurSkill.AddAction(a_cb, 0.8f);
        }
        else if (id == 3)
        {
            SkillActionAnimation a_ani = new SkillActionAnimation();
            a_ani.AniName = "skill03";
            a_ani.Duration = 1.6f;
            m_CurSkill.AddAction(a_ani, 0);

            SkillActionFlyingObject a_fo = new SkillActionFlyingObject();
            a_fo.ShowPath = "Character/Qigongdou/Skill/Skill03";
            a_fo.Speed = 10;
            a_fo.FlyTime = 1;
            m_CurSkill.AddAction(a_fo, 1);
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
        //m_Ani.CrossFade(name, mix);
        m_Avatar?.PlayAnimation(name, mix);
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

    private Avatar m_Avatar = null;

    private SkillRunTime m_CurSkill = null;

    /// <summary>
    /// 技能行为列表。
    /// </summary>
    private List<SkillEffect> m_Actions = new List<SkillEffect>();
}
