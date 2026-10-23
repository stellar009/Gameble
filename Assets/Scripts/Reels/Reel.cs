using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class Reel : MonoBehaviour
{
    public SymbolData[] allSymbols;
    public Image[] visibleSlots;
    public float baseSpinTime = 1.5f;
    public float staggerDelay = 0.3f;
    public float cycleInterval = 0.05f;

    private bool isSpinning;
    public bool IsSpinning => isSpinning;
    public SymbolData[] FinalSymbols { get; private set; }

    public void StartSpin()
    {
        isSpinning = true;
        FinalSymbols = null;    //clear previous result

        SymbolData[] finalResult = new SymbolData[3];
        for (int i = 0; i < 3; i++)
            finalResult[i] = GetWeightedRandomSymbol();

        StartCoroutine(SpinRoutine(finalResult));
    }

    IEnumerator SpinRoutine(SymbolData[] finalResult)
    {
        float spinTime = baseSpinTime + transform.GetSiblingIndex() * staggerDelay;
        float elapsed = 0;

        while (elapsed < spinTime)
        {
            for (int i = 0; i < visibleSlots.Length; i++)
                visibleSlots[i].sprite = allSymbols[Random.Range(0, allSymbols.Length)].sprite;

            elapsed += cycleInterval;          
            yield return new WaitForSeconds(cycleInterval);
        }

        for (int i = 0; i < 3; i++)
            visibleSlots[i].sprite = finalResult[i].sprite;

        FinalSymbols = finalResult;
        isSpinning = false;
    }

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
        return allSymbols[0];
    }
}