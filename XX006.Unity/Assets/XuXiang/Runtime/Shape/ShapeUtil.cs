using UnityEngine;
using System.Collections;

namespace XuXiang
{
    /// <summary>
    /// 图形函数。
    /// 圆形Circle 扇型Sector 矩形Rect 发射Transmit(矩形的另一种描述)。
    /// 朝向。Y方向为0，顺时针增加。
    /// 旋转角度。X方向为0，逆时针增加。
    /// </summary>
    public static class ShapeUtil
    {
        /// <summary>
        /// 检测两个圆是否碰撞。
        /// </summary>
        /// <param name="circle1">圆1数据。</param>
        /// <param name="circle2">圆2数据。</param>
        /// <returns>两个圆是否碰撞。</returns>
        public static bool CheckCollision(ShapeCircle circle1, ShapeCircle circle2)
        {
            float dis_sqrt = (circle1.position - circle2.position).sqrMagnitude;
            float r = Mathf.Max(0, circle1.radius) + Mathf.Max(0, circle2.radius);
            float check_sqrt = r * r;
            return dis_sqrt <= check_sqrt;
        }

        /// <summary>
        /// 检测圆与矩形是否碰撞。
        /// </summary>
        /// <param name="circle">圆数据。</param>
        /// <param name="rect">矩形数据。</param>
        /// <returns>是否碰撞。</returns>
        public static bool CheckCollision(ShapeCircle circle, ShapeRect rect)
        {
            float a = ShapeUtil.ConvertDirectionToAngle(rect.direction);
            Vector2 relative_cpos = circle.position - rect.position;
            Vector2 local_cpos = GetRotatePoint(relative_cpos, -a);
            Vector2 local_cpos_abs = new Vector2(Mathf.Abs(local_cpos.x), Mathf.Abs(local_cpos.y));
            return CheckCollisionCR(rect.width, rect.height, local_cpos_abs, circle.radius);
        }

        /// <summary>
        /// 检测圆与发射矩形是否碰撞。
        /// </summary>
        /// <param name="circle">圆数据。</param>
        /// <param name="transmit">发射数据。</param>
        /// <returns>是否碰撞。</returns>
        public static bool CheckCollision(ShapeCircle circle, ShapeTransmit transmit)
        {
            float a = ShapeUtil.ConvertDirectionToAngle(transmit.direction);
            Vector2 relative_cpos = circle.position - transmit.position;
            Vector2 local_cpos = GetRotatePoint(relative_cpos, -a);
            Vector2 local_cpos_abs = new Vector2(Mathf.Abs(local_cpos.x - transmit.lenghth / 2), Mathf.Abs(local_cpos.y));
            return CheckCollisionCR(transmit.width, transmit.lenghth, local_cpos_abs, circle.radius);
        }

        /// <summary>
        /// 检测圆与矩形是否碰撞，矩形为标准矩形，中心在原点，旋转角度为0。
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="abs_pos"></param>
        /// <param name="cr"></param>
        /// <returns>是否碰撞。</returns>
        public static bool CheckCollisionCR(float w, float h, Vector2 abs_pos, float cr)
        {
            float hw = w / 2;
            float hh = h / 2;
            float max_x = hh + cr;
            float max_y = hw + cr;
            if (abs_pos.x > max_x) { return false; }
            if (abs_pos.y > max_y) { return false; }
            if (abs_pos.x <= hh) { return true; }
            if (abs_pos.y <= hw) { return true; }
            float absx = abs_pos.x - hh;
            float absy = abs_pos.y - hw;
            if (absx * absx + absy * absy <= cr * cr) { return true; }
            return false;
        }

        /// <summary>
        /// 检测圆与扇型是否碰撞。
        /// </summary>
        /// <param name="cpos">圆数据。</param>
        /// <param name="sector">扇形数据。</param>
        /// <returns>是否碰撞。</returns>
        public static bool CheckCollision(ShapeCircle circle, ShapeSector sector)
        {
            //检测次序有依赖，不能调整
            float a = ShapeUtil.ConvertDirectionToAngle(sector.direction);
            float need_rotate = a - sector.angle / 2;
            Vector2 relative_cpos = circle.position - sector.position;
            Vector2 local_cpos = GetRotatePoint(relative_cpos, -need_rotate);
            float sqrt = circle.radius * circle.radius;
            float max_r = circle.radius + sector.radius;
            float dis_sqrt = local_cpos.sqrMagnitude;

            //简单距离检测
            if (dis_sqrt <= sqrt) { return true; }                  //扇型圆心在圆内
            if (dis_sqrt > max_r * max_r) { return false; }         //圆的圆心在扇型半径+圆半径外

            //下侧检测
            if ((local_cpos - new Vector2(sector.radius, 0)).sqrMagnitude <= sqrt) { return true; }        //终点在圆内
            if (local_cpos.y <= circle.radius)
            {
                if (local_cpos.y < -circle.radius) { return false; }                                       //圆在太下方
                if (local_cpos.x >= 0 && local_cpos.x <= sector.radius) { return true; }                   //与下方直线相交
            }

            //角度检测
            float local_ca = GetAngle(local_cpos);
            if (local_ca < 0) { return false; }
            if (local_ca <= sector.angle) { return true; }

            //上侧检测
            float need_rotate2 = a + sector.angle / 2;
            Vector2 local_cpos2 = GetRotatePoint(relative_cpos, -need_rotate2);
            if ((local_cpos2 - new Vector2(sector.radius, 0)).sqrMagnitude <= sqrt) { return true; }           //终点在圆内
            if (local_cpos2.y <= circle.radius) { return true; }                                               //与上方直线相交

            return false;
        }

        /// <summary>
        /// 检测圆与环型是否碰撞。
        /// </summary>
        /// <param name="cpos">圆数据。</param>
        /// <param name="ring">环形数据。</param>
        /// <returns>是否碰撞。</returns>
        public static bool CheckCollision(ShapeCircle circle, ShapeRing ring)
        {
            float min_r = Mathf.Max(0, ring.radius_in - circle.radius);
            float max_r = ring.radius_out + circle.radius;
            float dis_sqrt = (circle.position - ring.position).sqrMagnitude;
            if (dis_sqrt >= min_r * min_r && dis_sqrt <= max_r * max_r) { return true; }

            return false;
        }

        /// <summary>
        /// 将角度转换成Y旋转。
        /// </summary>
        /// <param name="a">要转换的角度值。</param>
        /// <returns>Unity的Y旋转值。</returns>
        public static float ConvertAngleToDirection(float a)
        {
            return NormalizeAngle(90 - a);
        }

        /// <summary>
        /// 将Y旋转转换成角度。
        /// </summary>
        /// <param name="a">要转换的Y旋转值。</param>
        /// <returns>角度值。</returns>
        public static float ConvertDirectionToAngle(float r)
        {
            return NormalizeAngle(90 - r);
        }

        /// <summary>
        /// 获取一个点绕原点旋转后的位置。
        /// </summary>
        /// <param name="point">点坐标。</param>
        /// <param name="a">旋转角度。</param>
        /// <returns>旋转后的位置。</returns>
        public static Vector2 GetRotatePoint(Vector2 point, float a)
        {
            //原点不旋转。
            if (Mathf.Abs(point.x) < Mathf.Epsilon && Mathf.Abs(point.y) < Mathf.Epsilon)
            {
                return point;
            }

            float len = point.magnitude;
            float angle = Mathf.Atan2(point.y, point.x) + a * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * len;
            float y = Mathf.Sin(angle) * len;
            return new Vector2(x, y);
        }

        /// <summary>
        /// 获取一个点相对原点的角度。
        /// </summary>
        /// <param name="point">点坐标。</param>
        /// <returns>相对原点的角度。</returns>
        public static float GetAngle(Vector2 point)
        {
            float a = Mathf.Atan2(point.y, point.x) * Mathf.Rad2Deg;
            return NormalizeAngle(a);
        }

        /// <summary>
        /// 规范化角度[0，360)。
        /// </summary>
        /// <param name="a">要调整的角度。</param>
        /// <returns></returns>
        public static float NormalizeAngle(float a)
        {
            while (a < 0) { a += 360; }
            while (a >= 360) { a -= 360; }
            return a;
        }
    }
}