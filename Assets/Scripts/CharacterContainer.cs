using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterContainer : MonoBehaviour
{
    [Header("角色位置点")]
    public List<RectTransform> positions;
    public Vector2 GetPositions(int pos)
    {
        if (pos < positions.Count)
        {
            return positions[pos].anchoredPosition;
        }
        Debug.LogWarning($"位置点 {pos} 不存在！");
        return Vector2.zero;
    }
}
