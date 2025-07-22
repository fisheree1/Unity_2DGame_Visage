# FinalBoss 脚本重构总结

## 更改概述
将冥想状态管理和无敌功能从 `FinalBossLife.cs` 移动到 `FinalBossMove.cs` 中，使 `FinalBossLife.cs` 专注于生命值管理。

## 主要更改

### 1. FinalBossMove.cs 的更改
- **新增字段**：
  - `meditationDuration`: 冥想持续时间
  - `invulnerabilityTime`: 无敌时间
  - `damageFlashDuration`: 伤害闪烁持续时间
  - `damageFlashColor`: 伤害闪烁颜色
  - `meditationTriggered[]`: 记录每个阶段是否已触发
  - `meditationThresholds[]`: 冥想触发阈值
  - `isInvulnerable`: 无敌状态
  - `originalColor`: 原始颜色

- **新增属性**：
  - `IsInvulnerable`: 获取无敌状态

- **新增方法**：
  - `HandleMeditationTrigger()`: 检查冥想状态触发
  - `TriggerMeditation(int stage)`: 触发冥想状态
  - `MeditationRoutine(int stage)`: 冥想状态协程
  - `InvulnerabilitySequence()`: 无敌状态协程
  - `DamageFlashEffect()`: 伤害闪烁效果

- **更新方法**：
  - `TakeHurt()`: 添加了无敌状态检查、伤害闪烁和无敌序列
  - `Update()`: 添加了冥想状态触发检查
  - `Start()`: 添加了原始颜色初始化

### 2. FinalBossLife.cs 的更改
- **移除功能**：
  - 所有冥想状态相关的字段和方法
  - 无敌状态管理
  - 伤害闪烁效果
  - Update() 方法和冥想触发逻辑

- **简化功能**：
  - 专注于生命值管理（TakeDamage, Heal, Die）
  - 保留事件系统（OnHealthChanged, OnDeath）
  - 保留掉落物品系统

- **更新逻辑**：
  - `TakeDamage()`: 现在检查 `FinalBossMove` 的无敌和冥想状态
  - 移除了 `UpdateHealthBar()` 和相关UI引用

### 3. 新增文件
- **Assets/FinalBoss/Scripts/FinalBoss/BossLife.cs**: 创建了一个通用的Boss生命值管理脚本，可以用于挂载在FinalBoss上

### 4. 更新了原始BossLife.cs
- 在 `Assets/Scripts/Boss/BossLife.cs` 中添加了 `GetHealthPercentage()` 方法

## 使用指南

### 对于FinalBoss：
1. 使用 `FinalBossMove.cs` 进行移动、AI和冥想状态管理
2. 使用 `FinalBossLife.cs` 进行生命值管理
3. 或者可以选择使用新创建的 `BossLife.cs` 替代 `FinalBossLife.cs`

### 组件依赖关系：
- `FinalBossMove` 依赖于 `BossLife` 或 `FinalBossLife` 来获取生命值信息
- `FinalBossLife` 依赖于 `FinalBossMove` 来检查无敌和冥想状态
- `FinalBossMove` 管理所有状态（冥想、无敌、伤害闪烁）

## 优点
1. **职责分离**: 每个脚本现在有明确的职责
2. **更好的复用性**: `BossLife.cs` 可以用于其他Boss
3. **简化的生命值管理**: `FinalBossLife.cs` 现在更简洁
4. **集中的状态管理**: 所有状态都在 `FinalBossMove.cs` 中管理
5. **保持兼容性**: 保留了所有原有的事件和功能

## 注意事项
- 确保在Unity中正确引用新的脚本
- 冥想状态现在完全由 `FinalBossMove.cs` 管理
- 无敌状态和伤害闪烁也由 `FinalBossMove.cs` 处理
- 生命值检查现在通过 `GetHealthPercentage()` 方法进行
