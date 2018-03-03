using System;
using System.Collections.Generic;
using Luna3D;

public class AreaDetection
{
    /// <summary>
    /// 判断一点是否在圆内
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="center"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool PointInCircle(float radius, Vector3 center, Vector3 p)
    {
        float distance = Vector3.Distance(center, p);
        return distance <= radius;
    }

    /// <summary>
    /// 判断一点是否在圆内
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="height">圆柱的高度</param>
    /// <param name="center"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    public static bool PointInCircle(float radius, float height, Vector3 center, Vector3 p)
    {
        Vector2 vc2 = new Vector2(center.x, center.z);
        Vector2 vp2 = new Vector2(p.x, p.z);
        float distance = Vector2.Distance(vc2, vp2);

        bool inheight = p.y <= center.y + height && p.y >= center.y - height;
        return distance <= radius && inheight;
    }

    /// <summary>
    /// 判断一点是否在扇形范围内
    /// </summary>
    /// <param name="radius">半径</param>
    /// <param name="center">范围中心</param>
    /// <param name="dir">扇形朝向</param>
    /// <param name="angle">扇形角度</param>
    /// <param name="p">点的坐标</param>
    /// <returns></returns>
    public static bool PointInFan(float radius, Vector3 center, Vector3 dir, float angle, Vector3 p)
    {
        bool inCircle = PointInCircle(radius, center, p);

        Vector3 a = dir;
        Vector3 b = p - center;
        float angel = Vector3.Dot(a.normalized, b.normalized);
        float c = Mathf.Acos(angel) * Mathf.Rad2Deg;
        return c <= angle / 2 && inCircle;
    }

    public static bool PointInFan(float radius, float height, Vector3 center, Vector3 dir, float angle, Vector3 p)
    {
        bool inCircle = PointInCircle(radius, height, center, p);

        Vector3 a = dir;
        Vector3 b = p - center;
        float angel = Vector3.Dot(a.normalized, b.normalized);
        float c = Mathf.Acos(angel) * Mathf.Rad2Deg;
        return c <= angle / 2 && inCircle;
    }
}