# 更新日志 / Changelog

## 0.4.0

### 中文

#### 新增
- 空窍悬停提示新增修为、当前转数、渡劫进度、下一灾劫及剩余战斗场数。
- 空窍升转时，本命蛊将播放塔2原生升级动画。

#### 优化
- 防伪蛊现在可以在战斗中复制已经存在的同名仙蛊。
- 防伪蛊的临时复制不会破坏牌组、奖励和其他来源的仙蛊唯一机制。
- 卡牌转数、流派、仙蛊等关键词按统一顺序显示。
- 参考塔1精简关键词介绍与卡牌描述，减少重复和过多提示。
- 刃蛊现在只显示当前转数对应的效果。
- 虚影牌不再重复显示“虚影”通用解释。
- 调整部分卡牌文本，使升级效果和当前状态表达更加清晰。

#### 修复
- 修复防伪蛊选择页面显示默认文字“Info text”的问题。
- 修复防伪蛊选择卡牌时可能出现异常、无法正常复制的问题。

### English

#### Added
- Added Aperture hover tips showing cultivation, current rank, tribulation progress, the next tribulation, and remaining combats.
- Vital Gu now plays the native Slay the Spire 2 upgrade animation when the Aperture advances.

#### Improvements
- Anti-Counterfeit Gu can now copy an Immortal Gu already present in combat.
- Temporary copies created by Anti-Counterfeit Gu do not affect Immortal Gu uniqueness rules for the deck, rewards, or other sources.
- Standardized the display order of card rank, path, and Immortal Gu tooltips.
- Simplified keyword explanations and card descriptions based on the original Slay the Spire 1 mod, reducing repetition and excessive tooltips.
- Blade Gu now displays only the effect corresponding to its current rank.
- Phantom cards no longer repeat the general Phantom explanation.
- Improved several card descriptions to present upgrade effects and current behavior more clearly.

#### Fixes
- Fixed the default “Info text” appearing on Anti-Counterfeit Gu’s card selection screen.
- Fixed an error that could prevent Anti-Counterfeit Gu from selecting and copying cards.

## 0.3.0

### 中文

- 新增先古能力牌「炼天魔尊」。
  - 每回合首次消耗有流派的非状态牌时，从 3 张相同流派的一至五转普通蛊虫中选择 1 张加入手牌。
  - 生成牌本回合耗能为 0，并获得消耗和虚无。
- 新增先古卡牌「太初光蛊」。
  - 获得 3 层闪耀。
  - 给予所有敌人 2 层虚弱。
  - 本回合闪耀不会因打出光道攻击牌而消失。
- 新增遗物「九转至尊仙胎蛊」。
  - 作为方源获得「欧洛巴斯之触」后的升级初始遗物。
  - 获得时继承当前空窍的转数与修炼进度。
  - 至尊仙窍无需渡劫。
- 修复直接获得「古老牙齿」时，无法将「小光蛊」变化为「太初光蛊」的问题。
- 修复空窍遗物替换或升级时，最大生命值可能重复增加的问题。
- 优化「九转至尊仙胎蛊」预览，不再默认显示一转凡窍，避免产生误解。
- 增加新增卡牌、能力、遗物的中文和英文文本。
- 增加英文语言文件，完善双语本地化支持。
- 增加 Godot 本地用户数据目录的忽略规则，降低个人环境信息进入版本库的风险。

本 mod 使用 AI 进行开发，可能会存在错误。遇到问题时，请尽量提供复现步骤、截图和游戏日志。

### English

- Added the Ancient card "Refining Heaven Demon Venerable".
  - The first time each turn you exhaust a non-Status card with a Dao path, choose 1 ordinary rank 1-5 Gu worm from 3 cards sharing that Dao path.
  - The generated card costs 0 this turn and gains Exhaust and Ethereal.
- Added the Ancient card "Primordial Light Gu".
  - Gain 3 stacks of Radiance.
  - Apply 2 Weak to all enemies.
  - This turn, Radiance is not removed by playing Light Dao Attacks.
- Added the relic "Rank Nine Sovereign Immortal Fetus Gu".
  - It replaces Touch of Orobas as Fang Yuan's upgraded starter relic.
  - When obtained, it inherits the current aperture's rank and cultivation progress.
  - The Sovereign Immortal Aperture does not require tribulations.
- Fixed direct acquisition of Archaic Tooth failing to transform Xiao Guang Gu into Primordial Light Gu.
- Fixed aperture relic replacement or advancement potentially granting duplicate Max HP bonuses.
- Improved the Rank Nine Sovereign Immortal Fetus Gu preview so it no longer defaults to a rank 1 mortal aperture.
- Added Chinese and English text for the new cards, powers, and relics.
- Added English localization files and improved bilingual localization support.
- Added ignore rules for Godot local user-data directories to reduce the risk of committing machine-specific information.

This mod is developed with AI assistance and may contain errors. Please report reproducible issues with steps, screenshots, and game logs when possible.

## 0.2.0

### 中文

- 新增本命蛊唯一机制：每局只能拥有 1 张本命蛊。
- 新增本命蛊主动移除惩罚：失去相当于最大生命值 80% 的生命值，至少保留 1 点生命。
- 新增普通仙蛊唯一机制，已拥有的仙蛊不会出现在战斗奖励和商店中。
- 新增卡牌品阶、流派、仙蛊、本命蛊和杀招分类显示。
- 新增卡牌分类和生成牌预览的悬浮说明。
- 优化杀招奖励池过滤和合炼材料处理。
- 优化构建部署，清理过期的本地调试符号文件。
- 更新模组清单版本为 0.2.0。

本 mod 使用 AI 进行开发，可能会存在错误。遇到问题时，请尽量提供复现步骤、截图和游戏日志。

### English

- Added Ben Ming Gu uniqueness: only one Ben Ming Gu can be owned per run.
- Added the Ben Ming Gu removal penalty: losing a Ben Ming Gu removes 80% of maximum HP, leaving at least 1 HP.
- Added uniqueness handling for owned Xian Gu in combat rewards and shops.
- Added card rank, Dao path, Xian Gu, Ben Ming Gu, and Sha Zhao category display.
- Added hover descriptions for card categories and generated card previews.
- Improved Sha Zhao reward filtering and recipe ingredient handling.
- Improved build deployment cleanup and removed stale local debug symbol files.
- Updated the mod manifest version to 0.2.0.

This mod is developed with AI assistance and may contain errors. Please report reproducible issues with steps, screenshots, and logs when possible.
