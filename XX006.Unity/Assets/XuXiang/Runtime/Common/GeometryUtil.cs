using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XuXiang
{
    public static class GeometryUtil
    {
        /// <summary>
        /// 获取垂直与某个法线且经过特定点的平面方程。
        /// </summary>
        /// <param name="normal">垂直平面的法线。</param>
        /// <param name="point">要经过的点。</param>
        /// <returns>xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</returns>
        public static Vector4 GetPlane(Vector3 normal, Vector3 point)
        {
            return new Vector4(normal.x, normal.y, normal.z, -Vector3.Dot(normal, point));
        }

        /// <summary>
        /// 获取点在某个面上的投影。
        /// </summary>
        /// <param name="point">点坐标。</param>
        /// <param name="plane">平面，xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</param>
        /// <returns>投影坐标。</returns>
        public static Vector3 GetProjection(Vector3 point, Vector4 plane)
        {
            float t1 = plane.x * point.x + plane.y * point.y + plane.z * point.z + plane.w;
            float t2 = plane.x * plane.x + plane.y * plane.y + plane.z * plane.z;
            float t = t1 / t2;
            float x = point.x - plane.x * t;
            float y = point.y - plane.y * t;
            float z = point.z - plane.z * t;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// 获取三角形法线。
        /// </summary>
        /// <param name="p1">顺时针顺序分布的第一个点坐标。</param>
        /// <param name="p2">顺时针顺序分布的第二个点坐标。</param>
        /// <param name="p3">顺时针顺序分布的第三个点坐标。</param>
        /// <returns>朝向单位向量。</returns>
        public static Vector3 GetNormal(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Vector3.Cross(p2 - p1, p3 - p1).normalized;
        }

        /// <summary>
        /// 获取四边形朝向。
        /// </summary>
        /// <param name="p1">顺时针顺序分布的第一个点坐标。</param>
        /// <param name="p2">顺时针顺序分布的第二个点坐标。</param>
        /// <param name="p3">顺时针顺序分布的第三个点坐标。</param>
        /// <param name="p3">顺时针顺序分布的第四个点坐标。</param>
        /// <returns>朝向单位向量。(p1-p3割开的两个三角形朝向的平均值)</returns>
        public static Vector3 GetNormal(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            return (GetNormal(p1, p2, p3) + GetNormal(p1, p3, p4)).normalized;
        }

        /// <summary>
        /// 获取三角形面积。
        /// </summary>
        /// <param name="p1">顺时针顺序分布的第一个点坐标。</param>
        /// <param name="p2">顺时针顺序分布的第二个点坐标。</param>
        /// <param name="p3">顺时针顺序分布的第三个点坐标。</param>
        /// <returns>三角形面积。</returns>
        public static float GetArea(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return Vector3.Cross(p2 - p1, p3 - p1).magnitude / 2;
        }

        /// <summary>
        /// 获取两个向量的夹角。
        /// </summary>
        /// <param name="a">向量A。</param>
        /// <param name="b">向量B。</param>
        /// <returns>向量夹角[0-180]。</returns>
        public static float GetAngle(Vector3 a, Vector3 b)
        {
            float dot = Vector3.Dot(a, b);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
            return angle;
        }

        /// <summary>
        /// 获取从一个向量逆时针旋转到另一个向量需要的角度。
        /// </summary>
        /// <param name="from">起始向量。</param>
        /// <param name="to">结束向量。</param>
        /// <param name="dir">观察面的方向。</param>
        /// <param name="projection">是否将向量投影到观察方向垂直的平面上计算。</param>
        /// <returns>旋转角度[0-180]。</returns>
        public static float GetAngle(Vector3 from, Vector3 to, Vector3 dir, bool projection = true)
        {
            if (projection)
            {
                Vector4 plane = GeometryUtil.GetPlane(dir, Vector3.zero);
                from = GeometryUtil.GetProjection(from, plane).normalized;
                to = GeometryUtil.GetProjection(to, plane).normalized;
            }

            Vector3 cross = Vector3.Cross(to, from);
            float dot_dir = Vector3.Dot(dir, cross);
            float dot = Vector3.Dot(from, to);
            float angle = Mathf.Asin(cross.magnitude) * Mathf.Rad2Deg;         //0 ~ 90
            if (dot_dir > 0)
            {
                if (dot < 0)
                {
                    angle = 180 - angle;
                }
            }
            else
            {
                if (dot < 0)
                {
                    angle = 180 + angle;
                }
                else
                {
                    angle = 360 - angle;
                }
            }
            return angle;
        }

        /// <summary>
        /// 通过三点获取一个平面方程。点的顺时针的面表示正面。
        /// </summary>
        /// <param name="a">A点。</param>
        /// <param name="b">B点。</param>
        /// <param name="c">C点。</param>
        /// <returns>xyzw分别表示平面方程Ax+By+Cz+D=0里的ABCD。</returns>
        public static Vector4 GetPlane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            return GetPlane(normal, a);
        }

        /// <summary>
        /// 获取摄像机视锥体远平面的四个点。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左下，右下，左上，右上四个点的世界坐标。</returns>
        public static Vector3[] GetCameraFarClipPlanePoint(Camera camera)
        {
            Vector3[] points = new Vector3[4];
            GetCameraFarClipPlanePoint(camera, points);
            return points;
        }

        /// <summary>
        /// 获取摄像机视锥体远平面的四个点。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左下，右下，左上，右上四个点的世界坐标。</returns>
        public static Vector3[] GetCameraFarClipPlanePoint(Camera camera, Vector3[] points)
        {
            Transform transform = camera.transform;
            float distance = camera.farClipPlane;
            float halfFovRad = Mathf.Deg2Rad * camera.fieldOfView * 0.5f;
            float upLen = distance * Mathf.Tan(halfFovRad);
            float rightLen = upLen * camera.aspect;
            Vector3 farCenterPoint = transform.position + distance * transform.forward;
            Vector3 up = upLen * transform.up;
            Vector3 right = rightLen * transform.right;
            points[0] = farCenterPoint - up - right;//left-bottom
            points[1] = farCenterPoint - up + right;//right-bottom
            points[2] = farCenterPoint + up - right;//left-up
            points[3] = farCenterPoint + up + right;//right-up
            return points;
        }

        /// <summary>
        /// 缓存计算过程中的顶点数组，减少GC。
        /// </summary>
        private static Vector3[] s_CachePoints = new Vector3[4];

        /// <summary>
        /// 获取摄像机视锥体的六个平面。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左右下上近远六个面方程数组。</returns>
        public static Vector4[] GetFrustumPlane(Camera camera)
        {
            Vector4[] planes = new Vector4[6];
            GetFrustumPlane(camera, planes);
            return planes;
        }

        /// <summary>
        /// 获取摄像机视锥体的六个平面。
        /// </summary>
        /// <param name="camera">摄像机对象。</param>
        /// <returns>左右下上近远六个面方程数组。</returns>
        public static Vector4[] GetFrustumPlane(Camera camera, Vector4[] planes)
        {
            Transform transform = camera.transform;
            Vector3 cameraPosition = transform.position;
            Vector3[] points = GetCameraFarClipPlanePoint(camera, s_CachePoints);

            //按顺时针传入点坐标。
            planes[0] = GetPlane(cameraPosition, points[0], points[2]);     //left
            planes[1] = GetPlane(cameraPosition, points[3], points[1]);     //right
            planes[2] = GetPlane(cameraPosition, points[1], points[0]);     //bottom
            planes[3] = GetPlane(cameraPosition, points[2], points[3]);     //up
            planes[4] = GetPlane(-transform.forward, transform.position + transform.forward * camera.nearClipPlane);//near
            planes[5] = GetPlane(transform.forward, transform.position + transform.forward * camera.farClipPlane);//far
            return planes;
        }

        /// <summary>
        /// 判断一个点是否在平面法线指向的那一侧(外侧)。
        /// </summary>
        /// <param name="plane">平面方程。</param>
        /// <param name="point">点坐标。</param>
        /// <returns>是否在外侧。</returns>
        public static bool IsOutsideThePlane(Vector4 plane, Vector3 point)
        {
            return Vector3.Dot(plane, point) + plane.w > 0;
        }

        /// <summary>
        /// 判断两个凸多面体是否相交。
        /// </summary>
        /// <param name="points">凸面体A的顶点列表。</param>
        /// <param name="planes">凸面体B的面列表。(法线指向外侧)</param>
        /// <returns></returns>
        public static bool IsIntersect(Vector4[] points, Vector4[] planes)
        {
            bool show = true;
            for (int i = 0; i < planes.Length && show; ++i)
            {
                int out_count = 0;
                for (int j = 0; j < points.Length; ++j)
                {
                    if (IsOutsideThePlane(planes[i], points[j]))
                    {
                        ++out_count;
                    }
                    else
                    {
                        break;
                    }
                }
                if (out_count == points.Length)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取边框的8个点。
        /// </summary>
        /// <param name="center">边框中心。</param>
        /// <param name="size">边框大小。</param>
        /// <param name="points">保存边框八个点的数组。</param>
        /// <returns>保存边框八个点的数组。/returns>
        public static Vector4[] GetBoundPoints(Vector3 center, Vector3 size, Vector4[] points)
        {
            Vector3 boundMin = center - size / 2;
            Vector3 boundMax = center + size / 2;
            points[0] = new Vector4(boundMin.x, boundMin.y, boundMin.z, 1);
            points[1] = new Vector4(boundMax.x, boundMax.y, boundMax.z, 1);
            points[2] = new Vector4(boundMax.x, boundMax.y, boundMin.z, 1);
            points[3] = new Vector4(boundMax.x, boundMin.y, boundMax.z, 1);
            points[4] = new Vector4(boundMax.x, boundMin.y, boundMin.z, 1);
            points[5] = new Vector4(boundMin.x, boundMax.y, boundMax.z, 1);
            points[6] = new Vector4(boundMin.x, boundMax.y, boundMin.z, 1);
            points[7] = new Vector4(boundMin.x, boundMin.y, boundMax.z, 1);
            return points;
        }

        /// <summary>
        /// 获取边框的8个点。
        /// </summary>
        /// <param name="boundMin">边框最小位置。</param>
        /// <param name="boundMax">边框最大位置。</param>
        /// <param name="points">保存边框八个点的数组。</param>
        /// <returns>保存边框八个点的数组。/returns>
        public static Vector4[] GetBoundPointsForAABB(Vector3 boundMin, Vector3 boundMax, Vector4[] points)
        {
            points[0] = new Vector4(boundMin.x, boundMin.y, boundMin.z, 1);
            points[1] = new Vector4(boundMax.x, boundMax.y, boundMax.z, 1);
            points[2] = new Vector4(boundMax.x, boundMax.y, boundMin.z, 1);
            points[3] = new Vector4(boundMax.x, boundMin.y, boundMax.z, 1);
            points[4] = new Vector4(boundMax.x, boundMin.y, boundMin.z, 1);
            points[5] = new Vector4(boundMin.x, boundMax.y, boundMax.z, 1);
            points[6] = new Vector4(boundMin.x, boundMax.y, boundMin.z, 1);
            points[7] = new Vector4(boundMin.x, boundMin.y, boundMax.z, 1);
            return points;
        }
    }
}