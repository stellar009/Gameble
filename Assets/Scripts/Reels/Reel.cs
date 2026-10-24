using System.Collections;
using UnityEngine.UI;
using UnityEngine;

// One vertical column of the slot machine. Spins independently,
// generates its own random result up front, then animates to it.
public class Reel : MonoBehaviour
{
    [Header("Data")]
    public SymbolData[] allSymbols;     // 6 symbol assets, wired in Inspector

    [Header("Visual")]
    public Image[] visibleSlots;        // 3 UI Images (top/mid/bot) wired in Inspector

    [Header("Spin Timing")]
    public float baseSpinTime = 1.5f;   // Reel0's spin duration
    public float staggerDelay = 0.3f;   // Extra time per reel (staggered stop)
    public float cycleInterval = 0.05f; // Symbol swap frequency during spin

    private bool isSpinning;
    public bool IsSpinning => isSpinning;

    // Null during spin, populated when reel stops. SpinManager reads this.
    public SymbolData[] FinalSymbols { get; private set; }

    // Called by SpinManager. Generates final result FIRST, then animates to it.
    public void StartSpin()
    {
        isSpinning = true;
        FinalSymbols = null;   // clear previous result so SpinManager waits again

        SymbolData[] finalResult = new SymbolData[3];
        for (int i = 0; i < 3; i++)
            finalResult[i] = GetWeightedRandomSymbol();

        StartCoroutine(SpinRoutine(finalResult));
    }

    // Visual spin: cycle random symbols, then snap to the pre-decided result.
    IEnumerator SpinRoutine(SymbolData[] finalResult)
    {
        // Reel0 = 1.5s, Reel1 = 1.8s, Reel2 = 2.1s (dramatic staggered stops)
        float spinTime = baseSpinTime + transform.GetSiblingIndex() * staggerDelay;
        float elapsed = 0;

        while (elapsed < spinTime)
        {
            // Cycle random sprites as "blur frames" — pure visual, not the result.
            for (int i = 0; i < visibleSlots.Length; i++)
                visibleSlots[i].sprite = allSymbols[Random.Range(0, allSymbols.Length)].sprite;

            elapsed += cycleInterval;   // keep consistent with WaitForSeconds below
            yield return new WaitForSeconds(cycleInterval);
        }

        // Reveal final symbols
        for (int i = 0; i < 3; i++)
            visibleSlots[i].sprite = finalResult[i].sprite;

        FinalSymbols = finalResult;
        isSpinning = false;
    }

    // Weighted random pick: higher weight = more frequent.
    SymbolData GetWeightedRandomSymbol()
    {
        float totalWeight = 0;
        foreach (var s in allSymbols) totalWeight += s.weight;

        float r = Random.value * totalWeight;
        foreach (var s in allSymbols)
        {
            r -= s.weight;
            if (r <= 0) return s;
        }
        return allSymbols[0];   // safety fallback (should never reach here)
    }
}