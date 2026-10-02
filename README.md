#  Swap It! - Dark Fantasy Match-3

##  Gameplay Showcase
[![Swap It! Gameplay](https://img.youtube.com/vi/kLHpYv9GrBA/0.jpg)](https://www.youtube.com/shorts/kLHpYv9GrBA)
*(Click the image above to watch the gameplay short!)*

A gothic-themed Match-3 puzzle game built in Unity. Diverging from the colorful and bright aesthetic of traditional puzzle games, this project delivers a dark, atmospheric experience where players shatter ancient runes and break through stone obstacles to protect the realm from a dragon's wrath. 

The codebase is built on AAA-standard architecture, heavily utilizing Data-Driven Design (ScriptableObjects), the Observer pattern for state management, and procedural animations for high-quality game "juice".

##  Features

*   **Data-Driven Level Design:** Complete separation of logic and design. Level dimensions, goals, move limits, and allowed tile colors are strictly managed via `LevelData` ScriptableObjects.
*   **Seamless Level Progression:** Automatic transition between levels using dynamic indexing and `PlayerPrefs` serialization, keeping players in the action without returning to the main menu.
*   **Robust State Machine:** A decoupled `GameManager` handles the game loop (`MENU`, `GAME`, `GAMEOVER`, `LEVELCOMPLETE`) through the `IGameStateListener` interface, ensuring zero race conditions.
*   **Advanced Turn Resolution:** The `GoalManager` acts as the definitive referee, ensuring all cascades, gravity drops, and combo calculations finish completely before evaluating win/lose conditions.
*   **Polished "Juice" & Feedback:** Procedural UI and board animations (punching, scaling, 360-rotations) alongside a dynamic `SoundManager` that increases audio pitch upon consecutive combos.

##  How to Play

*   Swap adjacent runes to match 3 or more of the same color.
*   Clear the required amount of specific runes before running out of moves.
*   Shatter adjacent stone obstacles to expand the playable board.
*   String together matches to trigger combos and hear the dark resonance build up!

## Credits & Assets

This project was brought to life using the following tools and audio assets. Huge thanks to the original creators for providing these resources:

### Audio (Pixabay)
*   **UI Click:** [Film Special Effects Inventory Open/Close](https://pixabay.com/sound-effects/film-special-effects-inventory-open-close-3-540173/)
*   **Rune Swap:** [Film Special Effects Stone Slide](https://pixabay.com/sound-effects/film-special-effects-stone-slide-sound-effects-322794/)
*   **Rune Shatter (Match):** [Film Special Effects Glass Shatter](https://pixabay.com/sound-effects/film-special-effects-glass-shatter-7-95202/)
*   **Victory Choir:** [Musical Angel Choir](https://pixabay.com/sound-effects/musical-angel-choir-463220/)
*   **Defeat Hit:** [Film Special Effects Cinematic Hit](https://pixabay.com/sound-effects/film-special-effects-cinematic-hit-159487/)

### Art & UI
*   **Interface Assets:** [Hyper Casual UI Pack](https://assetstore.unity.com/packages/2d/gui/hyper-casual-ui-pack-375832)

### Tools & Plugins
*   **Animation Engine:** [LeanTween](https://assetstore.unity.com/packages/tools/animation/leantween-3595)

---
*Developed by [Pianabol](https://github.com/Pianabol)*