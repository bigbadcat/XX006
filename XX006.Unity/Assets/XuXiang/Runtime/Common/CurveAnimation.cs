using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XuXiang
{
    /// <summary>
    /// 曲线动画。
    /// </summary>
    public class CurveAnimation : MonoBehaviour
    {
        #region 对外操作----------------------------------------------------------------

        /// <summary>
        /// 获取指定标识的动画。
        /// </summary>
        /// <param name="obj">游戏对象。</param>
        /// <param name="id">动画标识。</param>
        /// <returns>动画对象。</returns>
        public static CurveAnimation GeteAnimation(GameObject obj, int id)
        {
            CurveAnimation ani = null;
            TempList.Clear();
            obj.GetComponents(TempList);
            foreach (var t in TempList)
            {
                if (t.ID == id)
                {
                    ani = t;
                    break;
                }
            }
            TempList.Clear();
            return ani;
        }

        /// <summary>
        /// 获取曲线时长。
        /// </summary>
        /// <param name="curve">曲线对象。</param>
        /// <returns>时长。</returns>
        public static float GetCurveTime(AnimationCurve curve)
        {
            if (curve == null || curve.length <= 0)
            {
                return 0;
            }
            Keyframe kf = curve[curve.length - 1];
            return kf.time;
        }

        /// <summary>
        /// 重置动画数据。
        /// </summary>
        public void ResetAnim()
        {
            m_Duration = 0.01f;      //时长最少0.01秒
            if (m_PositionCurve)
            {
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_PositionX));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_PositionY));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_PositionZ));
            }
            if (m_ScaleCurve)
            {
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_ScaleX));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_ScaleY));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_ScaleZ));
            }
            if (m_RotateCurve)
            {
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_RotateX));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_RotateY));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_RotateZ));
            }
            if (m_AlphaCurve)
            {
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_Alpha));
            }
            if (m_ColorCurve)
            {
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_ColorR));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_ColorG));
                m_Duration = Mathf.Max(m_Duration, GetCurveTime(m_ColorB));
            }
        }

        /// <summary>
        /// 播放动画。
        /// </summary>
        /// <param name="stay">是否从当前动画时刻开始。</param>
        public void Play(bool stay)
        {
            this.enabled = true;
            OnTime(stay ? m_CurTime : 0);
        }

        /// <summary>
        /// 停止动画。
        /// </summary>
        /// <param name="stay">是否停留当前状态。</param>
        public void Stop(bool stay)
        {
            this.enabled = false;
            if (!stay)
            {
                OnTime(0);
            }
        }

        /// <summary>
        /// 添加动画结束监听者。
        /// </summary>
        /// <param name="listener">监听者。</param>
        public void AddEndListener(Action<CurveAnimation, bool> listener)
        {
            m_OnEndListener.AddListener(listener);
        }

        /// <summary>
        /// 移除动画结束监听者。
        /// </summary>
        /// <param name="listener">监听者。</param>
        public void RemoveEndListener(Action<CurveAnimation, bool> listener)
        {
            m_OnEndListener.RemoveEndListener(listener);
        }
        
        /// <summary>
        /// 清除结束回调。
        /// </summary>
        public void ClearEndListener()
        {
            m_OnEndListener.ClearListener();
        }

        #endregion

        #region 对外属性----------------------------------------------------------------

        /// <summary>
        /// 获取或设置标识。
        /// </summary>
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        /// <summary>
        /// 获取动画时长。
        /// </summary>
        public float Duration
        {
            get { return m_Duration; }
        }

        /// <summary>
        /// 获取是否播放中。
        /// </summary>
        public bool IsPlaying
        {
            get { return this.enabled; }
        }

        #endregion

        #region 内部操作----------------------------------------------------------------

        /// <summary>
        /// 唤醒。
        /// </summary>
        private void Awake()
        {
            //确定动画目标
            if (m_Target == null)
            {
                m_Target = this.transform;
            }
            m_UITarget = m_Target as RectTransform;
            m_InitPosition = m_UITarget != null ? m_UITarget.anchoredPosition3D : m_Target.localPosition;
            if (m_UITarget != null)
            {
                m_CanvasGroupTarget = m_UITarget.GetComponent<CanvasGroup>();
                m_GraphicTarget = m_UITarget.GetComponent<Graphic>();
            }
            ResetAnim();

            //不能自动播放，得禁掉自己
            if (m_AutoPlay)
            {
                OnTime(0);
            }
            else
            {
                this.enabled = false;
            }
        }

        /// <summary>
        /// 帧更新。
        /// </summary>
        void Update()
        {
            float new_t = m_CurTime + Time.deltaTime * m_Speed;
            if (new_t >= m_Duration)
            {
                if (m_IsLoop && m_Duration > 0)
                {
                    //浮点数取余
                    int n = (int)(new_t / m_Duration);
                    float t = new_t - m_Duration * n;
                    OnTime(t);
                }
                else
                {
                    //停留在最后时刻
                    OnTime(m_Duration);
                    this.enabled = false;                    
                }
                OnEnd();        //结束通知，循环播放也会
            }
            else
            {
                OnTime(new_t);
            }
        }

        /// <summary>
        /// 动画时刻。
        /// </summary>
        /// <param name="t"></param>
        private void OnTime(float t)
        {
            m_CurTime = t;
            if (m_Target == null)
            {
                return;
            }
            OnTimeTransform();
            OnTimeColor();
        }

        /// <summary>
        /// 变换时刻。
        /// </summary>
        private void OnTimeTransform()
        {
            //位置动画
            if (m_PositionCurve)
            {
                float x = m_PositionX.Evaluate(m_CurTime) * m_PositionScale;
                float y = m_PositionY.Evaluate(m_CurTime) * m_PositionScale;
                if (m_UITarget != null)
                {
                    m_UITarget.anchoredPosition = new Vector2(m_InitPosition.x + x, m_InitPosition.y + y);
                }
                else
                {
                    float z = m_PositionZ.Evaluate(m_CurTime) * m_PositionScale;
                    m_Target.localPosition = m_InitPosition + new Vector3(x, y, z);
                }
            }

            //缩放动画
            if (m_ScaleCurve)
            {
                float x = m_ScaleX.Evaluate(m_CurTime);
                float y = m_ScaleUniform ? x : m_ScaleY.Evaluate(m_CurTime);
                float z = m_ScaleUniform ? x : m_ScaleZ.Evaluate(m_CurTime);
                m_Target.localScale = new Vector3(x, y, z);
            }

            //旋转动画
            if (m_RotateCurve)
            {
                float x = m_RotateX.Evaluate(m_CurTime) * 360;
                float y = m_RotateY.Evaluate(m_CurTime) * 360;
                float z = m_RotateZ.Evaluate(m_CurTime) * 360;
                m_Target.localRotation = Quaternion.Euler(x, y, z);
            }
        }

        /// <summary>
        /// 颜色时刻。
        /// </summary>
        private void OnTimeColor()
        {
            //半透明动画
            if (m_AlphaCurve && (m_CanvasGroupTarget != null || m_GraphicTarget != null))
            {
                //半透明优先改CanvasGroup
                float a = Mathf.Clamp01(m_Alpha.Evaluate(m_CurTime));
                if (m_CanvasGroupTarget != null)
                {
                    m_CanvasGroupTarget.alpha = a;
                }
                else if (m_GraphicTarget != null)
                {
                    Color c = m_GraphicTarget.color;
                    c.a = a;
                    m_GraphicTarget.color = c;
                }
            }

            //颜色动画
            if (m_ColorCurve && m_GraphicTarget != null)
            {
                float r = Mathf.Clamp01(m_ColorR.Evaluate(m_CurTime));
                float g = Mathf.Clamp01(m_ColorG.Evaluate(m_CurTime));
                float b = Mathf.Clamp01(m_ColorB.Evaluate(m_CurTime));
                float a = m_GraphicTarget.color.a;
                m_GraphicTarget.color = new Color(r, g, b, a);
            }
        }

        /// <summary>
        /// 动画结束。
        /// </summary>
        private void OnEnd()
        {
            m_OnEndListener.Invoke(this, m_IsLoop);
        }

        #endregion

        #region 内部数据----------------------------------------------------------------

        /// <summary>
        /// 用于无GC获取动画。
        /// </summary>
        private static List<CurveAnimation> TempList = new List<CurveAnimation>();

        /// <summary>
        /// 动画标识。
        /// </summary>
        [SerializeField]
        private int m_ID = 0;

        /// <summary>
        /// 位移动画。会基于自身初始位置做动画。
        /// </summary>
        [SerializeField]
        private bool m_PositionCurve = false;
        [SerializeField]
        private float m_PositionScale = 100;
        [SerializeField]
        private AnimationCurve m_PositionX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField]
        private AnimationCurve m_PositionY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField]
        private AnimationCurve m_PositionZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

        /// <summary>
        /// 位移动画的初始位置。
        /// </summary>
        private Vector3 m_InitPosition = Vector3.zero;

        /// <summary>
        /// 缩放动画。原来的初始缩放值会被覆盖掉。
        /// </summary>
        [SerializeField]
        private bool m_ScaleCurve = false;
        [SerializeField]
        private bool m_ScaleUniform = false;
        [SerializeField]
        private AnimationCurve m_ScaleX = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        [SerializeField]
        private AnimationCurve m_ScaleY = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        [SerializeField]
        private AnimationCurve m_ScaleZ = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        /// <summary>
        /// 旋转动画。
        /// </summary>
        [SerializeField]
        private bool m_RotateCurve = false;
        [SerializeField]
        private AnimationCurve m_RotateX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField]
        private AnimationCurve m_RotateY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));
        [SerializeField]
        private AnimationCurve m_RotateZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 0));

        /// <summary>
        /// 半透明动画。(仅支持Grahpic和CanvasGroup)
        /// </summary>
        [SerializeField]
        private bool m_AlphaCurve = false;
        [SerializeField]
        private AnimationCurve m_Alpha = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        /// <summary>
        /// 颜色明动画。(仅支持Grahpic)
        /// </summary>
        [SerializeField]
        private bool m_ColorCurve = false;
        [SerializeField]
        private AnimationCurve m_ColorR = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        [SerializeField]
        private AnimationCurve m_ColorG = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
        [SerializeField]
        private AnimationCurve m_ColorB = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

        /// <summary>
        /// 动画持续时间。
        /// </summary>
        private float m_Duration;

        /// <summary>
        /// 是否循环。
        /// </summary>
        [SerializeField]
        private bool m_IsLoop;

        /// <summary>
        /// 播放速度。
        /// </summary>
        [Range(0, 100)]
        [SerializeField]
        private float m_Speed = 1;

        /// <summary>
        /// 是否自动播放。
        /// </summary>
        [SerializeField]
        private bool m_AutoPlay;

        /// <summary>
        /// 当前时间。
        /// </summary>
        private float m_CurTime = 0;

        /// <summary>
        /// 动画目标，若为则表示自身。
        /// </summary>
        [SerializeField]
        private Transform m_Target;

        /// <summary>
        /// 动画目标，如果此值不为null，则位置动画忽略m_Target和PositionZ。
        /// </summary>        
        private RectTransform m_UITarget;

        /// <summary>
        /// 画布组目标。(用于半透明动画)
        /// </summary>
        private CanvasGroup m_CanvasGroupTarget;

        /// <summary>
        /// 图形目标。(用于半透明和颜色动画)
        /// </summary>
        private Graphic m_GraphicTarget;

        /// <summary>
        /// 动画结束回调。
        /// </summary>
        private ListenerContainer<CurveAnimation, bool> m_OnEndListener = new ListenerContainer<CurveAnimation, bool>();

        #endregion
    }

#if UNITY_EDITOR

    /// <summary>
    /// UIEventReceiver的编辑页面。
    /// </summary>
    [CustomEditor(typeof(CurveAnimation))]
    [ExecuteInEditMode]
    public class CurveAnimationEditor : Editor
    {
        private SerializedProperty m_Script;
        private SerializedProperty m_ID;

        private SerializedProperty m_PositionCurve;
        private SerializedProperty m_PositionScale;
        private SerializedProperty m_PositionX;
        private SerializedProperty m_PositionY;
        private SerializedProperty m_PositionZ;

        private SerializedProperty m_ScaleCurve;
        private SerializedProperty m_ScaleUniform;
        private SerializedProperty m_ScaleX;
        private SerializedProperty m_ScaleY;
        private SerializedProperty m_ScaleZ;

        private SerializedProperty m_RotateCurve;
        private SerializedProperty m_RotateX;
        private SerializedProperty m_RotateY;
        private SerializedProperty m_RotateZ;

        private SerializedProperty m_AlphaCurve;
        private SerializedProperty m_Alpha;

        private SerializedProperty m_ColorCurve;
        private SerializedProperty m_ColorR;
        private SerializedProperty m_ColorG;
        private SerializedProperty m_ColorB;
        
        private SerializedProperty m_IsLoop;
        private SerializedProperty m_Speed;
        private SerializedProperty m_AutoPlay;
        private SerializedProperty m_Target;

        private void OnEnable()
        {
            m_Script = serializedObject.FindProperty("m_Script");
            m_ID = serializedObject.FindProperty("m_ID");
            m_PositionCurve = serializedObject.FindProperty("m_PositionCurve");
            m_PositionScale = serializedObject.FindProperty("m_PositionScale");
            m_PositionX = serializedObject.FindProperty("m_PositionX");
            m_PositionY = serializedObject.FindProperty("m_PositionY");
            m_PositionZ = serializedObject.FindProperty("m_PositionZ");

            m_ScaleCurve = serializedObject.FindProperty("m_ScaleCurve");
            m_ScaleUniform = serializedObject.FindProperty("m_ScaleUniform");
            m_ScaleX = serializedObject.FindProperty("m_ScaleX");
            m_ScaleY = serializedObject.FindProperty("m_ScaleY");
            m_ScaleZ = serializedObject.FindProperty("m_ScaleZ");

            m_RotateCurve = serializedObject.FindProperty("m_RotateCurve");
            m_RotateX = serializedObject.FindProperty("m_RotateX");
            m_RotateY = serializedObject.FindProperty("m_RotateY");
            m_RotateZ = serializedObject.FindProperty("m_RotateZ");

            m_AlphaCurve = serializedObject.FindProperty("m_AlphaCurve");
            m_Alpha = serializedObject.FindProperty("m_Alpha");

            m_ColorCurve = serializedObject.FindProperty("m_ColorCurve");
            m_ColorR = serializedObject.FindProperty("m_ColorR");
            m_ColorG = serializedObject.FindProperty("m_ColorG");
            m_ColorB = serializedObject.FindProperty("m_ColorB");

            m_IsLoop = serializedObject.FindProperty("m_IsLoop");
            m_Speed = serializedObject.FindProperty("m_Speed");
            m_AutoPlay = serializedObject.FindProperty("m_AutoPlay");
            m_Target = serializedObject.FindProperty("m_Target");
        }

        /// <summary>
        /// 绘制面板。
        /// </summary>
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_Script);
            EditorGUILayout.PropertyField(m_ID);
            DrawTransform();
            DrawColor();
            DrawOperate();

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// 绘制变换部分。
        /// </summary>
        private void DrawTransform()
        {
            EditorGUILayout.PropertyField(m_PositionCurve);
            if (m_PositionCurve.boolValue)
            {
                EditorGUILayout.PropertyField(m_PositionScale);
                EditorGUILayout.PropertyField(m_PositionX);
                EditorGUILayout.PropertyField(m_PositionY);
                EditorGUILayout.PropertyField(m_PositionZ);
            }

            EditorGUILayout.PropertyField(m_ScaleCurve);
            if (m_ScaleCurve.boolValue)
            {
                EditorGUILayout.PropertyField(m_ScaleUniform);
                EditorGUILayout.PropertyField(m_ScaleX);
                if (!m_ScaleUniform.boolValue)
                {
                    EditorGUILayout.PropertyField(m_ScaleY);
                    EditorGUILayout.PropertyField(m_ScaleZ);
                }
            }

            EditorGUILayout.PropertyField(m_RotateCurve);
            if (m_RotateCurve.boolValue)
            {
                EditorGUILayout.PropertyField(m_RotateX);
                EditorGUILayout.PropertyField(m_RotateY);
                EditorGUILayout.PropertyField(m_RotateZ);
            }
        }

        /// <summary>
        /// 绘制颜色部分。
        /// </summary>
        private void DrawColor()
        {
            EditorGUILayout.PropertyField(m_AlphaCurve);
            if (m_AlphaCurve.boolValue)
            {
                EditorGUILayout.PropertyField(m_Alpha);
            }

            EditorGUILayout.PropertyField(m_ColorCurve);
            if (m_ColorCurve.boolValue)
            {
                EditorGUILayout.PropertyField(m_ColorR);
                EditorGUILayout.PropertyField(m_ColorG);
                EditorGUILayout.PropertyField(m_ColorB);
            }
        }

        /// <summary>
        /// 绘制操作部分。
        /// </summary>
        private void DrawOperate()
        {
            EditorGUILayout.PropertyField(m_IsLoop);
            EditorGUILayout.PropertyField(m_Speed);
            EditorGUILayout.PropertyField(m_AutoPlay);
            EditorGUILayout.PropertyField(m_Target);

            if (EditorApplication.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Reset"))
                {
                    CurveAnimation ani = target as CurveAnimation;
                    ani.ResetAnim();
                    ani.Play(false);
                }
                GUILayout.EndHorizontal();
            }
        }
    }

#endif
}