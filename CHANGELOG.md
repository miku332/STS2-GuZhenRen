# 更新日志 / Changelog

## 0.5.2

### 中文

#### 修复

- 修复爱情蛊抽到时，可能因为进手牌时序问题导致效果不触发的问题。
- 修复鸿运齐天蛊、运算蛊、转运等概率修正会错误提高“意乱”失败概率的问题；现在这些概率修正仅对“意乱”反向生效。

#### 兼容性

- 支持《杀戮尖塔2》正式版 `v0.107.1`。
- 需要 RitsuLib `0.5.12`。
- 测试版 `v0.111.0` 请使用 `v0.5.2-beta.1`。

### English

#### Fixes

- Fixed Love Gu sometimes failing to trigger when drawn due to hand-entry timing.
- Fixed probability modifiers such as Heaven-Defying Luck Gu, Calculation Gu, and Luck Conversion incorrectly increasing Distracted Mind's failure chance; these modifiers now apply in reverse only for Distracted Mind.

#### Compatibility

- Supports the stable `v0.107.1` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For public beta `v0.111.0`, use `v0.5.2-beta.1`.

## 0.5.1

### 中文

#### 修复

- 恢复悔蛊的原版效果：从消耗堆选择另一张悔蛊时，现在会生成一张遗憾。
- 修复龙公阶段转换对话阻塞战斗流程，以及转换条件重复判定时可能重复播放的问题。
- 修正“上房揭瓦”“骨道道痕”和“意乱”的简体中文名称。

#### 兼容性

- 支持《杀戮尖塔2》正式版 `v0.107.1`。
- 需要 RitsuLib `0.5.12`。
- 测试版 `v0.111.0` 请使用 `v0.5.1-beta.1`。

### English

#### Fixes

- Restored the original Regret Gu behavior: selecting another Regret Gu from the exhaust pile now generates a Regret.
- Fixed Duke Long's phase-transition dialogue blocking combat flow or replaying when the transition condition was evaluated more than once.
- Corrected the Simplified Chinese names of Break the Roof, Bone Path Dao Mark, and Distracted Mind.

#### Compatibility

- Supports the stable `v0.107.1` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For public beta `v0.111.0`, use `v0.5.1-beta.1`.

## 0.5.0

### 中文

#### 调整

- 力量蛊：六转获得固有 -> 八转获得固有。
- 智慧蛊：九转获得固有 -> 八转获得固有。
- 地灾·魂爆蛊：1/2/3/4+名敌人时，24/16/12/10层 -> 20/14/10/8层。
- 地灾·水幕天华蛊：对拥有壁垒的敌人，25层 -> 20层。
- 天劫·死期将至：墨影幻灵获得后，改为回合结束时获得10点力量。
- 天劫·木魅蛊：生命回复比例95% -> 90%。
- “好友”现在视为负面状态。

#### 新增7张杀招

- 未来身：宙道，3费，持续3场战斗，升级所有牌；每场战斗限一次，可通过未使用的杀招蛊方加入对应杀招。
- 燃念飞石：智道，0费，造成4点伤害并给予4层焚烧；获得念时可从消耗堆触发。
- 涅槃火：炎道，2费，失去所有生命并回复至最大生命值的25%，给予等同于所失生命值的焚烧，消耗。
- 血染征袍：血道，0费，失去1点生命，获得等同于已损失生命值的格挡。
- 吃心：食道，2费，斩杀当前生命低于玩家最大生命值的敌人，斩杀时提升2点最大生命值。
- 乱方混向雾：智道，2费，击晕所有敌人；敌人无法行动的初始概率为50%，每回合降低20%，消耗。
- 因果神树：木道，3费，抵消未被格挡的伤害或负面状态并存入果，加入来因去果。

#### 其他新增

- 新增来因去果：木道，2费，将所有果随机转移给敌人。
- 新增意乱：不可打出、虚无；在手牌中时，打出杀招有50%概率失败并失去6点生命。
- 新增未来身衍生遗物：持续3场战斗，战斗开始时升级所有牌，并可通过杀招蛊方获得对应杀招。
- 新增7张杀招对应的蛊方遗物，独立显示在遗物栏第二行。
- 新增因果神树的果机制：储存未被格挡的伤害和负面状态，2回合后生效，并可转移给敌人。

#### 第四幕：天庭

- 新增方源专属第四幕“天庭”，路线为：火堆 -> 商店 -> 最终战。
- 选人界面新增第四幕开关，默认开启，仅在方源单人模式显示。
- 开关支持自由拖动、保存位置，并在分辨率变化后保持在屏幕范围内。

#### 最终BOSS：龙公

- 新增龙公最终战，初始敌人为气墙与龙公。
- 气墙：250生命值；气护会将龙公受到的伤害与负面状态转移至自身；回复意图回复30点生命。
- 龙公：800生命值；龙驭上宾40层，回合结束时失去40点最大生命并获得4点力量；九龙纹护身9层，每层降低10%伤害，受伤后减少1层。
- 第一阶段：乱龙拳造成4x3段伤害并每段加入意乱；气呼山造成32点伤害；龙爪击造成8点伤害并给予2层脆弱；第三回合召唤气劲。
- 气墙存活时召唤自转游龙气墙；气墙被击败时召唤紫金龙形气劲。
- 人气溃散、地气溃散、天气溃散分别造成少抽1张牌、失去1点能量、失去6点生命。
- 三种气溃散合并为仙窍崩溃，持续5回合；期间每回合少抽2张牌、失去2点能量和10点生命，结束后本命蛊毁灭。
- 第一阶段生命归零时触发三气归来：生命上限重置为800，回复全部生命，清除负面状态，龙驭上宾提升至200层，九龙纹护身恢复至9层，进入第二阶段。
- 第二阶段依次使用气盖山河、回旋龙牙、撼世龙锤、一气大手爆，数值分别为10点伤害并施加10层易伤/虚弱/脆弱、1x2点伤害并恢复4层护身、5点伤害、40点伤害。
- 第二阶段持续4回合，结束后龙公生命上限归零并死亡。
- 自转游龙气墙：500最大生命值，继承气墙剩余生命值+100；游龙3层，每打出1张牌回复3点生命；罡气100层，本回合最多失去100点生命；回复40点生命，强化回复20点生命并获得1层游龙。
- 紫金龙形气劲：125生命值；潜龙气爆造成6x2段伤害；龙气获得2点力量并给予龙公4层九龙纹护身。

#### 修复

- 修复血狂蛊左右两侧存在不能被打出的牌时，可能导致卡牌悬浮在空中且无法继续操作的问题。

### English

#### Changes

- Strength Gu: Innate rank 6 -> rank 8.
- Wisdom Gu: Innate rank 9 -> rank 8.
- Soul Explosion Gu: 24/16/12/10 stacks -> 20/14/10/8 stacks for 1/2/3/4+ enemies.
- Water Curtain Heaven Gu: 25 -> 20 stacks against enemies with Barrier.
- Death Approaches: when applied to Vantom, it now grants 10 Strength at the end of the turn.
- Wood Charm Gu: healing ratio 95% -> 90%.
- Friend is now treated as a debuff.

#### Added 7 Killer Moves

- Future Body, Burning Thought Flying Stone, Nirvana Fire, Blood-Stained Battle Robe, Eat Heart, Chaotic Directional Fog, and Causal Divine Tree.
- Added Cause and Effect, Distracted Mind, the Future Body relic, all related recipes, and the Causal Divine Tree Fruit system.
- Killer Move recipe relics now use a dedicated second relic row.

#### Act 4: Heavenly Court

- Added Fang Yuan’s exclusive Act 4 with the route Rest Site -> Shop -> Final Battle.
- Added a default-enabled Act 4 toggle for Fang Yuan in single-player character selection.
- The toggle can be dragged, remembers its position, and remains within the screen after resolution changes.

#### New Final Boss: Long Gong

- Added the Long Gong final encounter with Qi Wall and Long Gong.
- Qi Wall has 250 HP, redirects Long Gong’s damage and debuffs through Qi Protection, and heals for 30 HP.
- Long Gong has 800 HP, Dragon Guest 40, and Nine Dragon Pattern Protection 9. Dragon Guest removes 40 Max HP and grants 4 Strength at turn end; each protection stack reduces damage by 10% and is removed when damaged.
- Phase One includes Wild Dragon Fist (4x3 damage and one Distracted Mind per hit), Qi Roars at the Mountain (32 damage), Dragon Claw Strike (8 damage and 2 Frail), and Summon.
- Human, Earth, and Heaven Qi Dissipation reduce draw by 1, remove 1 Energy, and remove 6 HP at turn start. Together they become Aperture Collapse for 5 turns, causing -2 draw, -2 Energy, and -10 HP; the Ben Ming Gu is destroyed when it expires.
- At zero HP, Three Qi Return restores 800 HP, clears debuffs, raises Dragon Guest to 200, restores protection to 9, and starts Phase Two.
- Phase Two lasts 4 turns and uses attacks with values 10, 1x2, 5, and 40; Long Gong dies when the phase ends.
- Added Rotating Wandering Dragon Qi Wall with 500 Max HP, Wandering Dragon 3, and Gang Qi 100, plus Purple-Gold Dragon Qi with 125 HP and a 6x2 attack.

#### Fix

- Fixed Bloodcraze Gu becoming stuck when unplayable cards were present on either side of it.

## 0.4.11

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
- 支持《杀戮尖塔2》正式版 `v0.107.1`。
- 需要 RitsuLib `0.5.12`。
- 测试版 `v0.111.0` 请使用 `v0.4.11-beta.1`。

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
- Supports the stable `v0.107.1` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For public beta `v0.111.0`, use `v0.4.11-beta.1`.

## 0.4.10

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

## 0.4.9

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
- 支持《杀戮尖塔2》正式版 `v0.107.1`。
- 需要 RitsuLib `0.5.12`。
- 测试版 `v0.111.0` 请使用 `v0.4.9-beta.1`。

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
- Supports the stable `v0.107.1` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For public beta `v0.111.0`, use `v0.4.9-beta.1`.

## 0.4.8

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
- 支持《杀戮尖塔2》正式版 `v0.107.1`。
- 需要 RitsuLib `0.5.12`。
- 测试版 `v0.111.0` 请使用 `v0.4.8-beta.1`。

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
- Supports the stable `v0.107.1` build of Slay the Spire 2.
- Requires RitsuLib `0.5.12`.
- For public beta `v0.111.0`, use `v0.4.8-beta.1`.


## 0.4.4

### 中文

#### 调整
- 将定仙游从达弗遗物池调整至特兹卡塔拉遗物池。

#### 修复
- 修复血系效果在仅格挡伤害、没有实际失去生命时错误触发的问题。
- 修复定仙游遗物池配置错误的问题。

#### 注意
- 本版本仅支持《杀戮尖塔2》正式版 `v0.107.1`。
- 测试版 `v0.111.0 public-beta` 请使用 `v0.4.4-beta.1`。

### English

#### Changes
- Moved Fixed Immortal Travel from Darv's relic pool to Tezcatara's relic pool.

#### Fixes
- Fixed blood-related effects triggering when damage was fully blocked and no HP was actually lost.
- Fixed the relic pool configuration for Fixed Immortal Travel.

#### Notice
- This version supports the official release `v0.107.1` only.
- For `v0.111.0 public-beta`, use `v0.4.4-beta.1`.

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
