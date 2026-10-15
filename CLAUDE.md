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
- **No full-code handholding** unless they explicitly ask, or the topic is extremely hard and completely new. A *little* more concrete help works well: exact corrected syntax structures for syntax errors, and short explained snippets for brand-new APIs.
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

**Core loop:** roam (wandering enemies, scrap) → place a **drill** → the drill triggers a **swarm** → defend with turrets, walls and your gun → collect ore + scrap → sell at the **settlement** (safe zone + merchant) for credits → buy turrets, walls, ammo → drill again. You lose carried (unsold) loot on death.

**Main systems:** `WaveManager` (Calm/Swarm, started by the drill), `EnemySpawner` (single enemies + skitter packs), `Enemy` (Wander/Chase/Attack state machine, knockback/stun, separation, targets the closest valid target), `Deployable` base class (`Drill`, `Turret`, `Wall`) implementing `IDamageable`, `Pickup` (ore/scrap), `Settlement` (shop), `Hud`, `Main` (wires signals together). Physics layers: 1 player, 2 enemies, 3 deployables.

## Where we are (update this as steps finish)

Phases 1–4 are done (movement/combat basics, waves + HUD, deployables, economy loop + gun). Now on the **side quest**:

- Done: 18 enemy attack wind-up + knockback/stun · 19 skitter swarm enemy · 19b movement/collision polish (floating motion, circles, layers, separation) · 20 friendly fire + hit flash · 21 walls + generalized inventory (`Dictionary<Deployable.Kind, int>`), `PlaceDeployable(kind)`, generic `BuyItem` · a code cleanup pass (scripts organized into settings / state / read-only access / grouped methods; typed `EmitSignal...` helpers everywhere).
- **Next: step 22** pick up turrets (they keep their health/ammo).
- Then: 23 repair with scrap + **turret ammo/resupply** (replaces the turret lifetime timer — open questions: shared vs separate ammo, how to resupply) · 24 carry weight (ore slows you).
- After the side quest: **Phase 5 enemy variety**, then discuss juice/feel and the art decision.

## Backlog of the user's ideas

- **Deposits** for the drill, with **tiers that scale the swarm**; **manual mining** (less ore, for when broke, with its own challenge).
- **Atmosphere phase:** **radar/minimap** with pings (enemies, deposits, incoming swarms), **night** low visibility, **sandstorms**, **dust clouds** (from spawns, burrowers, grenades, missed shots, deaths, drills).
- **Fire on kill** that damages everyone (a `Hazard` team), maybe put out by sandstorms.
- A **bank** at the settlement, a **deployable safe space**, a **boss phase**, a fuller **inventory system**.
