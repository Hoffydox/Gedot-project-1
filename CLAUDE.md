# Project notes

## Working across devices

This project is worked on from two machines:
- Windows 11 desktop (original)
- Linux (CachyOS) laptop

Claude's memory is local to each machine and does not transfer — anything that needs to carry over between devices must go through this repo (commits, this file, etc.), not Claude's memory. **Keep this file updated** when the plan, current step or working style changes.

- Always `git pull` before starting and commit + push when done; avoid editing on both machines without syncing (`.tscn` scene files merge badly).
- Toolchain on both machines: **Godot 4.7.2 .NET (mono)** — exact version — and the **.NET 8 SDK**.
- `.vscode/launch.json` reads the Godot executable from the `GODOT4` environment variable, set per machine.
- Commits show which machine made them through the git author name (same email on both): **`Andreas Hoffmann (Desktop)`** on Windows, **`Andreas Hoffmann (Laptop)`** on Linux. If the laptop isn't set up yet: `git config --global user.name "Andreas Hoffmann (Laptop)"`. (Older commits use `Hoffmann15` = desktop and `Andreas Hoffmann` = laptop.)

## The user is learning C# — how to help

Learning C# is the main goal of this project, not just shipping the game.

- **Default style:** explain the concept and the goal, give a **pseudocode "shape"** of the solution, the user writes the code, then review it. Don't write gameplay code for them unless asked.
- **No full-code handholding** unless they explicitly ask, or the topic is extremely hard and completely new.
- **Current preference (since 2026-10-30): more hand-holding.** The user asked for more help for now: give the full code in small numbered steps, say exactly where each piece goes, and explain it line by line. The user still types it in themselves, then I review. Go back to the lighter pseudocode style only when they say so. A *little* more concrete help works well: exact corrected syntax structures for syntax errors, and short explained snippets for brand-new APIs.
- **Recap recurring concepts** briefly when they come up again rather than assuming they stuck: class vs instance, fields vs locals (and `var` shadowing a field), signals, properties, guards and code order, `_Ready` override replacing the base class's `_Ready`.
- Recurring slips to watch for: autocomplete adding stray `using` lines; case-sensitive node paths / action names / group names; `else` after an early `return`; forgetting to assign `[Export]` scene slots in the Inspector.
- Review the user's code by reading the files and the Godot log; build with `dotnet build First-project.csproj`.

## Vision and design pillars

**The core fantasy:** you're a contract prospector on a hostile alien moon — not a soldier or a hero, but someone doing a dangerous job for money with gear that's practical, worn and limited. The wildlife isn't evil; it just doesn't want you there. Every drill you set down is you *choosing* to make noise and invite trouble. At heart it's an **extraction game**: risk → reward → bank it before you lose it.

Pillars (use these to judge new ideas):
1. **Weight and maintenance over disposability** — gear is looked after (repair, resupply, carry weight), not consumed.
2. **Meaningful choices over stat bumps** — where to drill, what to buy with limited credits, whether to hide while the drill gets chewed up, friendly-fire risk near your own defences.
3. **Systems that interact** — fire hurts everyone and sandstorms put it out; dust blocks sight so the radar matters; walls block, so enemies attack what's in the way.
4. **Atmosphere and tension through information** — radar pings in the dark, night, sandstorms, dust: *not seeing* is part of the feel.
5. **Readable, fair combat** — dodgeable wind-ups, knockback with impact, recovery after missed attacks; deliberate even when a swarm is overwhelming.
6. **Escalation by choice** — deposit tiers scale the swarm; the player decides how much risk to take; manual mining is the safe fallback.

**On art, feel and vision:** questions and inspiration ideas are welcome (references, options, "what if" suggestions, concept placeholders) — but **the user wants to create the final art themselves**. Treat any art made by Claude (e.g. the SVGs in `assets/sprites/concepts/`) as placeholders and inspiration, not final assets.

## The game

Top-down 2D arena game in Godot 4.7 + C#, with a **fake 2.5D look** planned (angled art + Y-sort); real 3D is a possible future step.

**Theme:** a contract prospector on a hostile alien moon, inspired by the film *Prospect* (2018) — weight, scarcity, purpose, analog "used future" tech, but avoiding the overused rust/amber-CRT look. Visual direction still open: concept A (sun-bleached) is the most developed; concept B (sewn/taped gear + crystal fauna) is the alternative. Art lives in `assets/sprites/concepts/`.

**Art style — decided: pixel art, 32px player, drawn by the user in Aseprite.** Chosen from the density test in `assets/sprites/concepts/pixel_density/` (16/24/32/48px compared at equal on-screen size).
- **Render setup:** base resolution **640×360**, stretch mode `viewport`, scale mode `integer` (×3 at 1080p, ×4 at 1440p), default texture filter **Nearest**, **fullscreen**. 1 art pixel = 1 world unit at camera zoom 1.
- **Sizes (rough):** player ~32px tall, skitter ~16–18px, basic enemy ~40px, drill/turret ~32–48px. Never scale a sprite to fit; redraw it at the right size.
- **Pivot at the feet** (sprite offset so the node origin is where it touches the ground) for Y-sort and the fake 2.5D look.
- **One shared palette** (16–32 colours) across all sprites.
- **Font:** pixel font **m5x7** (`assets/fonts/m5x7.ttf`, imported with antialiasing/hinting off) set as the project's custom font; default theme scale 1. Use font size **16** (or 32) only — pixel fonts blur at other sizes. Anything made by someone else (fonts, sounds, etc.) gets a line in `CREDITS.md`.
- **Files:** `.aseprite` sources in `art_src/` (contains a `.gdignore` so Godot skips it); exported PNGs in `assets/sprites/`. Commit both. Static sprites → `Sprite2D`; animations → sprite-sheet export → `AnimatedSprite2D` + SpriteFrames.
- **Switch-over plan:** (A) change the render settings and rescale the world ÷3 (collision radii, speeds, ranges, spawn distances, camera zoom 0.6 → 1, HUD font sizes) in one commit on one machine; (B) then replace placeholder sprites one at a time, starting with the player.

**Core loop:** roam (wandering enemies, scrap) → place a **drill** → the drill triggers a **swarm** → defend with turrets, walls and your gun → collect ore + scrap → sell at the **settlement** (safe zone + merchant) for credits → buy turrets, walls, ammo → drill again. You lose carried (unsold) loot on death.

**Main systems:** `WaveManager` (Calm/Swarm, started by the drill), `EnemySpawner` (single enemies + skitter packs), `Enemy` (Wander/Chase/Attack state machine, knockback/stun, separation, targets the closest valid target), `Deployable` base class (`Drill`, `Turret`, `Wall`) implementing `IDamageable`, `Pickup` (ore/scrap), `Settlement` (shop), `Hud`, `Main` (wires signals together). Physics layers: 1 player, 2 enemies, 3 deployables.

## Where we are (update this as steps finish)

Phases 1–4 are done (movement/combat basics, waves + HUD, deployables, economy loop + gun). Now on the **side quest**:

- Done: 18 enemy attack wind-up + knockback/stun · 19 skitter swarm enemy · 19b movement/collision polish (floating motion, circles, layers, separation) · 20 friendly fire + hit flash · 21 walls + generalized inventory (`Dictionary<Deployable.Kind, int>`), `PlaceDeployable(kind)`, generic `BuyItem` · a code cleanup pass (scripts organized into settings / state / read-only access / grouped methods; typed `EmitSignal...` helpers everywhere).
- Done: **pixel-art switch-over step A** — 640×360 viewport/integer/Nearest/fullscreen, world rescaled ÷3 (placeholder SVGs shrunk via import scale 0.333, terrain decals kept their old positions so they're sparser), snap-to-pixel + physics interpolation on. With interpolation on, **anything spawned and then moved must call `ResetPhysicsInterpolation()` after its last position/rotation change** (otherwise it flickers in from (0,0)).
- Done: **step B for the player** — the user's own 32px player (Nanner 32 palette, `art_src/player/`) is in the game. **4 directions = 3 drawings** (down, up, side; west = side with `FlipH`). `player.tscn` has a `Body` `AnimatedSprite2D` with animations named **`pose_direction`** (`idle_down/up/side` 2 frames, `aim_down/up/side` 1 frame) from `assets/sprites/player/player_poses.png` (3×3 grid). `PlayerCharacter.UpdateAnimation()` picks the direction from the mouse (compare `|X|` vs `|Y|`) and `aim` while shoot is held. The gun is drawn into the body for now (a separate rotating gun sprite is the alternative if diagonal aiming looks off). Possible polish: spawn bullets at the gun muzzle instead of the player's centre.
- Done: **step 22** pick up turrets — press E (`interact`) within `pickupRange` of a turret; it goes back into the inventory and its state is pushed onto `Stack<TurretState> storedTurrets` in `PlayerCharacter`. Placing a turret pops it and calls `turret.LoadState()` (after `AddChild`, since `_Ready` resets health). Bought turrets are placed at full health.
- Done: **step 23** turret maintenance, split into small steps (design agreed 2026-10-30):
  - Done: **23a** turrets get ammo (magazine, 1 round per shot, ~30), lifetime timer removed; the pickup stack remembers health **and** ammo (a small `TurretState` class).
  - Done: **23b** **ammo types**: `enum AmmoType` (`Small` for now; later `Medium`/`Heavy` for new guns/turrets). The handgun and current turrets both use Small, so turret ammo comes from the player's own supply (a meaningful choice). Player ammo becomes `Dictionary<AmmoType, int>`, like the inventory, with `GetAmmo(type)` / `TryAddAmmo(type, amount)`; the gun's type is `gunAmmoType`, the turret's is `ammoType`; `AmmoChanged` sends the type.
  - Done: **23c** prompt above the nearest turret in range: each turret has a hidden `PromptLabel` + `ShowPrompt(bool)`; `PlayerCharacter.UpdateFocusedTurret()` keeps a `focusedTurret` field and only switches when the closest turret changes. Prompts in the field, menus only in the settlement.
  - Done: **23d** **timed-action system**: hold E (`interact`) to pick up (`pickUpTime` 1 s) or R (`refill`) to refill (`refillTime` 1.5 s, the turret takes `Min(room, what you have)` of its `AmmoType`). `PlayerCharacter` has `enum TurretAction` and `UpdateTurretAction()` (only does timing; restarts when the key, the action or the focused turret changes) plus one `switch` helper per question: `CanDoTurretAction` (checked every frame, so the bar never appears for impossible actions), `GetActionTime`, `GetActionText`, `FinishTurretAction`. The turret just displays: `ShowProgress(text, 0..1)` / `HideProgress()` on its `ActionBar` + `PromptLabel`. Holding E keeps picking up nearby turrets one after another (intended).
  - Done: **23e** **repair**: hold Q (`repair`), every `repairTime` (set to 1 s) costs 1 scrap for +`repairPerScrap` (5) HP and repeats while held (the timed-action system restarts by itself) until full or out of scrap; text shows "Repairing 25/40". `Deployable` has `MaxHealth`, `NeedsRepair` and `Repair(amount)`, so walls/drill can reuse it later. Scrap is now "sell it or repair with it".
- Done: **step 24** carry weight: `PlayerCharacter.Load` (0–1) = (ore × `oreWeight` 1 + scrap × `scrapWeight` 0.5) / `maxLoad` 40, capped at 1; speed = `moveSpeed * Mathf.Lerp(1, heavySpeedFactor 0.7, Load)` — linear, max 30% slower (the user tried 50% and found it too harsh; a `Load * Load` curve was offered and declined for now). `LootChanged` sends the load and the HUD shows "Load: x%". Enemies were slowed a bit to match (enemy 95, skitter 120).
- **The side quest is done.**
- Possible polish for the turret prompt: only list the actions that are possible right now (`CanDoTurretAction` already knows); a health bar on deployables.
- **Next: step 25 deposits** (chosen over Phase 5 on 2026-10-30; design agreed):
  - A `Deposit` scene (`[Export] int tier` 1–3, group `deposits`). **Hand-placed** in `main.tscn` for now; random placement later.
  - The drill key (F, `deploy`) only works near a deposit; the drill **snaps onto its centre**. Otherwise "No deposit here".
  - Tiers scale **ore and swarm together** (roughly tier 1 ≈ 15 ore as now, tier 2 ≈ 30, tier 3 ≈ 50; faster spawns + bigger skitter packs). The drill knows its deposit; the spawner reads the tier via the WaveManager.
  - After a **successful drill the deposit is exhausted for good** (greyed out). If enemies **destroy the drill, the deposit stays** and you can try again.
  - Tier info: **a hint from afar** (size/colour) **and the exact tier up close** (a prompt like the turret's: "Tier 2 deposit · [F] Drill").
  - Steps: **25a** deposit scene + drill only on a deposit · **25b** tiers scale ore + swarm · **25c** exhausted state + prompt + tier look.
  - Done: **25a** `Deposit.cs` / `deposit.tscn` (the user's own `Ore_patch.png`), three hand-placed deposits (tiers 1–3) right below `Terrain` in `main.tscn`; `PlayerCharacter.FindClosestDeposit()` (`drillRange` 40) and `PlaceDrill` snaps onto the deposit.
  - Done: **25b** `Drill.Setup(deposit)` is called **before** `AddChild` (because `OnPlaced` starts the swarm and reads `Tier`); ore = `oreRate * Tier`; `WaveManager` calls `spawner.StartSpawning(currentDrill.Tier)`; `EnemySpawner` has `[Export] float[] spawnIntervalPerTier {2, 1.4, 1}` and `int[] extraPackSizePerTier {0, 3, 6}` (index `tier - 1`).
  - **Now: balancing the tiers.** Rough numbers per 30 s drill before tuning: tier 1 ≈ 46 enemies / 78 bullets, tier 2 ≈ 84 / 128, tier 3 ≈ 147 / 210 (more than max ammo 120 + 30 per turret); skitters are most of the swarm. Next: ask the user how each tier should feel, then tune.
  - Balancing so far (user's own tweaks, 2026-10-30): fewer/smaller packs (`skitterPackChance` 0.2, `extraPackSizePerTier` {0, 2, 4}), enemy speed 50 (skitters now inherit it), ore 15 / scrap 1 credits, turret 50 credits + `fireCooldown` 0.6, player ammo start 120 / max 240.
  - Done: **longer waves per tier**: `Drill` has `[Export] float[] drillDurationPerTier {45, 60, 75}` (read in `OnPlaced`, which works because `Setup` runs before `AddChild`); ore grows with time (`oreRate` 0.4 → ~18 / 48 / 90 ore).
  - Done: **small collision for the player only**: physics layer 4 `player_blockers`. Turret and drill have a child `PlayerBlocker` StaticBody2D (layer 4, no mask, small circle); walls are on layers 3 + 4; the player's mask is 2 + 4. Enemies, their bite hitboxes and bullets still use the big bodies on layer 3, so no Enemy code changed. **Any new deployable needs its own `PlayerBlocker`.**
  - Done: **step 26 ammo crate** (design agreed 2026-10-31): `Deployable.Kind.AmmoCrate`, `AmmoCrate.cs` / `ammo_crate.tscn` (the user's `AMMO_BOX.png`, max health 25, `PlayerBlocker`), key J (`deploy_crate`), inventory 1 / max 2, HUD `CratesLabel`, shop `BuyCrateButton` (120 credits). Every `feedInterval` (1 s) it gives `feedAmount` (2) rounds to each turret within `feedRange` (60) that uses its `AmmoType`, and `playerFeedAmount` (5) to the player standing in reach (automatic, no key — a hold-R version was deferred until more things are interactable; that needs the turret-only focus/timed-action system generalized to deployables). Enemies can destroy it; empty = gone (`UseAmmo` → `QueueFree`).
  - **Next: 25c** — walkthrough already given (2026-10-31), not started yet: `Deposit` gets `[Export] Color[] tierTints` (Modulate per tier — colour, not scale, for pixel art; per-tier textures later when the user draws variants), `exhaustedTint`, `IsExhausted`, `Exhaust()` (called from `Drill.FinishDrilling`, not from `OnDestroyed`), and a `PromptLabel` (copied from the turret) with text built in code (`"Tier " + tier + " deposit
[F] Drill"`); `FindClosestDeposit` skips exhausted deposits; `PlayerCharacter.UpdateFocusedDeposit()` mirrors `UpdateFocusedTurret` but shows nothing while a drill is running.
- Then: **Phase 5 enemy variety** — ideas discussed: Charger (wind-up straight-line charge, smashes walls; suggested first), Spitter (ranged lobbed globs, reuses `Bullet`), Burrower (underground, pops up at the drill/turrets), Drill-eater (ignores the player), Shell beetle (armoured front, flank it).
- Other directions the user can pick from later: atmosphere (radar, night, sandstorms, dust), fire on kill, bank/safe space/inventory, more guns (Medium/Heavy ammo), the user's own art for the next sprites + feet pivot + Y-sort, juice and sound, game structure (menu, pause, restart, saving), small polish (prompt shows only possible actions, deployable health bars, muzzle spawn, enemy homePosition (0,0) bug, debug prints)., then discuss juice/feel and the art decision.

## Backlog of the user's ideas

- **Deposits** for the drill, with **tiers that scale the swarm**; **manual mining** (less ore, for when broke, with its own challenge).
- **Atmosphere phase:** **radar/minimap** with pings (enemies, deposits, incoming swarms), **night** low visibility, **sandstorms**, **dust clouds** (from spawns, burrowers, grenades, missed shots, deaths, drills).
- **Fire on kill** that damages everyone (a `Hazard` team), maybe put out by sandstorms.
- A **bank** at the settlement, a **deployable safe space**, a **boss phase**, a fuller **inventory system**.
