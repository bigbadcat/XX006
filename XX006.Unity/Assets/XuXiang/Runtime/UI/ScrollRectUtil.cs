using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace XuXiang
{
    /// <summary>
    /// ScrollRect工具。(在Awake或Start中设置ScrollRect的Normalized不起作用，需要持续一小段时间去强制设置)
    /// </summary>
    public class ScrollRectUtil : MonoBehaviourCache
	{
        /// <summary>
        /// 滑动区域组建。
        /// </summary>
        private ScrollRect _target;

        /// <summary>
        /// 水平位置。
        /// </summary>
        private float _horizontal;

        /// <summary>
        /// 竖直位置。
        /// </summary>
        private float _vertical;

        /// <summary>
        /// 设置时间计数。
        /// </summary>
        private int _count;

        /// <summary>
        /// 设置滑动区域的水平滑动位置。
        /// </summary>
        /// <param name="rect">滑动区域。</param>
        /// <param name="h">水平滑动位置。</param>
        public static void SetHorizontalNormalized(ScrollRect rect, float h)
        {
            SetNormalized(rect, h, rect.verticalNormalizedPosition);
        }

        /// <summary>
        /// 设置滑动区域的竖直滑动位置。
        /// </summary>
        /// <param name="rect">滑动区域。</param>
        /// <param name="h">竖直滑动位置。</param>
        public static void SetVerticalNormalized(ScrollRect rect, float v)
        {
            SetNormalized(rect, rect.horizontalNormalizedPosition, v);
        }

        /// <summary>
        /// 设置滑动区域的滑动位置。
        /// </summary>
        /// <param name="rect">滑动区域。</param>
        /// <param name="h">水平滑动位置。</param>
        /// <param name="h">竖直滑动位置。</param>
        public static void SetNormalized(ScrollRect rect, float h, float v)
        {
            ScrollRectUtil ut = rect.gameObject.GetOrAddComponent<ScrollRectUtil>();
            ut._target = rect;
            ut._horizontal = h;
            ut._vertical = v;
            ut._count = 0;
        }

        /// <summary>
        /// 更新。
        /// </summary>
        void Update ()
		{
            _target.StopMovement();
            _target.horizontalNormalizedPosition = _horizontal;
            _target.verticalNormalizedPosition = _vertical;
            if (++_count >= 2)  //持续两帧即可
            {
                Destroy(this);
            }
        }
	}
}

