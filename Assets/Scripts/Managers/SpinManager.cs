using TMPro;
using UnityEngine;
using System.Collections;
using System.Linq;

public class SpinManager : MonoBehaviour
{
    public Reel[] reels;
    public TextMeshProUGUI creditsText;
    public TextMeshProUGUI winText;    
    public int betAmount = 10;
    public int startingCredits = 1000;
    public Animator handle;

    public GameObject betPanel;

    private Animator betPanelAnimator;
    private int credits;
    private bool isSpinning;

    void Start()
    {
        betPanelAnimator = betPanel.GetComponent<Animator>();

        EnableBetPanel(false);

        credits = startingCredits;
        UpdateUI();
    }

    public void OnSpinClicked()
    {
        EnableBetPanel(false);

        if (isSpinning || credits < betAmount) return;

        handle.SetBool("Pulled", true);
        Invoke(nameof(ResetHandle), 0.5f);

        credits -= betAmount;
        UpdateUI();                            // update after deduction
        winText.text = "";
        isSpinning = true;

        foreach (var reel in reels)
            reel.StartSpin();

        StartCoroutine(CheckResultsAfterSpin());
    }

    IEnumerator CheckResultsAfterSpin()
    {
        yield return new WaitUntil(() => reels.All(r => r.FinalSymbols != null));

        EvaluateWin();
        isSpinning = false;
        UpdateUI();                            // update after win
    }

    void EvaluateWin()
    {
        var mid = new[] {
            reels[0].FinalSymbols[1],
            reels[1].FinalSymbols[1],
            reels[2].FinalSymbols[1]
        };

        if (mid[0] == mid[1] && mid[1] == mid[2])
        {
            int win = mid[0].payout * (betAmount / 10);
            credits += win;
            winText.text = $"WON: {win}";

            if (mid[0].symbolName == "Seven")
                TriggerJackpot();
        }
    }

    void TriggerJackpot()
    {
        credits += 10000;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {credits}";
    }

    void ResetHandle()
    {
        handle.SetBool("Pulled", false);
    }

    public void SetBetAmount(int amount)
    {
        if (credits == 0) return;

        betAmount = amount;
    }

    public void AllInAmount()
    {
        if (credits == 0) return;

        betAmount = credits;
    }

    public void EnableBetPanel(bool enable)
    {
        betPanelAnimator.SetBool("isClosed", enable);

        betPanel.SetActive(enable);
    }

    public void ExitSlotMachine()
    {
        Application.Quit();
    }
}