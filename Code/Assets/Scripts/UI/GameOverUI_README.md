# GameOverUI 设置指南

## 概述
这个文档将指导你如何设置和使用GameOverUI系统。

## 文件说明
- `GameOverUI.cs` - 主要的GameOverUI脚本，包含所有UI逻辑
- `GameOverUIManager.cs` - 简化的管理器脚本，用于更容易地管理GameOverUI
- `HeroLife.cs` - 已更新以支持新的GameOverUI系统

## 设置步骤

### 1. 准备GameOverUI预制体
在你的`GameOverUI.prefab`中，你需要以下UI元素：

```
GameOverUI (Canvas/Panel)
├── GameOverPanel (Panel)
│   ├── GameOverText (TextMeshPro - Text UI) - 显示"游戏结束"文本
│   ├── ButtonsContainer (Panel)
│   │   ├── RespawnButton (Button)
│   │   │   └── RespawnButtonText (TextMeshPro - Text UI)
│   │   ├── RestartButton (Button)
│   │   │   └── RestartButtonText (TextMeshPro - Text UI)
│   │   ├── MainMenuButton (Button)
│   │   │   └── MainMenuButtonText (TextMeshPro - Text UI)
│   │   └── QuitButton (Button)
│   │       └── QuitButtonText (TextMeshPro - Text UI)
│   └── (可选) AudioSource
```

**注意**: 所有文本组件都使用 **TextMeshPro - Text (UI)** 而不是普通的Text组件。

### 2. 挂载脚本
1. 将`GameOverUI.cs`脚本挂载到GameOverUI根对象上
2. 在Inspector中设置所有UI引用：
   - gameOverPanel: 指向GameOverPanel对象
   - respawnButton: 指向复活按钮
   - restartButton: 指向重启按钮
   - mainMenuButton: 指向主菜单按钮
   - quitButton: 指向退出按钮
   - 各种TextMeshProUGUI组件的引用（注意是TextMeshPro组件，不是Text）

### 3. 配置HeroLife
1. 在Hero对象的HeroLife组件中：
   - 将gameOverUI字段设置为你的GameOverUI预制体实例
   - 确保useCheckPointRespawn设置正确
   - 设置showGameOverOnNoCheckPoint根据需要

### 4. 场景设置
1. 将GameOverUI预制体拖到场景中
2. 确保它是Canvas的子对象
3. 设置合适的Canvas层级，确保GameOverUI在其他UI元素之上

### 5. TextMeshPro 特殊设置
由于使用了TextMeshPro，请确保：
1. **导入TextMeshPro资源**: 如果第一次使用TextMeshPro，Unity会提示导入TMP Essential Resources
2. **字体设置**: 在TextMeshPro组件中设置合适的字体资源
3. **中文字体支持**: 如果使用中文，需要：
   - 使用支持中文的字体文件
   - 或者创建包含中文字符的字体资源文件

**创建UI文本时的步骤**：
1. 右键点击Canvas → UI → Text - TextMeshPro
2. 在弹出的对话框中点击"Import TMP Essentials"（如果是第一次使用）
3. 选择合适的字体资源

## 功能特性

### 按钮功能
- **复活按钮**: 只有当有有效复活点时才显示，点击后在复活点重生
- **重启按钮**: 重新加载当前场景
- **主菜单按钮**: 返回到主菜单场景
- **退出按钮**: 退出游戏

### 键盘快捷键
- `R键`: 重启游戏
- `空格键/回车键`: 复活（如果有复活点）
- `ESC键`: 返回主菜单

### 动画效果
- 淡入淡出效果
- 缩放动画
- 可自定义动画曲线

### 音效支持
- 游戏结束音效
- 按钮点击音效

## 使用示例

### 从代码中显示GameOverUI
```csharp
// 通过HeroLife脚本（已集成）
heroLife.TakeDamage(heroLife.MaxHealth);

// 直接通过GameOverUI脚本
var gameOverUI = FindObjectOfType<GameOverUI>();
if (gameOverUI != null)
{
    gameOverUI.Show();
}

// 通过GameOverUIManager
var gameOverManager = FindObjectOfType<GameOverUIManager>();
if (gameOverManager != null)
{
    gameOverManager.ShowGameOver();
}
```

### 自定义设置
你可以在Inspector中调整以下设置：
- 动画持续时间
- 颜色和视觉效果
- 音效
- 场景名称/索引
- 按钮显示逻辑

## 故障排除

### 角色死亡后不显示GameOverUI的常见原因

**1. 检查HeroLife设置**
- 在HeroLife组件的Inspector中，确保`gameOverUI`字段已设置
- 检查`useCheckPointRespawn`设置（现在无论如何都会显示GameOverUI）
- 使用HeroLife组件右键菜单中的"调试GameOverUI设置"来检查

**2. 检查GameOverUI对象**
- 确保GameOverUI预制体已放置在场景中
- 确保GameOverUI是Canvas的子对象
- 检查GameOverUI对象上是否挂载了GameOverUI.cs脚本

**3. 使用调试工具**
- 将`GameOverUIDebugger.cs`脚本挂载到任意对象上
- 运行游戏后按Y键检查设置
- 按T键测试死亡功能
- 按U键强制显示GameOverUI

### 常见问题
1. **编译错误**: 确保所有脚本都在正确的文件夹中
2. **按钮不响应**: 检查EventSystem是否存在于场景中
3. **音效不播放**: 确保AudioSource组件已添加并配置
4. **复活不工作**: 检查HeroLife中的复活点设置
5. **TextMeshPro问题**:
   - **文本不显示**: 检查是否导入了TMP Essential Resources
   - **中文显示异常**: 确保使用支持中文的字体，或创建包含中文字符的字体资源
   - **脚本引用错误**: 确保Inspector中引用的是TextMeshProUGUI组件，不是Text组件
6. **GameOverUI不显示**:
   - **引用为空**: 检查HeroLife中的gameOverUI引用是否已设置
   - **对象未激活**: 确保GameOverUI对象在场景中且父对象都是激活状态
   - **脚本缺失**: 确保GameOverUI对象上挂载了GameOverUI.cs脚本
   - **Canvas层级**: 确保GameOverUI的Canvas层级足够高

### 调试技巧
- 在Console中查看调试信息
- 使用Unity的UI调试工具
- 检查Canvas的Render Mode设置

## 进阶自定义

如果你需要更多功能，可以：
1. 继承GameOverUI类添加新功能
2. 修改动画系统
3. 添加更多按钮或UI元素
4. 集成存档/载入系统
