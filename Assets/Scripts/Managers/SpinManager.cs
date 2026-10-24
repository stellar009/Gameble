using TMPro;
using UnityEngine;
using System.Collections;
using System.Linq;

// Orchestrates the slot machine: handles spin flow, win evaluation,
// credits/bet UI, jackpot, and the bet panel + handle animations.
public class SpinManager : MonoBehaviour
{
    [Header("References")]
    public Reel[] reels;                      // 3 reels, assigned in Inspector
    public TextMeshProUGUI creditsText;        // Credits display
    public TextMeshProUGUI winText;            // Win amount display
    public Animator handle;                    // Slot machine lever animation
    public GameObject betPanel;                // Panel where player picks bet

    [Header("Economy")]
    public int betAmount = 10;                 // Current bet
    public int startingCredits = 1000;         // Initial credits on first load

    private Animator betPanelAnimator;         // Animator on betPanel (slide in/out)
    private int credits;                        // Current player credits
    private bool isSpinning;                    // Lock to prevent double-spins

    void Start()
    {
        betPanelAnimator = betPanel.GetComponent<Animator>();

        // Bet panel starts hidden (player must pull lever first).
        EnableBetPanel(false);

        credits = startingCredits;
        UpdateUI();
    }

    // Called by the SPIN button (Unity Event in Inspector).
    public void OnSpinClicked()
    {
        // Close the bet panel as soon as player commits to spin.
        EnableBetPanel(false);

        // Bail out if already spinning or not enough credits.
        if (isSpinning || credits < betAmount) return;

        // Play lever-pull animation, then reset it after 0.5s.
        handle.SetBool("Pulled", true);
        Invoke(nameof(ResetHandle), 0.5f);

        credits -= betAmount;
        UpdateUI();
        winText.text = "";
        isSpinning = true;

        // Tell all 3 reels to start spinning (each handles its own RNG + animation).
        foreach (var reel in reels)
            reel.StartSpin();

        StartCoroutine(CheckResultsAfterSpin());
    }

    // Wait for all reels to finish, then evaluate the result.
    IEnumerator CheckResultsAfterSpin()
    {
        // FinalSymbols is null during spin, populated when reel stops.
        yield return new WaitUntil(() => reels.All(r => r.FinalSymbols != null));

        EvaluateWin();
        isSpinning = false;
        UpdateUI();
    }

    // Checks the middle row (index 1 of each reel's 3 symbols) for 3-of-a-kind.
    void EvaluateWin()
    {
        var mid = new[] {
            reels[0].FinalSymbols[1],
            reels[1].FinalSymbols[1],
            reels[2].FinalSymbols[1]
        };

        if (mid[0] == mid[1] && mid[1] == mid[2])
        {
            // Payout scales with bet (bet/10 = multiplier of base payout).
            int win = mid[0].payout * (betAmount / 10);
            credits += win;
            winText.text = $"WON: {win}";

            // Jackpot: all 3 middle symbols are "Seven".
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

    // Called via Invoke after 0.5s to return lever to rest position.
    void ResetHandle()
    {
        handle.SetBool("Pulled", false);
    }

    // Called by bet buttons (e.g., "Bet 10", "Bet 50", "Bet 100").
    public void SetBetAmount(int amount)
    {
        if (credits == 0) return;
        betAmount = amount;
    }

    // "All-In" button — bet everything the player has.
    public void AllInAmount()
    {
        if (credits == 0) return;
        betAmount = credits;
    }

    // Toggles the bet panel. GameInputManager calls this with true when
    // player pulls the lever; OnSpinClicked calls it with false to hide.
    public void EnableBetPanel(bool enable)
    {
        betPanelAnimator.SetBool("isClosed", enable);
        betPanel.SetActive(enable);
    }
}