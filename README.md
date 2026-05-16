# 1000X

A minimalist Bullet Hell shooter built with Unity, focusing on high-performance systems and modular architecture

---

## Game Information

### Controls & Navigation
- **Movement**: WASD / Arrow Keys
- **Shooting**: Space / Left Click (Hold)
- **Pause**: Escape
- **UI Navigation**: Use the Mouse to interact with menu buttons

### Win & Lose Conditions
- **Win Condition**: Survive until the Boss spawns (at the 5-minute mark) and successfully defeat it
- **Lose Condition**: Player's HP reaches 0

---

## Features

### Implemented Features
- [x] **Object Pooling System**: Efficient memory management for hundreds of active projectiles and enemies
- [x] **Data-Driven Design**: Enemy and weapon statistics are managed via ScriptableObjects for easy balancing without code changes
- [x] **Dynamic Scaling**: Difficulty and enemy spawning intensity increase over time using a weighted random system
- [x] **Weapon Overheat Mechanic**: Adds a layer of tactical play to discourage mindless shooting
- [x] **Multi-Phase Boss Battle**: Boss behavior and visuals evolve based on health thresholds
- [x] **Local Leaderboard**: Persistent high-score tracking using PlayerPrefs

### Unfinished Features & Reasoning
- **Power-up System**: Originally planned but postponed due to time constraints. Priority was given to polishing the core gameplay loop and ensuring system stability.

---

## Reflection

### Known Bugs & Limitations
- **Device Compatibility**: The game has currently only been tested on the primary development environment. Performance and UI scaling on varying hardware specifications or aspect ratios are yet to be fully verified.

### Future Improvements
- **Buff & Power-up System**: Implementing a variety of collectible buffs to increase gameplay depth.
- **Enemy Variety**: Adding more complex enemy types with unique movement and fire patterns to enhance the challenge.

### Biggest Challenge
The most significant challenge during development was **refactoring and cleaning up the code architecture**. Ensuring that the Singleton managers, event systems, and inheritance patterns (e.g., `EnemyBase`) remained clean and extensible required constant iteration to avoid technical debt while maintaining a high-performance codebase.

---
*Technical exploration project for Bullet Hell development in Unity.*
