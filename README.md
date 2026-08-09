# STS2-GuZhenRen

`STS2-GuZhenRen` 是一个《杀戮尖塔2》蛊真人同人模组项目，目标是把一代项目 `Slay the Spire Mod - Reverend Insanity(蛊真人)` 的角色、卡牌和机制逐步移植到《杀戮尖塔2》。

项目目前仍在开发中，不是完整成品。当前版本主要用于开发和测试方源角色、空窍遗物、本命蛊系统、部分蛊虫卡牌，以及已经实现的基础机制。

## 项目解决什么问题

一代蛊真人模组不能直接在《杀戮尖塔2》中运行。本项目解决的是“把一代蛊真人模组的玩法内容迁移到塔2”这个问题，包括：

- 将一代 Java 模组逻辑改写为塔2可用的 C# 模组代码。
- 将一代卡牌、遗物、能力、图片素材逐步迁移到塔2。
- 适配塔2的 RitsuLib / BaseLib 模组框架。
- 让方源角色可以在塔2中正常进入战斗、打牌、结算效果。

## 主要功能

当前已实现或正在测试的内容包括：

- 方源角色基础注册。
- 空窍 1-5 遗物。
- 方源初始牌组。
- 本命蛊开局选择、同一牌组唯一性限制，以及高转本命蛊重复销毁补偿。
- 仙蛊残骸篝火锻造机制。
- 多张已移植卡牌，包括光道、剑道、力道、土道、血道、炎道等部分卡牌。
- 已实现的部分机制：
  - 虚影牌触发
  - 焚烧
  - 念
  - 情
  - 积土
  - 冷血
  - 血战、血幕天华等血道能力
- 炎道基础卡牌：
  - 炎胄蛊
  - 燎原火
  - 炎瞳蛊
  - 御火
- 路道联动卡牌：
  - 失败蛊
  - 成功蛊及财富、永生、自由三个选项
- 概率体系：
  - 转运
  - 概率失败后提高对应概率牌的基础概率
- 运算蛊：消耗手牌、抽牌，并提高当前手牌中概率牌的概率。
- 厄运：抽到时降低本场战斗概率牌的概率，经过3场战斗后从牌组移除。倒计时会正确保存和更新。
- 土道体系：化石蛊、可叠加的化石附魔，以及四选一仙蛊屋安土重山堡。

尚未完整实现的内容包括剩余未移植卡牌、其他杀招与仙蛊屋、配方、事件、药水、关键词提示系统等。

## 安装方法

### 1. 准备环境

需要安装：

- 《Slay the Spire 2》
- .NET SDK 9 或项目可用的兼容版本
- STS2-RitsuLib
- 本项目源码

构建时需要提供本机《Slay the Spire 2》安装目录和 Godot 可执行文件位置。推荐使用环境变量，避免把个人路径写入项目文件。

PowerShell 示例：

```powershell
$env:STS2_DIR="<你的Slay the Spire 2安装目录>"
$env:GODOT_EXE="<你的Godot可执行文件路径>"
```

也可以在构建命令中通过 `-p:Sts2Dir=...` 和 `-p:GodotExe=...` 临时指定。

### 2. 构建并部署

在项目根目录运行：

```powershell
dotnet build .\GuZhenRen.csproj -c Debug -p:ExportPck=true --no-restore --nologo
```

构建成功后，项目会自动把模组文件复制到：

```text
<你的Slay the Spire 2安装目录>\mods\GuZhenRen
```

成功输出通常包含：

```text
已成功生成。
0 个错误
```

`NU1900` NuGet 漏洞数据警告一般不影响本地构建，只要最后是 `0 个错误` 即可。

## 使用方法

1. 构建项目。
2. 启动《Slay the Spire 2》。
3. 确认本地模组 `GuZhenRen` 已加载。
4. 在角色选择界面选择方源。
5. 进入战斗后测试卡牌效果。

游戏日志位置：

```text
%APPDATA%\SlayTheSpire2\logs\godot.log
```

如果游戏崩溃、卡牌无法打出、数值异常，优先查看这个日志。

## 许可证与素材说明

除非另有说明，本项目代码使用 GPL-3.0-only 许可证发布，详见根目录 `LICENSE`。

本项目包含或可能包含用于同人模组开发的卡牌图片、角色图片、界面图片等素材。相关素材的原始权利归各自权利方所有，仅用于本模组的开发、测试和展示；这些素材不因本项目代码采用 GPL-3.0-only 而被重新授权。

## 输入输出示例

### 示例 1：构建项目

输入：

```powershell
dotnet build .\GuZhenRen.csproj -c Debug -p:ExportPck=true --no-restore --nologo
```

预期输出：

```text
GuZhenRen -> ...\Debug\GuZhenRen.dll
Copying Gu Zhen Ren mod to Slay the Spire 2...
Exporting Gu Zhen Ren PCK...
已成功生成。
0 个错误
```

### 示例 2：打出燎原火

输入：

```text
在敌人没有焚烧时，打出未升级的“燎原火”。
```

预期输出：

```text
所有敌人获得焚烧。
每个敌人会受到两次焚烧伤害：3 点，然后 6 点。
每个敌人合计受到 9 点焚烧伤害。
玩家手牌中加入 1 张灼伤。
```

### 示例 3：打出御火

输入：

```text
本回合先消耗 2 张牌，然后打出“御火”。
```

预期输出：

```text
所有敌人获得两次 1 层焚烧。
如果敌人原本没有焚烧，每个敌人会受到两次焚烧伤害：1 点，然后 2 点。
每个敌人合计受到 3 点焚烧伤害。
```

### 示例 4：查看日志

输入：

```powershell
Select-String -Path "$env:APPDATA\SlayTheSpire2\logs\godot.log" -Pattern "\[FenShao\]"
```

预期输出示例：

```text
[GuZhenRen] [FenShao] Burning damage: amount=3, source=CARD.GU_ZHEN_REN_CARD_LIAO_YUAN_HUO
[GuZhenRen] [FenShao] Burning damage: amount=6, source=CARD.GU_ZHEN_REN_CARD_LIAO_YUAN_HUO
```

这表示焚烧按照当前层数造成了 3 点和 6 点伤害。
