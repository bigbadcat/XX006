using UnityEngine;
using System.Collections;

namespace XuXiang
{
    /// <summary>
    /// 图形绘图通用函数。
    /// </summary>
    public static class ShapGizmos
    {
        /// <summary>
        /// 绘制圆的折线角度分量。
        /// </summary>
        public static float CircleAngleDelta = 6;

        /// <summary>
        /// 绘制圆形。
        /// </summary>
        /// <param name="shap">圆形数据。</param>
        public static void DrawShap(ShapeCircle shap)
        {
            Vector2 start_pos = new Vector2(shap.radius, 0) + shap.position;
            Vector2 last_pos = start_pos;
            for (float next_a = CircleAngleDelta; next_a < 360; next_a += CircleAngleDelta)
            {
                float next_r = next_a * Mathf.Deg2Rad;
                float x = Mathf.Cos(next_r) * shap.radius;
                float y = Mathf.Sin(next_r) * shap.radius;
                Vector2 next_pos = new Vector2(x, y) + shap.position;
                Gizmos.DrawLine(last_pos.ToVector3(), next_pos.ToVector3());
                last_pos = next_pos;
            }
            Gizmos.DrawLine(last_pos.ToVector3(), start_pos.ToVector3());
        }

        /// <summary>
        /// 绘制矩形。
        /// </summary>
        /// <param name="shap">矩形数据。</param>
        public static void DrawShap(ShapeRect shap)
        {
            float rx = shap.height / 2;
            float lx = -rx;            
            float ty = shap.width / 2;
            float by = -ty;
            DrawRect(lx, rx, ty, by, shap.direction, shap.position);
        }

        /// <summary>
        /// 绘制发射。
        /// </summary>
        /// <param name="shap">发射数据。</param>
        public static void DrawShap(ShapeTransmit shap)
        {      
            float ty = shap.width / 2;
            float by = -ty;
            DrawRect(0, shap.lenghth, ty, by, shap.direction, shap.position);
        }

        /// <summary>
        /// 绘制矩形。
        /// </summary>
        /// <param name="lx">相对中心左侧距离。</param>
        /// <param name="rx">相对中心右侧距离。</param>
        /// <param name="ty">相对中心上侧距离。</param>
        /// <param name="by">相对中心下侧距离。</param>
        /// <param name="dir">矩形朝向。</param>
        /// <param name="pos">矩形中心位置。</param>
        private static void DrawRect(float lx, float rx, float ty, float by, float dir, Vector2 pos)
        {
            //旋转变化后再位置偏移
            float a = ShapeUtil.ConvertDirectionToAngle(dir);
            Vector2 lb = ShapeUtil.GetRotatePoint(new Vector2(lx, by), a) + pos;
            Vector2 lt = ShapeUtil.GetRotatePoint(new Vector2(lx, ty), a) + pos;
            Vector2 rt = ShapeUtil.GetRotatePoint(new Vector2(rx, ty), a) + pos;
            Vector2 rb = ShapeUtil.GetRotatePoint(new Vector2(rx, by), a) + pos;
            Gizmos.DrawLine(lb.ToVector3(), lt.ToVector3());
            Gizmos.DrawLine(lt.ToVector3(), rt.ToVector3());
            Gizmos.DrawLine(rt.ToVector3(), rb.ToVector3());
            Gizmos.DrawLine(rb.ToVector3(), lb.ToVector3());
        }

        /// <summary>
        /// 绘制扇形。
        /// </summary>
        /// <param name="shap">扇形数据。</param>
        public static void DrawShap(ShapeSector shap)
        {
            Vector2 origin_pos = new Vector2(shap.radius, 0);
            float a = ShapeUtil.ConvertDirectionToAngle(shap.direction);
            float min_a = a - shap.angle / 2;
            float max_a = a + shap.angle / 2;
            float cur_a = min_a;
            Vector2 cur_pos = ShapeUtil.GetRotatePoint(origin_pos, cur_a) + shap.position;
            Vector2 end_pos = ShapeUtil.GetRotatePoint(origin_pos, max_a) + shap.position;
            Gizmos.DrawLine(shap.position.ToVector3(), cur_pos.ToVector3());
            Gizmos.DrawLine(shap.position.ToVector3(), end_pos.ToVector3());
            for (float next_a = cur_a + CircleAngleDelta; next_a < max_a; next_a += CircleAngleDelta)
            {
                Vector2 next_pos = ShapeUtil.GetRotatePoint(origin_pos, next_a) + shap.position;
                Gizmos.DrawLine(cur_pos.ToVector3(), next_pos.ToVector3());
                cur_a = next_a;
                cur_pos = next_pos;
            }
            Gizmos.DrawLine(cur_pos.ToVector3(), end_pos.ToVector3());
        }

        /// <summary>
        /// 绘制环形。
        /// </summary>
        /// <param name="shap">环形数据。</param>
        public static void DrawShap(ShapeRing shap)
        {
            Vector2 start_pos = new Vector2(shap.radius_in, 0) + shap.position;
            Vector2 last_pos = start_pos;
            Vector2 start_pos2 = new Vector2(shap.radius_out, 0) + shap.position;
            Vector2 last_pos2 = start_pos2;
            for (float next_a = CircleAngleDelta; next_a < 360; next_a += CircleAngleDelta)
            {
                float next_r = next_a * Mathf.Deg2Rad;
                float v_cos = Mathf.Cos(next_r);
                float v_sin = Mathf.Sin(next_r);
                Vector2 next_pos = new Vector2(v_cos * shap.radius_in, v_sin * shap.radius_in) + shap.position;
                Gizmos.DrawLine(last_pos.ToVector3(), next_pos.ToVector3());
                last_pos = next_pos;

                Vector2 next_pos2 = new Vector2(v_cos * shap.radius_out, v_sin * shap.radius_out) + shap.position;
                Gizmos.DrawLine(last_pos2.ToVector3(), next_pos2.ToVector3());
                last_pos2 = next_pos2;
            }
            Gizmos.DrawLine(last_pos.ToVector3(), start_pos.ToVector3());
            Gizmos.DrawLine(last_pos2.ToVector3(), start_pos2.ToVector3());
        }
    }
}