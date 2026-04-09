# 塔防冲锋原型 - Unity 设置指南

## 项目设置

### 1. 创建新项目
- 打开 Unity Hub
- 选择 "New Project"
- 模板：2D (URP)
- Unity 版本：2022.3.53f1c1 LTS
- 项目名称：`tower-defense-rush-prototype`

### 2. 导入脚本
将 `Scripts/` 文件夹中的所有 C# 脚本复制到项目的 `Assets/Scripts/` 目录。

### 3. 场景设置

#### 创建基本场景结构：
```
Scene Hierarchy:
├── Main Camera (2D)
├── Directional Light
├── Canvas (Screen Space - Overlay)
│   ├── JoystickBackground (Image - 圆形)
│   │   └── JoystickHandle (Image - 小圆)
│   ├── WaveText (Text)
│   ├── EnemyCountText (Text)
│   └── KillCountText (Text)
├── GameManager (空物体)
│   └── WaveManager (脚本)
├── SpawnPoints (空物体)
│   ├── SpawnPoint1 (Transform)
│   ├── SpawnPoint2 (Transform)
│   ├── SpawnPoint3 (Transform)
│   └── SpawnPoint4 (Transform)
├── TownCenter (Sprite - 表示城镇)
└── Players
    ├── Lord (PlayerController 脚本)
    │   ├── Visual (子物体 - Sprite)
    │   └── AttackPoint (子物体 - Transform)
    ├── Jimmy1 (Jimmy 脚本 - FlameFox)
    │   └── Visual (子物体 - Sprite)
    ├── Jimmy2 (Jimmy 脚本 - BoarKing)
    │   └── Visual (子物体 - Sprite)
    └── Jimmy3 (Jimmy 脚本 - RockGolem)
        └── Visual (子物体 - Sprite)
```

### 4. 层级设置 (Layers)
- `Player` - 玩家和吉米
- `Enemy` - 敌人
- `Town` - 城镇中心

### 5. 预制体创建

#### 敌人预制体 (WildBoar)
```
GameObject: WildBoar
├── SpriteRenderer (颜色：棕色)
├── Rigidbody2D (Dynamic, Freeze Rotation)
├── CircleCollider2D
├── Enemy (脚本)
│   └── enemyType: WildBoar
└── HealthBar (子物体)
    └── SpriteRenderer (缩放表示血量)
```

#### 领主预制体 (Lord)
```
GameObject: Lord
├── SpriteRenderer (颜色：蓝色)
├── Rigidbody2D (Dynamic)
├── CircleCollider2D
├── PlayerController (脚本)
│   └── joystick: 拖拽Canvas中的Joystick
└── Visual (子物体)
```

#### 吉米预制体 (Jimmy)
```
GameObject: Jimmy
├── SpriteRenderer (根据类型不同颜色)
├── Rigidbody2D (Dynamic)
├── CircleCollider2D
└── Jimmy (脚本)
    ├── jimmyType: FlameFox/BoarKing/RockGolem
    └── lord: 拖拽Lord物体
```

### 6. WaveManager 配置
```
Wave Data (5波):
[0] Wave 1: enemyCount=5, spawnInterval=2, enemyType=WildBoar
[1] Wave 2: enemyCount=8, spawnInterval=1.5, enemyType=WildBoar
[2] Wave 3: enemyCount=5, spawnInterval=2, enemyType=BoarWarrior
[3] Wave 4: enemyCount=10, spawnInterval=1, enemyType=WildBoar
[4] Wave 5: enemyCount=1, spawnInterval=1, enemyType=BoarBoss
```

### 7. 快速测试键位
- **WASD / 方向键**：移动领主
- **自动攻击**：领主和吉米自动攻击范围内敌人

### 8. 运行测试
1. 点击 Play
2. 使用 WASD 移动领主
3. 观察吉米跟随和自动攻击
4. 测试5波敌人是否能正常生成和战斗

---

## 原型目标

**验证核心问题**：摇杆操控领主 + 吉米自动跟随协同战斗是否有趣？

**成功标准**：
- [ ] 3分钟内能感受到操作反馈
- [ ] 吉米跟随不卡顿
- [ ] 战斗有爽快感（敌人死亡反馈明确）
- [ ] 想要继续玩下去

---

## 已知限制

- 敌人AI简单（只会直线冲向目标）
- 没有粒子特效
- 没有音效
- 美术使用纯色方块
- 没有升级/选择门系统

这些都是刻意简化的，用于验证核心玩法循环。
