# 🎰 Gameble — A 3×3 Slot Machine Game

A lightweight Unity WebGL slot machine built as an internship project. Features classic 3×3 reel spinning, weighted RNG, credit/bet management, and a jackpot trigger.

**▶ Play the game:** https://stellar009.itch.io/gameble

---

## 📑 Table of Contents

1. [Game Overview](#-game-overview)
2. [Instructions to Run WebGL Build](#-instructions-to-run-webgl-build)
3. [Bonus Features](#-bonus-features)
4. [Thought Process & Approach](#-thought-process--approach)

---

## 🎮 Game Overview

**Gameble** is a classic 3-reel, 3-row slot machine game where the player pulls a lever (or presses a key) to spin three independent reels in hopes of landing three matching symbols on the middle payline.

### Core Loop
1. **Pull the lever** (Left Mouse Button or Spacebar) → Bet panel appears.
2. **Choose a bet** using the bet buttons (e.g., Bet 10, Bet 50, Bet 100, or All-In).
3. **Click SPIN** → Credits are deducted, all three reels begin spinning.
4. **Reels stop in sequence** (Reel 0 → Reel 1 → Reel 2) for a satisfying staggered effect.
5. **Win evaluation** — If the middle row shows three matching symbols, the player wins `payout × (bet / 10)` credits.
6. **Jackpot** — If the middle row shows three "Seven" symbols, the player receives a **10,000-credit jackpot** on top of the regular win.

### Symbols & Payouts

| Symbol   | Payout (3-of-a-kind) | Weight (probability) |
|----------|----------------------|----------------------|
| Cherry   | 100                  | 25                   |
| Bar      | 10                   | 25                   |
| Bell     | 50                   | 25                   |
| Seven    | 500                  | 25                   |

### Controls

| Action                       | Input                              |
|------------------------------|------------------------------------|
| Pull the lever (open bet panel) | **Left Mouse Button** or **Spacebar** |
| Select bet amount            | Click bet buttons in the bet panel |
| Spin the reels               | Click the SPIN button              |

### Tech Stack

- **Engine:** Unity 6000.6.3
- **Render Pipeline:** 2D URP (Universal Render Pipeline)
- **UI System:** Unity UI (Canvas, Screen Space - Camera)
- **Input:** Unity New Input System
- **Text:** TextMeshPro
- **Build Target:** WebGL (Brotli compression, IL2CPP backend)
- **Language:** C#

---

## 🚀 Instructions to Run WebGL Build

### Option A — Play Online (Easiest)

Just visit the hosted build:
> https://stellar009.itch.io/gameble

Click **"Run game"** and the slot machine loads directly in your browser. No download required.

### Option B — Run Locally from Source

#### Prerequisites
- Unity Hub with **Unity 6000.6.3** installed
- **WebGL Build Support** module added to your Unity installation
- A modern browser (Chrome, Firefox, Edge — must support WebAssembly 2023)

#### Steps

1. **Clone or download** this repository.
2. Open **Unity Hub** → click **Add** → select the project folder.
3. Open the project in Unity 6000.6.3 (first import may take a few minutes).
4. Open the main scene: `Assets/Scenes/GameScene.unity`.
5. To play in the editor: press the **Play** button (▶) at the top.
6. To build for WebGL:
   - Go to **File → Build Settings**.
   - Select **WebGL** from the platform list (click **Switch Platform** if needed).
   - Ensure the main scene is added to the build (index 0).
   - Click **Build** (or **Build And Run** to test locally).
7. To test the build locally, **do not open `index.html` directly** — browsers block file:// features. Instead:
   - Use **Build And Run** (Unity starts a local server automatically), or
   - Serve the build folder with `python -m http.server 8080` and visit `http://localhost:8080`.

### Player Flow in the Browser

1. Game loads (typically < 5 seconds on a normal connection).
2. Title screen / slot machine appears.
3. **Pull the lever** with Left Mouse Button or Spacebar.
4. Bet panel slides in — choose a bet amount.
5. Click **SPIN** to start the reels.
6. Watch for matching symbols on the middle row!

---

## 🎁 Bonus Features

**This build contains no bonus features.**

The implementation focuses on the core slot-machine loop: pull → bet → spin → evaluate → payout. Bonus rounds, free spins, wild/scatter symbols, multi-payline evaluation, and progression systems have intentionally been excluded to keep scope tight and ship a clean MVP within the project timeline.

---

## 🧠 Thought Process & Approach

### 1. Architecture — Separation of Concerns

Before writing any code, I planned a layered architecture so each system has a single responsibility. The slot machine is divided into three independent layers that communicate only through well-defined APIs:

| Layer          | Responsibility                                                        |
|----------------|------------------------------------------------------------------------|
| `SymbolData`   | Pure data container (ScriptableObject). Holds symbol identity, sprite, payout, and probability weight. Knows nothing about the game. |
| `Reel`         | Owns one vertical column of 3 symbols. Generates its own random result, animates the spin, and exposes `FinalSymbols` for evaluation. |
| `SpinManager`  | Orchestrates the game loop. Handles the spin button, deducts credits, waits for all reels to finish, evaluates wins, and updates UI. |
| `GameInputManager` | Singleton that wraps Unity's new Input System. Listens for the "Pull" action and tells SpinManager to open the bet panel. |

**Why this matters:** Reels don't know about each other or about money. SpinManager doesn't know how reels animate. SymbolData is data-only, so designers can add new symbols without touching code. This makes the codebase easy to extend (e.g., adding a 5×3 grid later requires only adding 2 more Reel instances).

### 2. RNG Strategy — Decide Outcome Before Animating

A critical design decision was **when** to generate the random result. The naive approach (pick a symbol at the moment the reel stops) creates subtle bugs and makes the animation misleading.

Instead, my approach:
1. The moment the player clicks SPIN, **each Reel generates its full 3-symbol final result** using weighted probability.
2. The result is stored internally (`finalResult` array).
3. The visual spin animation cycles random "blur" sprites purely for show.
4. When the spin timer ends, the reel snaps to the pre-decided result.

This mirrors how real slot machines work: the outcome is determined the instant the player commits, and the animation is decoration. It also makes the system trivially testable — you can mock the result and verify the animation always lands on it.

### 3. Weighted Probability via ScriptableObjects

Each `SymbolData` asset exposes a `weight` field in the Inspector. Higher weight = more frequent appearance. The weighted-random algorithm:

1. Sum all weights (e.g., `30 + 25 + 15 + 10 + 5 + 2 = 87`).
2. Pick a random float between 0 and 87.
3. Walk the symbol list, subtracting each symbol's weight from the random number.
4. When the number drops to ≤ 0, return that symbol.

This gives designers direct control over the game's RTP (Return To Player) without code changes. Cherry (weight 30) appears roughly 15× more often than Seven (weight 2), which makes the jackpot rare and exciting.

### 4. Staggered Reel Stops via Sibling Index

Each Reel calculates its spin duration as `baseSpinTime + siblingIndex × staggerDelay`. Since Reel0, Reel1, and Reel2 are siblings under the same parent in the Hierarchy, `transform.GetSiblingIndex()` returns 0, 1, and 2 respectively. This produces spin durations of 1.5s, 1.8s, and 2.1s — creating the satisfying "stop one reel at a time" effect that real slot machines have, without hardcoding reel-specific logic.

### 5. Coroutine-Based Win Evaluation

After all reels are told to spin, `SpinManager` starts a coroutine that waits via `WaitUntil(() => reels.All(r => r.FinalSymbols != null))`. Each Reel sets its `FinalSymbols` only when its animation completes, so the evaluation naturally waits for the slowest reel. This avoids fragile `Invoke` timers or hardcoded delays — the win evaluation fires the instant the last reel stops.

A critical detail: `StartSpin()` sets `FinalSymbols = null` before generating a new result. Without this, the `WaitUntil` check would immediately pass on the second spin (because the array would still hold the previous spin's data), evaluating an old result instead of waiting for the new one.

### 6. WebGL-Specific Optimizations

To make the build load fast and run smoothly in browsers:
- **Brotli compression** with **Decompression Fallback** enabled (works on servers without Brotli support).
- **IL2CPP backend** with **High managed stripping** — removes unused .NET code, dramatically shrinking the .wasm payload.
- **Sprite Atlas** packs all symbol sprites into a single texture, reducing draw calls.
- **Target WebAssembly 2023** for modern browsers, with the standard 256 MB initial memory (sufficient for this small game).
- **Audio unlock pattern** — WebGL blocks audio until the user interacts, so the first input (pull lever) also unlocks the AudioListener.

### 7. Why "Screen Space - Camera" for the Canvas

I chose `Screen Space - Camera` render mode (instead of the simpler `Screen Space - Overlay`) so the UI sits on a plane in front of the camera. This enables:
- Depth sorting between UI panels (bet panel can slide in front of the reels).
- Particle systems and 3D effects to render between UI layers (useful for future jackpot celebrations).
- Consistent scaling via the Canvas Scaler with a 1080×1920 reference resolution.

### 8. Trade-offs & Known Limitations

- **No save system beyond `PlayerPrefs`** — Credits persist locally but are device-specific.
- **Single payline (middle row only)** — A real slot would evaluate 5–25 paylines, but for an internship MVP one payline keeps win logic readable.
- **No bonus rounds** — Deliberately cut to fit the project timeline.

### 9. What I Learned

- The value of **deciding outcomes before animating** — applies to far more than slot games (loot boxes, gacha, card draws, any "random reward" UI).
- **ScriptableObjects** are a clean way to make game data designer-editable without recompiling.
- **WebGL has sharp edges** (Brotli headers, build size sensitivity) that desktop builds don't expose.
- **Coroutines + WaitUntil** is a powerful pattern for waiting on multi-actor state changes without busy-waiting or timers.

---

## 📁 Project Structure

```
Assets/
├── Scenes/
│   └── GameScene.unity
├── Scripts/
│   ├── Data/
│   │  └── SymbolData.cs           # ScriptableObject for symbol data
│   ├── Managers/
│   │  ├── GameInputManager.cs     # Singleton input handler (new Input System)
│   │  └── SpinManager.cs          # Game orchestrato
│   ├── Reels/
│   │  └── Reel.cs                 # One reel's spin + RNG
│   └── UnityGeneratedScripts
│      ├── GameInput.cs
│      └── GameInput.inputactions   
├── ScriptableObjects/
│   └── Symbols/
│       ├── Cherry.asset
│       ├── Bar.asset
│       ├── Bell.asset
│       └── Seven.asset
├── Art/
│   ├── Slot Machine/           # Slot Machine sprite PNGs
│   ├── UI/                     # UI sprrite PNGs
│   └── Symbols/                # Symbol sprite PNGs
```

---

## 📜 License

This project was created for an internship submission. All third-party assets (sprites, audio) are given by the organisation.

---

## 👤 Author

Built with Unity 6000.6.3. Play the live build at **https://stellar009.itch.io/gameble**.
