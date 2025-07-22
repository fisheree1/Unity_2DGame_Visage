# Ghost (High Priest) 对话系统设置指南

## 概述
这个文档将指导你如何设置Ghost（大祭司）的对话系统。

## 文件说明
- `NewDialogue.cs` - Ghost的对话脚本，包含完整的对话逻辑
- 基于Child的对话系统结构开发

## 对话内容（英文翻译）

### 大祭司的对话内容：
1. **开场白**
   - "It's been fifteen years since we last met, hasn't it?"
   - "Now I'm old, my appearance has changed. You surely wouldn't recognize me."

2. **回忆过去**
   - "I don't know why you betrayed us at the last moment, but I'm still grateful to you."
   - "Thank you for teaching me magic back then, and for saving me from the monsters' prison."
   - "I can say that my current position is entirely built by you."

3. **认识到失败**
   - "The fact that you've come here means you have failed."

4. **关于神器和礼物**
   - "Back in prison, you told me your mission was to obtain the divine artifact."
   - "You also left me something, to be used at the cliff's edge on a full moon night."
   - "You said it would be useful in a moment of crisis."

5. **归还物品**
   - "Now I remain safe and sound, so I'll return this to you."
   - "Remember, use it at the cliff's edge on a full moon night."
   - "Walk to the right and you'll find what you seek."

6. **最后的告别**
   - "This is probably the last time we'll meet, old friend."
   - "May you find the redemption you seek."

## 设置步骤

### 1. 准备UI组件
你需要以下UI元素（与Child对话系统相同的结构）：

```
Canvas
├── InteractionUI (Panel)
│   └── InteractionText (TextMeshPro - Text UI)
└── DialoguePanel (Panel)
    ├── SpeakerNameText (TextMeshPro - Text UI)
    ├── DialogueText (TextMeshPro - Text UI)
    └── ContinueButton (Button)
        └── ButtonText (TextMeshPro - Text UI)
```

### 2. 挂载脚本
1. 将`NewDialogue.cs`脚本挂载到Ghost对象上
2. 在Inspector中设置所有UI引用：
   - interactionUI: 指向交互提示UI
   - interactionText: 指向交互提示文本
   - dialoguePanel: 指向对话面板
   - speakerNameText: 指向说话者名称文本
   - dialogueText: 指向对话内容文本
   - continueButton: 指向继续按钮

### 3. 配置设置
- **interactionRange**: 交互范围（默认3f）
- **interactKey**: 交互按键（默认E键）
- **typingSpeed**: 打字机效果速度（默认0.05f）

### 4. 角色设置
1. 确保Ghost对象有适当的Collider（如果需要物理交互）
2. 确保Player对象有"Player"标签
3. 确保Player对象有HeroMovement组件

## 功能特性

### 交互系统
- **范围检测**: 玩家靠近时显示交互提示
- **按键交互**: 按E键开始对话
- **自动隐藏**: 玩家离开时自动隐藏提示

### 对话系统
- **打字机效果**: 文字逐字显示，营造沉浸感
- **跳过动画**: 再次按E键或空格键可跳过打字动画
- **说话者区分**: 不同角色显示不同颜色
  - 玩家：青色 (Cyan)
  - 大祭司：洋红色 (Magenta) - 神秘感
- **移动禁用**: 对话期间禁用玩家移动

### 视觉效果
- **Scene视图**: 紫红色范围指示器显示交互范围
- **颜色编码**: 基于角色的文本颜色

## 控制说明

### 交互控制
- `E键`: 开始对话 / 继续对话 / 跳过打字动画
- `空格键`: 继续对话 / 跳过打字动画（对话进行中）

### 自动行为
- 玩家进入范围：显示交互提示
- 玩家离开范围：隐藏交互提示
- 对话开始：禁用玩家移动
- 对话结束：恢复玩家移动

## 调试信息

脚本会在Console中输出以下调试信息：
- 对话系统初始化信息
- 对话开始/结束信息
- 每行对话的显示信息

## 自定义选项

### 修改对话内容
在`InitializeDialogue()`方法中编辑`dialogueLines`数组

### 调整视觉效果
- 修改`typingSpeed`调整打字速度
- 在`DisplayCurrentLine()`中修改颜色设置
- 调整`interactionRange`改变交互范围

### 添加音效
可以在以下位置添加音效：
- `StartDialogue()`: 对话开始音效
- `StartTyping()`: 打字音效
- `ContinueDialogue()`: 按键音效

## 故障排除

### 常见问题
1. **交互提示不显示**: 检查Player标签和HeroMovement组件
2. **对话不开始**: 检查UI引用是否正确设置
3. **文本不显示**: 检查TextMeshPro组件和字体设置
4. **玩家无法移动**: 检查对话是否正确结束

### 调试步骤
1. 检查Console中的调试信息
2. 确认所有UI引用都已在Inspector中设置
3. 测试交互范围是否合适
4. 确认Player对象配置正确

## 扩展建议

1. **添加选择分支**: 修改对话系统支持玩家选择
2. **物品给予**: 在对话结束时给予玩家物品
3. **条件对话**: 根据游戏状态显示不同对话
4. **动画集成**: 添加角色动画与对话同步
