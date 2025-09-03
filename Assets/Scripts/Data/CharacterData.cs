using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "ADV/Character Data", order = 1)]
public class CharacterData:ScriptableObject
{
    public string characterName;
    public Sprite[] fgs;  // 不同的立绘
    public Sprite[] expressions;  // 不同表情的立绘
    public Vector2 defaultPosition = new Vector2(0, 0);
    public float defaultScale = 1f;
}