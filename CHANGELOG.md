# 更新日志 / Changelog

## 0.5.1-beta.1

本测试版包含 `v0.5.1` 的全部修复，并适配《杀戮尖塔2》测试版 `v0.111.0`：恢复悔蛊选择另一张悔蛊时生成遗憾的原版效果；修复龙公阶段转换对话阻塞或重复播放的问题；修正“上房揭瓦”“骨道道痕”和“意乱”的简体中文名称。

This beta release contains all `v0.5.1` fixes and targets the public beta `v0.111.0` of Slay the Spire 2: restored Regret Gu generating a Regret when another Regret Gu is selected; fixed Duke Long's phase-transition dialogue blocking or replaying; and corrected the Simplified Chinese names of Break the Roof, Bone Path Dao Mark, and Distracted Mind.

## 0.5.0-beta.1

本测试版包含 `v0.5.0` 的全部内容，并适配《杀戮尖塔2》测试版 `v0.111.0`。完整中英文更新内容与正式版 `v0.5.0` 相同。

This beta release contains all `v0.5.0` content and targets the public beta `v0.111.0` of Slay the Spire 2. The full Chinese and English changelog is the same as the stable `v0.5.0` release.

## 0.4.11-beta.1

### 中文

#### 新增
- 新增本命蛊永久降级下限保护。镜中倒影、欢迎来到旺购百货等永久降级效果，无法将本命蛊降低至当前空窍转数以下。

#### 优化
- 卡牌概率现在会显示鸿运齐天蛊等效果影响后的实际概率。
- 统一蛊方遗物的显示格式，并参考塔1精简蛊方描述。
- 调整送友风蛊方材料，使其与塔1一致。
- 优化十转空窍描述，不再错误提及本命蛊转数。

#### 修复
- 修复十转后仍会继续遭遇灾劫的问题。
- 修复积土与坚固钳子的格挡保留冲突，现在会保留两者中的较高数值。

#### 兼容性
- 适用于《杀戮尖塔2》测试版 `v0.111.0`。
- 需要 RitsuLib `0.5.12`。
- 正式版 `v0.107.1` 请使用 `v0.4.11`。

### English

#### Added
- Added a permanent downgrade floor for Ben Ming Gu. Permanent downgrade effects such as Reflections and Welcome to Wongo's can no longer reduce it below the current aperture rank.

#### Improvements
- Card probabilities now display their effective values after modifiers such as Heaven-Defying Luck Gu.
- Unified Killer Move recipe relic formatting and simplified recipe descriptions based on the Slay the Spire 1 version.
- Updated the Farewell Friend Wind recipe ingredients to match the original mod.
- Clarified the Rank 10 aperture description so it no longer incorrectly refers to the Ben Ming Gu rank.

#### Fixes
- Fixed tribulations continuing after reaching Rank 10.
- Fixed the interaction between Accumulated Earth and Sturdy Clamp; the higher Block retention value is now preserved.

#### Compatibility
- Supports the public beta `v0.111.0` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For the stable `v0.107.1` build, use `v0.4.11`.

## 0.4.10-beta.1

### 中文

#### 修复
- 修复本命蛊被合炼或移除后，在后续篇章重复出现本命蛊选择页面的问题。
- 修复最终 Boss 战触发爱情蛊逃跑后，没有“前往”按钮或直接跳转的问题。
- 爱情蛊逃离 Boss 战后现在会显示无奖励页面，点击“前往”后继续正常流程。

### English

#### Fixes
- Fixed the Ben Ming Gu selection screen reappearing in later acts after the Ben Ming Gu was synthesized or removed.
- Fixed Love Gu escape during the final boss fight skipping the Proceed button or transitioning immediately.
- Escaping a boss fight with Love Gu now displays an empty reward screen with a Proceed button before continuing normally.

## 0.4.9-beta.1

### 中文

#### 新增
- 新增基于 RitsuLib 的更新检测，并区分正式版与测试版更新渠道。
- 创意工坊安装的模组将跳转至创意工坊页面，手动安装的模组将跳转至手动下载页面。

#### 修复
- 修复超巨化药水仅强化锯齿金蜈第一段伤害的问题，现在会强化全部攻击段数。
- 修复有力气蛊期间虚影牌占用手牌上限的问题，恢复塔1行为。
- 修复满手牌时生成的虚影牌会进入弃牌堆的问题，现在可正常加入手牌。
- 更新有力气蛊及对应能力的中英文描述。

#### 兼容性
- 适用于《杀戮尖塔2》测试版 `v0.111.0`。
- 需要 RitsuLib `0.5.12`。
- 正式版 `v0.107.1` 请使用 `v0.4.9`。

### English

#### Added
- Added RitsuLib-based update checks with separate stable and beta update channels.
- Workshop installations open the Workshop page, while manual installations open the manual download page.

#### Fixes
- Fixed Gigantification only enhancing the first hit of Sawtooth Golden Centipede; it now enhances every hit.
- Fixed Phantom cards counting toward the hand limit while Effort Gu is active, restoring the Slay the Spire 1 behavior.
- Fixed Phantoms generated with a full hand being sent to the discard pile; they can now enter the hand normally.
- Updated the Chinese and English descriptions for Effort Gu and its related Power.

#### Compatibility
- Supports the public beta `v0.111.0` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For the stable `v0.107.1` build, use `v0.4.9`.

## 0.4.8-beta.1

### 中文

#### 新增
- 爱情蛊新增“正面效果”与“负面效果”关键词，可查看各项随机结果及触发概率。
- 火堆杀招选择页面新增返回按钮，可直接返回火堆。

#### 优化
- 火堆选项“合炼杀招”调整为“杀招（不消耗行动）”，明确合炼不会消耗火堆行动。
- 优化金刚念、多重剑影蛊、时针、挽澜、拔山、鼎力等卡牌的升级数值显示。
- 青牛劳力蛊、驰马骏力蛊、黑蟒缠力蛊、白象元力蛊、石龟负力蛊、飞熊之力蛊和我力现在会明确显示生成升级版虚影。
- 防伪蛊描述调整为塔1样式，明确复制范围、复制牌状态及仙蛊唯一规则。
- 只有一个可合炼蛊方时，也会先选择杀招，再进入材料选择页面。
- 存在多个可合炼杀招时，可直接点击另一张杀招切换选择，无需先取消当前选择。

#### 修复
- 修复金刚念升级效果未在卡面正确显示的问题。
- 修复仙蛊残骸仍有剩余次数时，无法在同一火堆继续锻造的问题。
- 修复送友风蛊方无法正确识别偷道蛊虫的问题。
- 修复见面曾相识蛊方无法正确识别偷道蛊虫的问题。
- 修复冷血在怪物回合开始时击杀敌人，可能导致活体盾与高塔炮手等特定战斗无法继续的问题。

#### 兼容性
- 适用于《杀戮尖塔2》测试版 `v0.111.0`。
- 需要 RitsuLib `0.5.12`。
- 正式版 `v0.107.1` 请使用 `v0.4.8`。

### English

#### Added
- Added Positive Effect and Negative Effect keywords to Love Gu, showing every possible random outcome and its probability.
- Added a back button to the Killer Move selection screen, allowing players to return directly to the rest site.

#### Improvements
- Renamed the rest-site option to “Killer Move (No Action Cost)” to clarify that assembly does not consume the rest-site action.
- Improved upgrade value display for Vajra Thought, Multiple Sword Shadow Gu, Clock Hand, Turning the Tide, Pulling Mountain, Tripod Strength, and other affected cards.
- Cards that create Phantoms now clearly indicate when the generated Phantom is upgraded.
- Updated Anti-counterfeit Gu’s description to match the Slay the Spire 1 version and clarify its copy pool, copy state, and Immortal Gu uniqueness interaction.
- When only one Killer Move recipe is available, players now select the Killer Move before choosing its materials.
- When multiple Killer Moves are available, clicking another card now switches the selection directly without requiring the current selection to be canceled first.

#### Fixes
- Fixed Vajra Thought’s upgrade effect not being displayed correctly.
- Fixed Immortal Gu Remains becoming unresponsive when additional forging uses were still available at the same rest site.
- Fixed the Farewell Wind recipe not recognizing Theft Path Gu correctly.
- Fixed the Familiar Face recipe not recognizing Theft Path Gu correctly.
- Fixed Cold Blood killing an enemy at the start of the enemy turn potentially locking encounters involving Living Shield, Turret Operator, and similar enemy combinations.

#### Compatibility
- Supports the public beta `v0.111.0` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For the stable `v0.107.1` build, use `v0.4.8`.


## 0.4.3

### 中文

#### 新增
- 新增仙窍升转专属语音。
- 升炼至九转仙窍时，将根据是否拥有杀蛊播放不同语音。
- 升炼至十转仙窍时，将播放对应的专属语音。

### English

#### Added
- Added unique voice lines for Immortal Aperture rank advancement.
- Advancing to Rank 9 plays a different voice line depending on whether Sha Gu is owned.
- Advancing to Rank 10 plays its corresponding unique voice line.

## 0.4.2

### 中文

#### 优化
- 更新方源的战斗与界面小头像。

### English

#### Improvements
- Updated Fang Yuan's in-game portrait.

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
