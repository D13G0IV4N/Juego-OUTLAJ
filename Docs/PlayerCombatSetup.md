# Player Combat Setup (Round 1)

This setup adds player attacks without replacing existing movement (`PlayerMovement`) or life (`LifeManager`) systems.

## New scripts

- `Assets/Scripts/PlayerCombat.cs`
- `Assets/Scripts/PlayerAttackHitbox.cs`

## Required Animator parameters (Player Animator)

Add these **Trigger** parameters in `Player_Animator`:

- `Punch1`
- `Punch2`
- `Kick`
- `Uppercut`

## Expected attack state names / clip names

Create these states in the player Animator Base Layer:

- `Player_Punch1` -> clip `Player_Punch1.anim`
- `Player_Punch2` -> clip `Player_Punch2.anim`
- `Player_Kick` -> clip `Player_Kick.anim`
- `Player_Uppercut` -> clip `Player_Uppercut.anim`

Recommended transitions:

- `Any State -> Player_Punch1` when Trigger `Punch1`
- `Any State -> Player_Punch2` when Trigger `Punch2`
- `Any State -> Player_Kick` when Trigger `Kick`
- `Any State -> Player_Uppercut` when Trigger `Uppercut`

For each attack state, add transition back to locomotion (`Player`/`Player_Walk`/`Player_Run`) using **Has Exit Time**.

## Player child GameObjects / hitboxes

Under `Player`, create one child (or more if desired):

- `AttackHitbox_Main` (child of Player)
  - `BoxCollider2D` (set `Is Trigger = true`)
  - `PlayerAttackHitbox` component

Optional: create additional children such as `AttackHitbox_Upper` and add `PlayerAttackHitbox` to each one.

## Inspector references to assign manually

On `Player` object:

1. Add component: `PlayerCombat`.
2. Assign `Animator` (Player Animator component).
3. Assign `Player Movement` (existing `PlayerMovement` component).
4. Assign `Hitboxes` array with all `PlayerAttackHitbox` children.
5. Configure attack timings/damage in `PlayerCombat`:
   - Punch1 / Punch2 / Kick / Uppercut (`windUp`, `activeTime`, `recovery`, `cooldown`, `damage`).

On each attack hitbox child:

1. Assign `targetLayers` in `PlayerAttackHitbox` to enemy layers only.
2. Resize and place collider in front of player sprite reach.

## Input mapping implemented

- Left Click (`Mouse0`): alternates `Punch1`, `Punch2`.
- Right Click (`Mouse1`): `Kick`.
- `V` key: `Uppercut`.

## Anti-spam / stability

- `PlayerCombat` blocks new attacks while an attack is active.
- `cooldown` per attack limits rapid spam.
- Hitboxes are enabled only during attack active windows.
- Each hitbox can hit the same target only once per attack window.

## Notes on compatibility

- Existing `PlayerMovement` movement/jump/run flow remains unchanged.
- Existing `LifeManager` flow remains unchanged.
- Existing enemy scripts are not replaced.
- Current round only sends `OnPlayerAttackHit(int damage)` message to detected targets; enemy health/death is intentionally not implemented.
