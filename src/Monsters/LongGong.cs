using GuZhenRen.Cards;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace GuZhenRen.Monsters;

[RegisterMonster]
public sealed class LongGong : ModMonsterTemplate
{
    private const int LuanLongQuanDamage = 4;
    private const int LuanLongQuanHits = 3;
    private const int QiHuShanDamage = 32;
    private const int LongZhaoJiDamage = 8;
    private const int LongZhaoJiFrail = 2;
    private const int QiGaiShanHeDamage = 10;
    private const int HuiXuanLongYaDamage = 1;
    private const int HanShiLongChuiDamage = 5;
    private const int YiQiDaShouBaoDamage = 40;

    private bool _introPlayed;
    private bool _hasSummoned;
    private bool _summonedYouLong;
    private bool _threeQiPreparation;
    private bool _phaseTransitionPending;
    private bool _phaseTransitionDialoguePlayed;
    private bool _secondPhase;
    private bool _skipLongYuThisTurn;

    public bool IsInSecondPhase => _secondPhase;

    public override int MinInitialHp => 800;

    public override int MaxInitialHp => 800;

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://GuZhenRen/scenes/monsters/long_gong.tscn");

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            AssetProfile.VisualsScenePath!);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        if (_introPlayed)
        {
            return;
        }

        _introPlayed = true;
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<LongYuShangBinPower>(
            context, Creature, 40, Creature, null);
        await PowerCmd.Apply<JiuLongWenHuShenPower>(
            context, Creature, 9, Creature, null);

        _ = TaskHelper.RunSafely(PlayIntroAfterCombatStarts());
    }

    private async Task PlayIntroAfterCombatStarts()
    {
        while (!CombatManager.Instance.IsInProgress)
        {
            if (CombatManager.Instance.IsEnding || Creature.IsDead)
            {
                return;
            }

            await Cmd.Wait(0.1f, ignoreCombatEnd: true);
        }

        if (Creature.IsDead)
        {
            return;
        }

        await Cmd.Wait(0.5f);
        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.intro.speakLine1"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Short);
        await Cmd.Wait(1.5f);

        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.intro.speakLine2"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Long);
        await Cmd.Wait(3f);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var luanLongQuan = new MoveState(
            "LUAN_LONG_QUAN",
            PerformLuanLongQuan,
            new MultiAttackIntent(LuanLongQuanDamage, LuanLongQuanHits),
            new StatusIntent(LuanLongQuanHits));
        var qiHuShan = new MoveState(
            "QI_HU_SHAN",
            PerformQiHuShan,
            new SingleAttackIntent(QiHuShanDamage));
        var summon = new MoveState(
            "SUMMON",
            PerformSummon,
            new SummonIntent());
        var renQi = new MoveState(
            "REN_QI_GUI_LAI",
            PerformRenQiGuiLai,
            new DebuffIntent());
        var diQi = new MoveState(
            "DI_QI_GUI_LAI",
            PerformDiQiGuiLai,
            new DebuffIntent());
        var tianQi = new MoveState(
            "TIAN_QI_GUI_LAI",
            PerformTianQiGuiLai,
            new DebuffIntent());
        var longZhaoJi = new MoveState(
            "LONG_ZHAO_JI",
            PerformLongZhaoJi,
            new SingleAttackIntent(LongZhaoJiDamage),
            new DebuffIntent());
        var sanQiGuiLai = new MoveState(
            "SAN_QI_GUI_LAI",
            PerformSanQiGuiLai,
            new BuffIntent())
        {
            MustPerformOnceBeforeTransitioning = true
        };
        var qiGaiShanHe = new MoveState(
            "QI_GAI_SHAN_HE",
            PerformQiGaiShanHe,
            new SingleAttackIntent(QiGaiShanHeDamage),
            new DebuffIntent());
        var huiXuanLongYa = new MoveState(
            "HUI_XUAN_LONG_YA",
            PerformHuiXuanLongYa,
            new MultiAttackIntent(HuiXuanLongYaDamage, 2),
            new BuffIntent());
        var hanShiLongChui = new MoveState(
            "HAN_SHI_LONG_CHUI",
            PerformHanShiLongChui,
            new SingleAttackIntent(HanShiLongChuiDamage));
        var yiQiDaShouBao = new MoveState(
            "YI_QI_DA_SHOU_BAO",
            PerformYiQiDaShouBao,
            new SingleAttackIntent(YiQiDaShouBaoDamage));

        var afterQiHuShan = new ConditionalBranchState("AFTER_QI_HU_SHAN");
        afterQiHuShan.AddState(summon, () => !_hasSummoned);
        afterQiHuShan.AddState(longZhaoJi, () => true);

        var afterSummon = new ConditionalBranchState("AFTER_SUMMON");
        afterSummon.AddState(renQi, () => _summonedYouLong);
        afterSummon.AddState(longZhaoJi, () => true);

        luanLongQuan.FollowUpState = qiHuShan;
        qiHuShan.FollowUpState = afterQiHuShan;
        summon.FollowUpState = afterSummon;
        renQi.FollowUpState = diQi;
        diQi.FollowUpState = tianQi;
        tianQi.FollowUpState = longZhaoJi;
        longZhaoJi.FollowUpState = luanLongQuan;

        sanQiGuiLai.FollowUpState = qiGaiShanHe;
        qiGaiShanHe.FollowUpState = huiXuanLongYa;
        huiXuanLongYa.FollowUpState = hanShiLongChui;
        hanShiLongChui.FollowUpState = yiQiDaShouBao;
        yiQiDaShouBao.FollowUpState = qiGaiShanHe;

        return new MonsterMoveStateMachine(
            [
                luanLongQuan, qiHuShan, afterQiHuShan, summon, afterSummon,
                renQi, diQi, tianQi, longZhaoJi, sanQiGuiLai,
                qiGaiShanHe, huiXuanLongYa, hanShiLongChui, yiQiDaShouBao
            ],
            luanLongQuan);
    }

    private async Task PerformLuanLongQuan(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(LuanLongQuanDamage)
            .WithHitCount(LuanLongQuanHits)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_attack_blunt")
            .BeforeDamage(AddYiLuanToEachPlayerDrawPile)
            .Execute(null);
    }

    private async Task AddYiLuanToEachPlayerDrawPile()
    {
        foreach (var player in CombatState.Players)
        {
            var yiLuan = CombatState.CreateCard<YiLuan>(player);
            await CardPileCmd.AddGeneratedCardToCombat(
                yiLuan,
                PileType.Draw,
                player,
                CardPilePosition.Random);
        }
    }

    private Task PerformQiHuShan(IReadOnlyList<Creature> targets) =>
        DamageCmd.Attack(QiHuShanDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(null);

    private async Task PerformSummon(IReadOnlyList<Creature> targets)
    {
        _hasSummoned = true;
        var wall = CombatState.Enemies.FirstOrDefault(enemy =>
            enemy.IsAlive && enemy.Monster is QiQiang);
        if (wall is not null)
        {
            var inheritedHp = Math.Min(500, wall.CurrentHp + 100);
            await CreatureCmd.Escape(wall);
            var upgradedWall = await CreatureCmd.Add(
                ModelDb.Monster<YouLongQiQiang>().ToMutable(),
                CombatState,
                CombatSide.Enemy,
                "qi_qiang");
            await CreatureCmd.SetCurrentHp(upgradedWall, inheritedHp);
            await EnsurePowerAmount<QiHuPower>(upgradedWall, 1);
            await EnsurePowerAmount<YouLongPower>(upgradedWall, 3);
            await EnsurePowerAmount<GangQiPower>(upgradedWall, 100);
            _summonedYouLong = true;
            _threeQiPreparation = true;
            return;
        }

        await CreatureCmd.Add(
            ModelDb.Monster<LongQi>().ToMutable(),
            CombatState,
            CombatSide.Enemy,
            "long_qi");
        _summonedYouLong = false;
        _threeQiPreparation = false;
    }

    private async Task EnsurePowerAmount<TPower>(
        Creature target,
        decimal amount)
        where TPower : PowerModel
    {
        var context = new ThrowingPlayerChoiceContext();
        var power = target.GetPower<TPower>();
        if (power is null)
        {
            await PowerCmd.Apply<TPower>(
                context, target, amount, Creature, null);
            return;
        }

        if (power.Amount != amount)
        {
            await PowerCmd.ModifyAmount(
                context, power, amount - power.Amount, Creature, null);
        }
    }

    private Task PerformRenQiGuiLai(IReadOnlyList<Creature> targets) =>
        ApplyToPlayers<RenQiKuiSanPower>(targets, 1);

    private Task PerformDiQiGuiLai(IReadOnlyList<Creature> targets) =>
        ApplyToPlayers<DiQiKuiSanPower>(targets, 1);

    private async Task PerformTianQiGuiLai(IReadOnlyList<Creature> targets)
    {
        await ApplyToPlayers<TianQiKuiSanPower>(targets, 1);
        _threeQiPreparation = false;
    }

    private Task ApplyToPlayers<TPower>(
        IReadOnlyList<Creature> targets,
        int amount)
        where TPower : PowerModel =>
        PowerCmd.Apply<TPower>(
            new ThrowingPlayerChoiceContext(),
            targets,
            amount,
            Creature,
            null);

    private async Task PerformLongZhaoJi(IReadOnlyList<Creature> targets)
    {
        _threeQiPreparation = false;
        await DamageCmd.Attack(LongZhaoJiDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
        await PowerCmd.Apply<FrailPower>(
            new ThrowingPlayerChoiceContext(),
            targets,
            LongZhaoJiFrail,
            Creature,
            null);
    }

    private async Task PerformSanQiGuiLai(IReadOnlyList<Creature> targets)
    {
        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.transition.speakLine4"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Short);
        await Cmd.Wait(1f, ignoreCombatEnd: true);

        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.transition.speakLine5"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Long);
        await Cmd.Wait(3f, ignoreCombatEnd: true);

        _secondPhase = true;
        _threeQiPreparation = false;
        _skipLongYuThisTurn = true;

        await CreatureCmd.SetMaxHp(Creature, 800);
        await CreatureCmd.Heal(Creature, 800);
        _phaseTransitionPending = false;

        var context = new ThrowingPlayerChoiceContext();
        var longYu = Creature.GetPower<LongYuShangBinPower>();
        if (longYu is null)
        {
            await PowerCmd.Apply<LongYuShangBinPower>(
                context, Creature, 200, Creature, null);
        }
        else if (longYu.Amount != 200)
        {
            await PowerCmd.ModifyAmount(
                context, longYu, 200 - longYu.Amount, Creature, null);
        }

        var protection = Creature.GetPower<JiuLongWenHuShenPower>();
        if (protection is null)
        {
            await PowerCmd.Apply<JiuLongWenHuShenPower>(
                context, Creature, 9, Creature, null);
        }
        else if (protection.Amount != 9)
        {
            await PowerCmd.ModifyAmount(
                context, protection, 9 - protection.Amount, Creature, null);
        }

        if (Creature.GetPower<SanQiGuiLaiPower>() is null)
        {
            await PowerCmd.Apply<SanQiGuiLaiPower>(
                context, Creature, 1, Creature, null);
        }
    }

    private async Task PerformQiGaiShanHe(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(QiGaiShanHeDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(null);
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<VulnerablePower>(
            context, targets, 10, Creature, null);
        await PowerCmd.Apply<WeakPower>(
            context, targets, 10, Creature, null);
        await PowerCmd.Apply<FrailPower>(
            context, targets, 10, Creature, null);
    }

    private async Task PerformHuiXuanLongYa(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(HuiXuanLongYaDamage)
            .WithHitCount(2)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
        await PowerCmd.Apply<JiuLongWenHuShenPower>(
            new ThrowingPlayerChoiceContext(),
            Creature,
            4,
            Creature,
            null);
    }

    private Task PerformHanShiLongChui(IReadOnlyList<Creature> targets) =>
        DamageCmd.Attack(HanShiLongChuiDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(null);

    private Task PerformYiQiDaShouBao(IReadOnlyList<Creature> targets) =>
        DamageCmd.Attack(YiQiDaShouBaoDamage)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(null);

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Creature
            || !_threeQiPreparation
            || _secondPhase
            || result.UnblockedDamage <= 0
            || !Creature.IsAlive)
        {
            return;
        }

        _threeQiPreparation = false;
        await CreatureCmd.Stun(Creature, "LONG_ZHAO_JI");
    }

    public override bool ShouldDie(Creature creature)
    {
        if (creature != Creature || _secondPhase)
        {
            return true;
        }

        _phaseTransitionPending = true;
        return false;
    }

    public override bool ShouldAllowHitting(Creature creature) =>
        creature != Creature || !_phaseTransitionPending;

    public override bool ShouldStopCombatFromEnding() =>
        _phaseTransitionPending;

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Enemy
            && _phaseTransitionPending
            && !_secondPhase
            && participants.Contains(Creature))
        {
            SetMoveById("SAN_QI_GUI_LAI");
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPreventingDeath(Creature creature)
    {
        if (creature != Creature || _secondPhase || !_phaseTransitionPending)
        {
            return;
        }

        await CreatureCmd.Heal(Creature, 1, playAnim: false);
        _skipLongYuThisTurn = true;
        SetMoveById("SAN_QI_GUI_LAI");

        if (!_phaseTransitionDialoguePlayed)
        {
            _phaseTransitionDialoguePlayed = true;
            _ = TaskHelper.RunSafely(PlayPhaseTransitionDialogue());
        }
    }

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        if (creature != Creature || wasRemovalPrevented)
        {
            return;
        }

        if (!_secondPhase)
        {
            return;
        }

        var remainingEnemies = CombatState.Enemies
            .Where(enemy => enemy.IsAlive && enemy != Creature)
            .ToList();
        if (remainingEnemies.Count > 0)
        {
            await CreatureCmd.Kill(remainingEnemies, force: true);
        }
    }

    private async Task PlayPhaseTransitionDialogue()
    {
        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.transition.speakLine1"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Short);
        await Cmd.Wait(1f, ignoreCombatEnd: true);

        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.transition.speakLine2"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Short);
        await Cmd.Wait(1f, ignoreCombatEnd: true);

        TalkCmd.Play(
            MonsterModel.L10NMonsterLookup(
                "GU_ZHEN_REN_MONSTER_LONG_GONG.transition.speakLine3"),
            Creature,
            VfxColor.Purple,
            VfxDuration.Long);
        await Cmd.Wait(2f, ignoreCombatEnd: true);
    }

    private void SetMoveById(string moveId)
    {
        var monster = Creature.Monster;
        if (monster is not null
            && monster.MoveStateMachine?.States.TryGetValue(
                moveId,
                out var state) == true
            && state is MoveState move)
        {
            monster.SetMoveImmediate(move, forceTransition: true);
        }
    }

    public bool ConsumeLongYuSkip()
    {
        if (!_skipLongYuThisTurn)
        {
            return false;
        }

        _skipLongYuThisTurn = false;
        return true;
    }
}
