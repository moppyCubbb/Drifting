using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bazier
{
    public static Vector2 EvaluteQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = EvaluteQuadratic(a, b, c, t);
        Vector2 p1 = EvaluteQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }
}
