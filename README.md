# UAA Spring 2026 Capstone Project – Construction Training Simulator

This project was created for our client Dr. Hejintao Huang at Cooper Union. It is an interactive Unity-based training tool for construction module sequencing and foundation parameter testing.

## 👥 Team & Acknowledgments

- **Developers**: Shayde DuBey & Kristian Gordon
- **Client**: Dr. Hejintao Huang, Cooper Union  
- **Advisor**: Dr. Pradeeban Kathiravelu  
- **Additional Support**: Kevin Sebastian & Christopher Lin  


## 📄 License

This project is released under the **MIT License** (see LICENSE file).

---

## 🛠️ Requirements

- **Unity Editor Version**: `6000.3.11f1` or higher  
- **Build Target**: PC, Mac, or Linux Standalone  

---

## 🚀 How to Run & Build

1. Open the project in Unity **6000.3.11f1+**.
2. Ensure there are **no prefab errors** in the console.
3. Assign a **Build Folder** via `File > Build Profiles`.
4. Click **Build and Run**.

---

## 🎮 Interaction Guide

### Module 1 – Hallway Button
- Go to the **end of the hallway**.
- Press the button **once for each step** of the building sequence to play.

### Module 2 – Central Testing Area
Located in the **center** of the scene.

| Screen Position | Behavior |
|----------------|----------|
| **Left screen** | Always fails |
| **Right screen** | Always succeeds |
| **Center screen** | Requires user to input **parameters for the shallow foundation** |

> ⚠️ The center screen will not succeed until valid shallow foundation parameters are provided.

---

## 🧪 Notes

- This is a simulation/demonstration tool, not a full game.
- Parameter validation for the shallow foundation is handled by the center screen logic.
- The left/right screens are fixed behavior examples (always fail / always succeed) for comparison.

---

## 🙏 Acknowledgements

Thanks to Dr. Pradeeban Kathiravelu for advising this capstone project, and to Kevin Sebastian & Christopher Lin for supplying formulas and context.
