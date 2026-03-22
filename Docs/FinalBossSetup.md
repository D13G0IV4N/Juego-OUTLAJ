# Final Boss Setup (X-Man)

This setup reuses the current project combat flow:

- `PlayerCombat` opens player hitboxes only during attack windows.
- `PlayerAttackHitbox` damages any target with `EnemyHealth`.
- `EnemyHealth` handles boss HP, hit reaction, death animation, and combat shutdown.
- `EnemyDamage` is reused for the boss's attack hurtbox so the player only takes damage during attack windows.
- `EnemyDefeatSceneTransition` is reused for the post-fight victory flow.
- `FinalBossController` adds the missing boss movement, spacing, activation, facing, and attack timing.

## Scripts involved

- Existing: `Assets/Scripts/EnemyHealth.cs`
- Existing: `Assets/Scripts/EnemyDamage.cs`
- Existing: `Assets/Scripts/EnemyDefeatSceneTransition.cs`
- Existing: `Assets/Scripts/PlayerCombat.cs`
- Existing: `Assets/Scripts/PlayerAttackHitbox.cs`
- New: `Assets/Scripts/FinalBossController.cs`

## Boss_XMan root object setup

On `Boss_XMan`, add / assign these components:

1. `Animator`
2. `Rigidbody2D`
   - Body Type: `Dynamic`
   - Gravity Scale: your current grounded value
   - Freeze Rotation Z: enabled
3. Main body collider (`BoxCollider2D` or `CapsuleCollider2D`)
   - `Is Trigger`: **off**
4. `EnemyHealth`
5. `FinalBossController`

### Recommended EnemyHealth values

- `Max Health`: set your boss HP here
- `Hit Trigger Name`: `Hit`
- `Hit State Name`: `BossXMan_Hit`
- `Death Trigger Name`: `Death`
- `Death State Name`: `BossXMan_Death`
- `Target Rigidbody`: Boss_XMan `Rigidbody2D`
- `Behaviours To Disable`: add `FinalBossController`
- `Colliders To Disable`: optional; leave empty if you want it to auto-disable all colliders on death

### Recommended FinalBossController values

- `Player`: your player transform
- `Animator`: Boss_XMan animator
- `Rb`: Boss_XMan rigidbody
- `Enemy Health`: Boss_XMan `EnemyHealth`
- `Attack Damage Windows`: add the attack hitbox child `EnemyDamage` component(s)
- `Left Limit`: scene transform marking the left edge
- `Right Limit`: scene transform marking the right edge
- `Move Speed`: tuning value
- `Activation Range`: tuning value
- `Follow Distance`: tuning value
- `Retreat Distance`: tuning value
- `Attack Distance`: tuning value
- `Attack Interval`: tuning value
- `Attack Wind Up`: tuning value
- `Attack Active Duration`: tuning value
- `Move Bool Name`: `IsMoving`
- `Attack Trigger Name`: `Attack`

## Required boss child objects

Create this child under `Boss_XMan`:

### `AttackHitbox`

Components:

1. `BoxCollider2D` (or another 2D trigger collider matching X-Man's attack reach)
   - `Is Trigger`: **on**
   - Position it in front of the boss torso/fists
2. `EnemyDamage`
   - `Damage Cooldown`: `0.75` to `1.0` recommended
   - `Attack Window Active On Start`: **off**

This hitbox is how the boss damages the player. It stays harmless until `FinalBossController` opens the attack window.

## Collider / trigger rules

### Boss root
- Main body collider: `Is Trigger = false`
- Used for physical presence and for the player's attack hitbox to find `EnemyHealth` via `GetComponentInParent<EnemyHealth>()`

### Boss attack child
- Attack collider: `Is Trigger = true`
- Used only with `EnemyDamage`
- This must **not** be your root physics collider

### Player attack child
- Keep using your current `PlayerAttackHitbox` setup
- Player attack hitboxes should remain trigger colliders

## Animator setup

Use your existing clips only:

- `BossXMan_Idle`
- `BossXMan_Attack`
- `BossXMan_Hit`
- `BossXMan_Death`

### Required Animator parameters

Create these parameters in the X-Man Animator Controller:

- `IsMoving` (`Bool`)
- `Attack` (`Trigger`)
- `Hit` (`Trigger`)
- `Death` (`Trigger`)

### Recommended states

- `BossXMan_Idle` using clip `BossXMan_Idle`
- `BossXMan_Move` using **the same clip as idle for now if you do not have a walk clip**
- `BossXMan_Attack` using clip `BossXMan_Attack`
- `BossXMan_Hit` using clip `BossXMan_Hit`
- `BossXMan_Death` using clip `BossXMan_Death`

If you do not already have a movement clip, reuse `BossXMan_Idle` inside the `BossXMan_Move` state. The controller still needs a separate move state so movement logic can switch animation cleanly without creating new clips.

### Transitions to create

1. `BossXMan_Idle -> BossXMan_Move`
   - Condition: `IsMoving == true`
   - Has Exit Time: off
2. `BossXMan_Move -> BossXMan_Idle`
   - Condition: `IsMoving == false`
   - Has Exit Time: off
3. `Any State -> BossXMan_Attack`
   - Condition: `Attack` trigger
   - Has Exit Time: off
4. `BossXMan_Attack -> BossXMan_Idle`
   - Has Exit Time: on
   - No condition needed
5. `Any State -> BossXMan_Hit`
   - Condition: `Hit` trigger
   - Has Exit Time: off
6. `BossXMan_Hit -> BossXMan_Idle`
   - Has Exit Time: on
   - No condition needed
7. `Any State -> BossXMan_Death`
   - Condition: `Death` trigger
   - Has Exit Time: off
8. `BossXMan_Death`
   - No transitions back out

## How damage connects to the current system

### Player damages boss
1. `PlayerCombat` opens player attack hitboxes during active frames.
2. `PlayerAttackHitbox` checks collided objects for `EnemyHealth`.
3. Boss_XMan has `EnemyHealth` on the root, so the boss takes damage automatically.
4. `EnemyHealth` plays `Hit` when damaged and `Death` when HP reaches zero.

### Boss damages player
1. `FinalBossController` decides when X-Man attacks.
2. During the attack window, it calls `EnemyDamage.BeginAttackWindow()` on the attack hitbox.
3. `EnemyDamage` only hurts the player while that window is active.
4. Outside that window, body contact does not damage the player.

## Victory / end of fight setup

Reuse `EnemyDefeatSceneTransition` in the `FinalBoss` scene.

### Create UI

1. Add a Canvas.
2. Add a panel (for example `VictoryPanel`) and disable it by default.
3. Add a `Text` or `TMP_Text` child with a message such as:
   - `X-Man defeated! Press Space to continue.`

### Add transition script

Create an empty GameObject such as `FinalBossVictoryFlow` and add `EnemyDefeatSceneTransition`.

Assign:

- `Target Enemy`: Boss_XMan `EnemyHealth`
- `Panel Root`: `VictoryPanel`
- `Message Text` or `Message Text TMP`: your label
- `Message`: your final text
- `Next Scene Name`: your credits / ending / menu scene name (optional)
- `Continue Key`: `Space`
- `Pause Time Scale`: on (recommended)
- `Player Movement`: player's `PlayerMovement`
- `Player Combat`: player's `PlayerCombat`

### Build Settings reminder

If `FinalBoss` or your next scene is not in Build Settings, add it in:

- `File -> Build Profiles` / `Build Settings`
- Add `FinalBoss`
- Add the scene you want to load after the win

## Suggested starting tuning

Try these initial values and adjust from there:

- `Max Health`: `18`
- `Move Speed`: `2.2`
- `Activation Range`: `10`
- `Follow Distance`: `4.5`
- `Retreat Distance`: `1.6`
- `Attack Distance`: `2.4`
- `Attack Interval`: `1.7`
- `Attack Wind Up`: `0.28`
- `Attack Active Duration`: `0.18`

## Scene checklist

- `Boss_XMan` tagged/layered however you prefer, but it must keep a non-trigger body collider
- Player object must keep the `Player` tag
- Boss root has `EnemyHealth`
- Boss root has `FinalBossController`
- Boss attack child has trigger collider + `EnemyDamage`
- Boss Animator has parameters: `IsMoving`, `Attack`, `Hit`, `Death`
- `EnemyHealth` names match those Animator parameters / states
- Left and right scene limits are assigned
- Victory flow references Boss_XMan `EnemyHealth`
