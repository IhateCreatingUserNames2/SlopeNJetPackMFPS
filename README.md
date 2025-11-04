🎿 Parameter Guide - Skiing Tribes System

# Install: 
- Download SloopMovement.cs, SloopJetPack.cs , bl_FirstPersonController.cs 
- Replace Original bl_FirstPersonController.cs 
- Put Sloop scripts into a Folder named 'Sloop' inside Assets/ 
- Add Sloop Scripts to your Character Game Object 



https://github.com/user-attachments/assets/17d7ac64-a1da-4d0e-8982-5b01bf9a9756



📊 Core Parameters

<img width="424" height="572" alt="image" src="https://github.com/user-attachments/assets/1dbc6f0f-8747-4c3a-8482-67650401421e" />

🏃 Base Speeds
| Parameter | Recommended Value | Description |
| :--- | :--- | :--- |
| **minSpeedToSki** | 8-12 | Minimum speed to activate skiing. Below this = normal movement |
| **maxSpeed** | 40-60 (0 = no limit) | Absolute maximum speed. Prevents physics bugs |

⛰️ Ramp Physics
| Parameter | Recommended Value | Description |
| :--- | :--- | :--- |
| **gravity** | 20-30 | Force applied on ramps. Higher = accelerates faster downhill |
| **uphillMomentumRetention** | 0.80-0.95 | How much momentum is kept when going uphill. 0.85 = loses 15% |

🎮 Control (WASD)
| Parameter | Recommended Value | Description |
| :--- | :--- | :--- |
| **groundControl** | 20-30 | WASD control during normal movement |
| **skiControl** | 10-20 | WASD control during skiing. 10 = drift, 20 = full control |
| **airControl** | 0.5-1.5 | WASD control in the air. Multiplier of groundControl |

🛑 Friction
| Parameter | Recommended Value | Description |
| :--- | :--- | :--- |
| **groundFriction** | 2-5 | Friction on flat ground. Higher = stops faster |
| **skiFriction** | 0.05-0.2 | Friction during skiing. Lower = maintains momentum for longer |
| **groundStickForce** | 10-20 | Force to stick to the ground. Prevents "bouncing" on slopes |

---
🎯 Settings by Character Type
🪶 Light Character (Scout)
* **minSpeedToSki**: 7
* **gravity**: 25
* **uphillMomentumRetention**: 0.90 (keeps 90% of momentum)
* **skiControl**: 18
* **skiFriction**: 0.08
* **Result**: Accelerates fast, maintains speed uphill, agile control

⚖️ Medium Character (Soldier)
* **minSpeedToSki**: 9
* **gravity**: 22
* **uphillMomentumRetention**: 0.85 (keeps 85% of momentum)
* **skiControl**: 15
* **skiFriction**: 0.12
* **Result**: Balanced, classic Tribes experience

🛡️ Heavy Character (Juggernaut)
* **minSpeedToSki**: 12
* **gravity**: 28
* **uphillMomentumRetention**: 0.75 (keeps only 75% of momentum)
* **skiControl**: 12
* **skiFriction**: 0.15
* **Result**: Hard to stop, loses a lot of momentum uphill, heavy control

---
🐛 Common Troubleshooting
⚡ **"Character gains TOO much speed when jumping"**
* **Cause**: Jump was adding horizontal velocity
* **Solution**: Fixed! Jump now only adds vertical velocity
* **Check**: `maxSpeed` is configured (e.g.: 50)

🐌 **"Character turns VERY SLOWLY during skiing"**
* **Increase**: `skiControl` (e.g.: from 10 to 18)
* **Increase**: `airControl` (e.g.: from 0.8 to 1.2)

🏔️ **"Character doesn't go up ramps, only down"**
* **Increase**: `uphillMomentumRetention` (e.g.: from 0.75 to 0.90)
* **Reduce**: `gravity` (e.g.: from 30 to 22)
* **Increase**: `skiControl` (to have more control going up)

🎿 **"Skiing activates ALWAYS, even on flat ground"**
* **Increase**: `minSpeedToSki` (e.g.: from 8 to 12)
* **Check**: Terrain has angle > 5° (it might not be flat)

🛑 **"Character never stops"**
* **Increase**: `skiFriction` (e.g.: from 0.1 to 0.3)
* **Increase**: `groundFriction` (e.g.: from 2 to 5)

🦘 **"Character 'bounces' on slopes"**
* **Increase**: `groundStickForce` (e.g.: from 15 to 25)
* **Check**: `CharacterController.stepOffset` is correct

---
🎮 FINAL Recommended Values
To start, use these values (balanced experience):

* **minSpeedToSki**: 9
* **maxSpeed**: 50
* **gravity**: 22
* **groundFriction**: 3
* **skiFriction**: 0.12
* **groundControl**: 25
* **skiControl**: 15
* **airControl**: 1.0
* **groundStickForce**: 15
* **uphillMomentumRetention**: 0.85

Adjust to your preference! 🎿✨
