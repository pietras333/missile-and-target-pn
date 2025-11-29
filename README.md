# 🚀 Proportional Navigation Missile Simulation

A data-driven Unity simulation demonstrating the **Proportional Navigation (PN)** guidance algorithm - the most widely used missile guidance law in real-world aerospace systems.

## 🎯 Scientific Background

### What is Proportional Navigation?

PN is a **pursuit guidance algorithm** where the missile's commanded acceleration is proportional to the line-of-sight (LOS) rate between missile and target.

**Core Equation:**
```
a_lateral = N × V_c × λ̇
```

Where:
- `a_lateral` = Lateral acceleration command 🔄
- `N` = Navigation gain (typically 3-5) ⚙️
- `V_c` = Closing velocity 💨
- `λ̇` = Line-of-sight angular rate 👁️

### 📐 Key Principles

**Line of Sight (LOS)** 📏
- Vector from missile to target
- Changes as both entities move
- Rate of change drives guidance commands

**LOS Rate (λ̇)** 🌀
- Angular velocity of the LOS vector
- Calculated as: `(LOS_current - LOS_previous) / dt`
- Zero LOS rate = collision course achieved ✅

**Closing Velocity (V_c)** ⚡
- Relative velocity component along LOS
- `V_c = (V_missile - V_target) · LOS`
- Determines engagement urgency

**Navigation Gain (N)** 🎚️
- Multiplier for commanded acceleration
- N=3: Proportional Navigation
- N>3: Augmented Proportional Navigation (more aggressive)
- Higher N = tighter turns, faster intercept

## 🔬 Implementation Details

### Algorithm Flow
1. **Measure LOS** → Calculate vector to target 📍
2. **Compute LOS Rate** → Differentiate LOS over time 📊
3. **Calculate Closing Speed** → Project relative velocity ➡️
4. **Apply PN Law** → Generate acceleration command 🎯
5. **Update Velocity** → Integrate acceleration 🔄
6. **Maintain Speed** → Normalize to constant magnitude 🏃

### Why PN Works

✨ **Optimal Geometry**: Maintains collision triangle geometry  
⚡ **Energy Efficient**: Minimizes required acceleration  
🎯 **Predictable**: Works against constant-velocity targets  
🔄 **Adaptive**: Compensates for target maneuvers  
💻 **Computationally Simple**: Real-time capable  

### Real-World Applications

🚀 **Air-to-Air Missiles** - AIM-9 Sidewinder, AIM-120 AMRAAM  
🛡️ **Surface-to-Air Systems** - Patriot, S-400  
🌊 **Anti-Ship Missiles** - Harpoon, Exocet  
🎮 **Game AI** - Homing projectiles, smart enemies  

## 📊 Simulation Features

### Data-Driven Architecture
- ✅ Zero GameObject spawning at runtime
- ✅ Fully serializable classes
- ✅ Inspector-editable parameters
- ✅ Real-time visualization

### Target Movement Patterns
🔵 **Circle** - Constant angular velocity orbit  
∞ **Figure-8** - Lissajous curve pattern  
➡️ **Straight** - Constant velocity trajectory  
⏸️ **Stationary** - Fixed position  

### Path Visualization
📈 **LineRenderer Trails** - Complete trajectory history  
🎨 **Color-Coded** - Missiles (red) vs Targets (green)  
📉 **Gradient Fading** - Visual depth perception  

### Performance Metrics
📊 Track active missiles/targets  
💥 Count successful intercepts  
⏱️ Adjustable time scale  
🔄 Automatic pair spawning on hit  

## 🎮 Usage

1. Attach `MissileSimulatorManager` to GameObject
2. Configure missile parameters (speed, navigation gain)
3. Set target movement patterns
4. Press Play ▶️
5. Watch PN algorithm in action! 🎯

## 📚 Scientific Accuracy

This simulation implements **Pure Proportional Navigation** with:
- ✅ Correct LOS rate calculation
- ✅ Proper velocity vector mechanics
- ✅ Realistic closing velocity computation
- ✅ Constant speed constraint
- ✅ Physical acceleration integration

Perfect for education, research, or game development! 🎓🔬🎮

---

**Note**: This is a simplified 3D implementation. Real-world systems include additional factors like drag, gravity, thrust limits, and sensor noise.
