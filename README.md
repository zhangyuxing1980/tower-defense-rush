# Tower Defense Rush - Prototype

## 项目状态

### ✅ 已完成
- **代码编译**: 所有C#脚本编译成功
- **核心系统**:
  - PlayerController.cs - 领主摇杆移动+自动攻击
  - Jimmy.cs - 3种吉米AI（焰尾狐、野猪王、岩石巨像）自动跟随+攻击
  - Enemy.cs - 敌人AI寻路+攻击
  - WaveManager.cs - 波次管理系统
  - GameManager.cs - 游戏状态管理
  - SynergySystem.cs - 协同攻击系统
  - CameraShake.cs - 摄像机震动
  - ChoiceGateManager.cs - 选择门系统
  - Joystick.cs - 摇杆控制

- **MCP测试框架**:
  - TestCaseBase.cs - 测试基类
  - TestRunner.cs - 自动测试运行器
  - PerformanceTest.cs - 性能测试
  - CombatSystemTest.cs - 战斗系统测试
  - JimmyAITest.cs - 吉米AI测试
  - WaveSystemTest.cs - 波次系统测试
  - SynergySystemTest.cs - 协同攻击测试
  - ChoiceGateTest.cs - 选择门测试
  - ITestCase.cs - 测试接口

- **编辑器工具**:
  - PrototypeSceneSetup.cs - 一键设置场景

### ⚠️ 需要注意

**场景是空的**: 当前 `PrototypeScene.unity` 只有 Main Camera 和 Global Light 2D。

**需要在Unity中设置场景**:

#### 方法1: 使用编辑器脚本（推荐）

**步骤1: 创建基础场景**
1. 打开Unity项目 `prototypes/tower-defense-rush`
2. 在菜单栏点击 `Tower Defense Rush -> Setup Prototype Scene`
3. 点击 `Setup Complete Scene` 按钮
4. 场景将自动创建:
   - Player (领主)
   - 3个Jimmy (焰尾狐、野猪王、岩石巨像)
   - WaveManager
   - TestRunner
   - TownCenter
   - 4个敌人生成点
   - Enemy Prefab

**步骤2: 转换为2D场景（重要！）**
1. 在菜单栏点击 `Tower Defense Rush -> Setup 2D Scene`
2. 点击 `Convert to 2D` 按钮
3. 这将:
   - 设置相机为正交投影(Orthographic)
   - 为所有对象添加2D精灵(Sprite)
   - 设置正确的颜色和大小
   - 添加可视化测试运行器(VisualTestRunner)

#### 方法2: 手动设置

1. 创建 `Player` GameObject:
   - 添加 Rigidbody2D (gravity=0, freeze rotation)
   - 添加 PlayerController 脚本
   - 创建子对象 Visual (SpriteRenderer, 蓝色方块)

2. 创建 3个 `Jimmy` GameObjects:
   - 添加 Rigidbody2D
   - 添加 Jimmy 脚本，设置类型和lord引用
   - 创建子对象 Visual (SpriteRenderer, 不同颜色)

3. 创建 `WaveManager`:
   - 添加 WaveManager 脚本
   - 设置townCenter和spawnPoints
   - 配置3波敌人

4. 创建 `TestRunner`:
   - 添加 TestRunner 脚本
   - 添加各种测试组件

5. 创建 `TownCenter` (绿色方块) 作为敌人目标

### 🎮 操作说明

- **移动**: WASD 或 摇杆
- **暂停**: ESC
- **重新开始**: R (游戏结束或胜利后)

### 📊 测试报告

测试完成后，报告将保存到:
```
C:\Users\[Username]\AppData\LocalLow\DefaultCompany\tower-defense-rush\test_report.json
```

### 🔧 技术细节

- **Unity版本**: 2022.3.53f1c1 LTS
- **渲染管线**: URP (Universal Render Pipeline)
- **脚本位置**: `Assets/Scripts/`
- **场景位置**: `Assets/Scenes/PrototypeScene.unity`

### 📝 原型验证目标

1. 摇杆控制领主 + 吉米自动跟随是否感觉良好？
2. 3只吉米（不同属性）的协作是否有趣？
3. 协同攻击系统（3连击）是否令人满意？
4. 波次难度曲线是否合理？
5. 整体帧率性能是否可接受？

### 🐛 已知问题

- 敌人攻击玩家时还没有实际伤害逻辑（原型简化）
- 选择门UI需要手动设置
- 需要更多视觉反馈和特效

### 📁 文件结构

```
prototypes/tower-defense-rush/
├── Assets/
│   ├── Scripts/
│   │   ├── PlayerController.cs
│   │   ├── Jimmy.cs
│   │   ├── Enemy.cs
│   │   ├── WaveManager.cs
│   │   ├── GameManager.cs
│   │   ├── SynergySystem.cs
│   │   ├── CameraShake.cs
│   │   ├── ChoiceGateManager.cs
│   │   ├── Joystick.cs
│   │   ├── Testing/
│   │   │   ├── TestCaseBase.cs
│   │   │   ├── TestRunner.cs
│   │   │   ├── ITestCase.cs
│   │   │   ├── PerformanceTest.cs
│   │   │   ├── CombatSystemTest.cs
│   │   │   ├── JimmyAITest.cs
│   │   │   ├── WaveSystemTest.cs
│   │   │   ├── SynergySystemTest.cs
│   │   │   └── ChoiceGateTest.cs
│   │   └── Editor/
│   │       └── PrototypeSceneSetup.cs
│   └── Scenes/
│       └── PrototypeScene.unity
├── Packages/
│   └── manifest.json
└── ProjectSettings/
    └── ...
```
