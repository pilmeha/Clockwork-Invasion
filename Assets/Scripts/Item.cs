using UnityEngine;

[CreateAssetMenu(menuName = "Match-3/Item")]
public sealed class Item : ScriptableObject
{
    public int value;
    public bool isEnergy; // плитка энергии
    public Sprite sprite;
}
