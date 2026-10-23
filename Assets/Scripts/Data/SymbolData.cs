using UnityEngine;

[CreateAssetMenu(fileName = "NewSymbol", menuName = "Slot/Symbol", order = 0)]
public class SymbolData : ScriptableObject
{
    [Header("Identity")]
    public string symbolName;        // "Cherry", "Seven", etc.

    [Header("Visual")]
    public Sprite sprite;            // The image shown on reels

    [Header("Economy")]
    [Tooltip("Payout when 3 matching symbols land on payline")]
    public int payout = 10;          // e.g., Cherry=10, Seven=500

    [Tooltip("Relative probability weight (higher = more common)")]
    [Range(1, 100)]
    public float weight = 10;        // e.g., Cherry=30, Seven=2
}