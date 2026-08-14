# Void Survivor — Game Design

## 1. High-Level
A small 2D top-down arena roguelite. The player survives 10 increasingly difficult waves, automatically attacks enemies, gains XP and Gold, selects upgrades, buys weapons/stat bonuses, and defeats a final boss.

## 2. MVP Content
- 1 player character
- 1 arena
- 4 normal enemies
- 1 boss
- 4 weapons
- 4 weapon slots
- 10 upgrade types
- 10 waves
- 1 game mode
- 5–10 minutes per run

## 3. Core Loop
Main Menu -> Battle -> XP/Gold -> Level Up -> Wave Complete -> Shop -> Next Wave -> Boss -> Victory/Defeat -> Restart

## 4. Weapons
- Pulse Gun: single-target ranged, high rate of fire
- Scatter Blaster: multi-projectile spread
- Boomerang: outward + return attack
- Arc Blade: close-range area attack

## 5. Enemies
- Chaser
- Runner
- Shooter
- Tank
- Final Boss

## 6. Player Stats
- MaxHP
- HPRegen
- MoveSpeed
- Damage
- AttackSpeed
- CritChance
- CritDamage
- Range
- PickupRange
- Armor

## 7. Level Up
On level up, pause gameplay and offer 3 random upgrade options; choose 1.

## 8. Shop
At the end of each non-boss wave, offer about 4 randomized products: weapons, weapon upgrades and stat bonuses. Optional refresh costs Gold.

## 9. Wave Structure
10 total waves. Wave duration and enemy difficulty increase over time. Wave 10 contains the boss encounter.

## 10. Win / Lose
Win by defeating the boss. Lose when player HP reaches 0. Both end in result screens with restart/main menu options.

## 11. Presentation
Use simple original 2D geometric/pixel-inspired visuals, concise SFX, and a minimal but complete UI. Avoid large external asset dependencies.

## 12. Out of Scope
Multiplayer, procedural dungeons, multiple maps, accounts, online services, complex equipment rarity, skill trees, achievements, mobile-specific work, Steam integration, and other non-MVP systems.
