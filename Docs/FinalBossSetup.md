# Final Boss Setup (X-Man)

This document explains how to connect the new final boss system to the existing project structure in `Assets/Scripts`.

## Reused systems

The final boss setup intentionally reuses the current combat flow:

- `EnemyHealth` receives player hitbox damage from `PlayerAttackHitbox`.
- `EnemyDamage` only hurts the player during boss attack windows.
- `PlayerCombat` and `PlayerAttackHitbox` remain the player-side attack system.
- `EnemyDefeatSceneTransition` can finish the fight cleanly after the boss dies.
- `FinalBossController` is the new boss brain that handles activation, spacing, facing, horizontal movement, and attack timing.

## Required components on `Boss_XMan`

Add or verify these components on the root `Boss_XMan` object:

1. `SpriteRenderer`
2. `Animator`
3. `Rigidbody2D`
   - Body Type: `Dynamic`
   - Gravity Scale: `3` to `5` (or your scene default)
   - Freeze Rotation Z: enabled
4. A non-trigger body collider such as `BoxCollider2D` or `CapsuleCollider2D`
5. `EnemyHealth`
6. `FinalBossController`
7. `EnemyDefeatSceneTransition` (optional but recommended for clean victory flow)

## Boss child object needed

Create one child under `Boss_XMan`:

- `AttackHitbox`
  - Add `BoxCollider2D`
  - Set `Is Trigger = true`
  - Resize it to the boss attack reach in front of X-Man
  - Add `EnemyDamage`

You do **not** need a separate `EnemyDetection` object for this boss, because `FinalBossController` handles the boss activation range and movement logic directly while still reusing `EnemyDamage` and `EnemyHealth`.

## Collider rules

- Boss body collider: **not** trigger
- Boss attack hitbox collider: **trigger**
- Player attack hitbox collider: **trigger** (already used by `PlayerAttackHitbox`)
- Floor and scene limit colliders: **not** trigger

## Inspector setup for `EnemyHealth`

On `Boss_XMan -> EnemyHealth` use:

- `Max Health`: your desired boss HP
- `Hit Trigger Name`: `Hit`
- `Hit State Name`: `BossXMan_Hit`
- `Death Trigger Name`: `Die`
- `Death State Name`: `BossXMan_Death`
- `Disable Colliders Delay`: `0.05`
- `Target Rigidbody`: the boss `Rigidbody2D`
- `Colliders To Disable`: leave empty if you want all colliders disabled automatically on death, or assign the boss body collider and attack hitbox collider explicitly
- `Behaviours To Disable`: add `FinalBossController`

`EnemyHealth` will already stop offensive logic by disabling `EnemyDamage` children when the boss dies.

## Inspector setup for `FinalBossController`

Assign these references/values on `Boss_XMan -> FinalBossController`:

- `Player Target`: drag the scene `Player`
- `Move Speed`: boss horizontal speed
- `Activation Range`: distance at which the boss wakes up
- `Follow Distance`: if the player is farther than this, the boss moves forward
- `Retreat Distance`: if the player is closer than this, the boss steps back
- `Attack Distance`: if the player is inside this range, the boss attacks
- `Attack Interval`: full time between the start of one attack and the start of the next
- `Attack Wind Up`: startup before damage becomes active
- `Attack Active Duration`: how long the damage window stays active
- `Min X Limit`: world-space left movement clamp
- `Max X Limit`: world-space right movement clamp
- `Enemy Damage Windows`: assign the `AttackHitbox` child `EnemyDamage`
- `Idle State Name`: `BossXMan_Idle`
- `Attack Trigger Name`: `Attack`

### Recommended starting values

These are safe starting values you can tune in the Inspector:

- `Move Speed`: `2.25`
- `Activation Range`: `12`
- `Follow Distance`: `5`
- `Retreat Distance`: `1.75`
- `Attack Distance`: `3`
- `Attack Interval`: `1.6`
- `Attack Wind Up`: `0.2`
- `Attack Active Duration`: `0.2`
- `Max Health`: `12`

## Inspector setup for `EnemyDamage`

On `Boss_XMan/AttackHitbox -> EnemyDamage`:

- `Damage Cooldown`: `1`
- `Attack Window Active On Start`: disabled

The boss only damages the player when `FinalBossController` opens an attack window.

## How the boss takes damage from the player

The existing player flow remains the same:

1. `PlayerCombat` starts an attack.
2. `PlayerAttackHitbox` enables the player attack collider.
3. If that hitbox overlaps `Boss_XMan` or any child collider on the boss hierarchy, it looks for `EnemyHealth` in the parent chain.
4. `EnemyHealth.OnPlayerAttackHit(int damage)` reduces boss HP.
5. Boss hit animation plays using `BossXMan_Hit`.
6. On zero HP, `BossXMan_Death` plays and the boss shuts down.

## Animator setup for `Boss_XMan.controller`

Use the existing clips only:

- `BossXMan_Idle`
- `BossXMan_Attack`
- `BossXMan_Hit`
- `BossXMan_Death`

### Required Animator parameters

Create these **Trigger** parameters:

- `Attack`
- `Hit`
- `Die`

### Required states

Create or verify these states in the Base Layer:

- `BossXMan_Idle` using clip `BossXMan_Idle`
- `BossXMan_Attack` using clip `BossXMan_Attack`
- `BossXMan_Hit` using clip `BossXMan_Hit`
- `BossXMan_Death` using clip `BossXMan_Death`

### Required transitions

1. `BossXMan_Idle -> BossXMan_Attack`
   - Condition: `Attack` trigger
   - `Has Exit Time`: off
2. `BossXMan_Attack -> BossXMan_Idle`
   - `Has Exit Time`: on
3. `BossXMan_Idle -> BossXMan_Hit`
   - Condition: `Hit` trigger
   - `Has Exit Time`: off
4. `BossXMan_Hit -> BossXMan_Idle`
   - `Has Exit Time`: on
5. `Any State -> BossXMan_Death`
   - Condition: `Die` trigger
   - `Has Exit Time`: off
   - No transitions out of death

The repository already contains a `Boss_XMan.controller` with these clips/states/triggers, so mostly verify it matches the list above.

## Respecting floor and scene limits

- Keep the boss root on the floor using the root `Rigidbody2D` + non-trigger body collider.
- Set `Min X Limit` and `Max X Limit` in `FinalBossController` to the same world X positions as your scene limit colliders.
- The boss movement is horizontal only because `FinalBossController` only changes X velocity.

## Clean victory flow after death

The easiest clean ending is to reuse `EnemyDefeatSceneTransition`.

### Add a simple victory UI

1. Create a `Canvas` in `FinalBoss`.
2. Add a panel named `VictoryPanel`.
3. Set the panel inactive by default.
4. Add a `Text` or `TMP_Text` child with a message like:
   - `X-Man defeated! Press Space to continue.`

### Configure `EnemyDefeatSceneTransition`

Add `EnemyDefeatSceneTransition` to `Boss_XMan` or a scene manager object and assign:

- `Target Enemy`: `Boss_XMan` `EnemyHealth`
- `Panel Root`: `VictoryPanel`
- `Message Text` or `Message Text TMP`: your text component
- `Message`: your custom victory message
- `Next Scene Name`: optional, for example `Menu` or another ending scene
- `Continue Key`: `Space`
- `Pause Time Scale`: enabled if you want the game to freeze on victory
- `Player Movement`: scene `PlayerMovement`
- `Player Combat`: scene `PlayerCombat`

If `Next Scene Name` is empty, the script loads the next scene in Build Settings.

## Final connection checklist

- `Boss_XMan` has `Animator`, `Rigidbody2D`, body collider, `EnemyHealth`, and `FinalBossController`
- `Boss_XMan` animator uses `Assets/Animations/Boss/Boss_XMan.controller`
- `AttackHitbox` child exists with trigger collider + `EnemyDamage`
- `FinalBossController` references the player and attack hitbox
- `EnemyHealth` uses `Hit` and `Die` triggers with `BossXMan_Hit` and `BossXMan_Death`
- Player hitboxes target the boss layer/body collider
- `EnemyDefeatSceneTransition` is assigned if you want the fight to end with UI or a scene change
