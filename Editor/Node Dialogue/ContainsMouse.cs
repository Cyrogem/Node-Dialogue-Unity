using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ContainsMouse
{
    public static bool Check(Vector2 mousePosition, Rect rect)
    {
        return (mousePosition.x > rect.xMin && mousePosition.x < rect.xMax &&
                mousePosition.y > rect.yMin && mousePosition.y < rect.yMax);
    }
}
