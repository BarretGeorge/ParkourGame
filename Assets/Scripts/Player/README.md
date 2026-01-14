# 3D跑酷游戏 - 快速开始指南

## 项目概况

这是一个商业级3D无限跑酷游戏项目，使用Unity 6开发。项目采用模块化设计，包含完整的玩家控制、关卡生成、障碍物、收集品、UI等系统。

## 当前版本：Alpha v0.1

### 已实现功能
- ✅ 核心移动控制系统
- ✅ 车道变道系统（3-7条车道）
- ✅ 跳跃系统（单跳+二段跳）
- ✅ 智能相机跟随系统
- ✅ 速度递增机制
- ✅ 基础碰撞检测
- ✅ 金币收集框架

## 快速开始

### 1. 在Unity中打开项目
```bash
# Unity会自动识别项目结构
# 直接用Unity Hub打开 /workspaces/code/games 目录
```

### 2. 设置游戏场景

按照 `SCENE_SETUP_GUIDE.md` 的详细说明进行设置。

**简略步骤**：
1. 创建Player对象并添加脚本
2. 创建地面
3. 配置相机
4. 运行测试

### 3. 测试控制

| 按键 | 功能 |
|------|------|
| A / ← | 向左变道 |
| D / → | 向右变道 |
| Space / W / ↑ | 跳跃（可二段跳） |
| S / ↓ | 下滑（待实现） |
| F1 | 传送（调试） |
| F5 | 重置玩家（调试） |
| F10 | 暂停/继续（调试） |

## 核心系统说明

### PlayerController
玩家的核心控制器，整合所有子系统。

**主要属性**：
- `CurrentSpeed` - 当前速度
- `DistanceTraveled` - 行驶距离
- `CoinsCollected` - 收集金币数
- `Score` - 当前分数

**事件**：
- `OnPlayerDeath` - 玩家死亡事件
- `OnSpeedChanged` - 速度变化事件
- `OnCoinCollected` - 金币收集事件

### LaneManager
管理玩家的车道切换。

**方法**：
- `MoveLeft()` - 向左移动
- `MoveRight()` - 向右移动
- `ChangeToLane(int)` - 切换到指定车道

### PlayerJump
处理跳跃和重力。

**方法**：
- `Jump()` - 执行跳跃
- `CanJump()` - 检查是否可跳跃
- `IsGrounded` - 是否在地面

### CameraController
智能相机系统。

**特性**：
- 平滑跟随
- 动态FOV
- 相机倾斜
- 死亡特效
- 相机震动

## 配置参数

所有游戏参数通过 **PlayerData** ScriptableObject配置：

### 移动参数
- `Base Speed` - 基础速度（默认10m/s）
- `Max Speed` - 最大速度（默认30m/s）
- `Speed Increase Rate` - 速度增长率（默认0.5m/s²）

### 车道参数
- `Lane Count` - 车道数量（默认3条）
- `Lane Width` - 车道宽度（默认3米）
- `Lane Change Speed` - 变道速度（默认10m/s）

### 跳跃参数
- `Jump Height` - 跳跃高度（默认6米）
- `Double Jump Height` - 二段跳高度（默认5米）
- `Gravity Multiplier` - 重力倍率（默认2x）
- `Enable Double Jump` - 启用二段跳

## 文件结构

```
Assets/Scripts/
├── Player/
│   ├── PlayerController.cs    # 主控制器
│   ├── PlayerData.cs          # 配置数据
│   ├── LaneManager.cs         # 车道管理
│   └── PlayerJump.cs          # 跳跃系统
├── Camera/
│   └── CameraController.cs    # 相机控制
└── Utilities/
    ├── TagsAndLayers.cs       # 常量定义
    └── EditorTestTools.cs     # 测试工具
```

## 开发路线图

当前进度查看 `PROJECT_PROGRESS.md` 或 `DEVELOPMENT_PLAN.md`

- [x] **Phase 1**: 核心移动控制系统 ✓
- [ ] **Phase 2**: 跑酷动作系统（滑铲、攀爬等）
- [ ] **Phase 3**: 无限关卡生成器
- [ ] **Phase 4**: 障碍物系统
- [ ] **Phase 5-15**: 后续系统...

## 调试技巧

### 查看调试信息
PlayerController在运行时显示调试面板（左上角）：
- 当前速度
- 行驶距离
- 分数和金币
- 车道位置
- 跳跃状态

### 场景视图可视化
- 选中Player：可见车道线（青色=所有车道，绿色=当前车道）
- 选中Camera：可见目标位置（黄色球）和观察点（绿色球）

### 编辑器测试工具
添加 `EditorTestTools.cs` 组件可获得：
- F1: 传送功能
- F2: 切换测试速度
- F3: 无敌模式
- F5: 重置玩家
- F10: 暂停/继续

## 下一步

推荐开发顺序：
1. **Phase 2**: 添加滑铲、攀爬等动作
2. **Phase 3**: 实现关卡生成器
3. **Phase 4**: 创建障碍物系统

或根据项目需求调整开发顺序。

## 技术支持

- 查看文档: `SCENE_SETUP_GUIDE.md`, `DEVELOPMENT_PLAN.md`, `PROJECT_PROGRESS.md`
- 代码注释: 所有核心脚本都有详细的中文注释
- 调试工具: 使用内置的EditorTestTools进行测试

## 贡献指南

1. 保持代码风格一致
2. 添加详细的注释
3. 更新相关文档
4. 测试所有功能

---

**版本**: Alpha v0.1
**最后更新**: 2026-01-14
**Unity版本**: Unity 6 (6000.0.27f1)
