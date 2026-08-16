using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Patches;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class AiQingGu : GuZhenRenCardTemplate
{
    private bool _resolvingDrawEffect;

    public override int Rank => 9;

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/AiQingGu.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.AiQingGuPositiveEffect,
        GuZhenRenKeywords.AiQingGuNegativeEffect,
        CardKeyword.Unplayable,
        CardKeyword.Exhaust
    ];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public AiQingGu()
        : base(-2, CardType.Skill, CardRarity.Rare, TargetType.None, false)
    {
    }

    public void OnCardDrawn()
    {
        if (CombatState is null || _resolvingDrawEffect)
        {
            return;
        }

        _resolvingDrawEffect = true;
        var choiceContext = new HookPlayerChoiceContext(
            this,
            Owner.NetId,
            CombatState,
            GameActionType.Combat);
        var effectTask = ResolveDrawEffect(choiceContext);
        TaskHelper.RunSafely(
            choiceContext.AssignTaskAndWaitForPauseOrCompletion(effectTask));
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) => Task.CompletedTask;

    private async Task ResolveDrawEffect(PlayerChoiceContext choiceContext)
    {
        try
        {
            if (Owner.Creature.IsDead || Pile?.Type != PileType.Hand)
            {
                return;
            }

            var escapeAfterEffects = await ResolvePositiveEffect(choiceContext);
            await ResolveNegativeEffect(choiceContext);
            await CardCmd.Exhaust(choiceContext, this);

            if (escapeAfterEffects && Owner.Creature.IsAlive)
            {
                await EscapeCombat();
            }
        }
        finally
        {
            _resolvingDrawEffect = false;
        }
    }

    private async Task<bool> ResolvePositiveEffect(
        PlayerChoiceContext choiceContext)
    {
        var roll = Owner.RunState.Rng.CombatCardSelection.NextFloat(100f);
        if (roll < 60f)
        {
            await AddRandomShaZhao();
            return false;
        }

        if (roll < 75f)
        {
            await ObtainRandomRelic();
            return false;
        }

        if (roll < 90f)
        {
            await CreatureCmd.Heal(Owner.Creature, 15m);
            return false;
        }

        return true;
    }

    private async Task ResolveNegativeEffect(
        PlayerChoiceContext choiceContext)
    {
        var roll = Owner.RunState.Rng.CombatCardSelection.NextFloat(100f);
        if (roll < 25f)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                6m,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Owner.Creature,
                this);
            return;
        }

        if (roll < 50f)
        {
            await CreatureCmd.LoseMaxHp(
                choiceContext,
                Owner.Creature,
                3m,
                true);
            return;
        }

        if (roll < 75f)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                Owner.Creature,
                -2,
                Owner.Creature,
                this);
            return;
        }

        await PowerCmd.Apply<DexterityPower>(
            choiceContext,
            Owner.Creature,
            -2,
            Owner.Creature,
            this);
    }

    private async Task AddRandomShaZhao()
    {
        var candidates = new CardModel[]
        {
            ModelDb.Card<AngryBird>(),
            ModelDb.Card<AnQiSha>(),
            ModelDb.Card<GuangYinFeiRen>(),
            ModelDb.Card<JianHenSuoMing>(),
            ModelDb.Card<JianLangSanDie>(),
            ModelDb.Card<SanShiSanTianGuang>(),
            ModelDb.Card<ShangFangJieWa>(),
            ModelDb.Card<SongYouFeng>(),
            ModelDb.Card<TianPuGuangHe>(),
            ModelDb.Card<WanWo>(),
            ModelDb.Card<WanWoDaShouYin>(),
            ModelDb.Card<WanWuDaTongBian>(),
            ModelDb.Card<WanXingFeiYing>(),
            ModelDb.Card<WuJinXuanGuangQi>(),
            ModelDb.Card<WuZhiQuanXinJian>(),
            ModelDb.Card<XueJianLeng>(),
            ModelDb.Card<XuePiaoLiu>(),
            ModelDb.Card<YangMangBeiHuoYi>(),
            ModelDb.Card<ZhuiMingHuo>()
        };
        var canonical = Owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (canonical is null || CombatState is null)
        {
            return;
        }

        var copy = CombatState.CreateCard(canonical, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            copy,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    private async Task ObtainRandomRelic()
    {
        Owner.PopulateRelicGrabBagIfNecessary(Owner.RunState.Rng.UpFront);

        var roll = Owner.RunState.Rng.CombatCardSelection.NextFloat();
        var rarity = roll < 0.5f
            ? RelicRarity.Common
            : roll < 0.85f
                ? RelicRarity.Uncommon
                : RelicRarity.Rare;
        var relic = Owner.RelicGrabBag.PullFromFront(
            rarity,
            IsAllowedRandomRelic,
            Owner.RunState);
        if (relic is not null)
        {
            await RelicCmd.Obtain(relic.ToMutable(), Owner);
        }
    }

    private async Task EscapeCombat()
    {
        if (CombatState is null
            || Owner.RunState.CurrentRoom is not CombatRoom room)
        {
            return;
        }

        AiQingGuEscapeRewardPatch.SkipRewardsFor(room);

        foreach (var enemy in CombatState.Enemies
                     .Where(enemy => enemy.IsAlive)
                     .ToList())
        {
            await CreatureCmd.Escape(enemy);
        }

        await CombatManager.Instance.CheckWinCondition();
    }

    private bool IsAllowedRandomRelic(RelicModel relic)
    {
        var entry = relic.Id.Entry;
        return relic.IsAllowed(Owner.RunState)
            && !entry.Contains("Bottled", StringComparison.OrdinalIgnoreCase)
            && !entry.Contains("ChunQiuChan", StringComparison.OrdinalIgnoreCase);
    }
}
