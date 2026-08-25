using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Multiplayer;
using GuZhenRen.Patches;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class AiQingGu : GuZhenRenCardTemplate
{
    private bool _resolvingDrawEffect;

    public override int Rank => 9;

    protected override bool ShowXianGuHoverTip => false;

    protected override bool IsPlayable => false;

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/AiQingGu.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.AiQingGuPositiveEffect,
        GuZhenRenKeywords.AiQingGuNegativeEffect
    ];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public AiQingGu()
        : base(-2, CardType.Skill, CardRarity.Rare, TargetType.None, false)
    {
    }

    public void OnCardDrawn()
    {
        if (CombatState is null
            || _resolvingDrawEffect)
        {
            return;
        }

        _resolvingDrawEffect = true;
        TaskHelper.RunSafely(PrepareAndRequestResolution());
    }

    private async Task PrepareAndRequestResolution()
    {
        try
        {
            for (var i = 0; i < 20
                 && !Owner.Creature.IsDead
                 && CombatState is not null
                 && Pile?.Type != PileType.Hand;
                 i++)
            {
                await Cmd.Wait(0.05f, ignoreCombatEnd: true);
            }

            if (Owner.Creature.IsDead
                || CombatState is null
                || Pile?.Type != PileType.Hand)
            {
                return;
            }

            var payload = CreatePayload();
            if (Owner.NetId == RunManager.Instance.NetService.NetId)
            {
                NetGuZhenRenActions.RequestWithRetry(
                    () => NetGuZhenRenActions.RequestAiQingGu(payload),
                    () => !Owner.Creature.IsDead
                        && CombatState is not null
                        && Pile?.Type == PileType.Hand,
                    static () => { },
                    "AiQingGu");
            }
        }
        finally
        {
            _resolvingDrawEffect = false;
        }
    }

    private AiQingGuPayload CreatePayload()
    {
        var positiveRoll = Owner.RunState.Rng.CombatCardSelection.NextFloat(100f);
        var positiveOutcome = positiveRoll < 60f
            ? AiQingPositiveOutcome.Card
            : positiveRoll < 75f
                ? AiQingPositiveOutcome.Relic
                : positiveRoll < 90f
                    ? AiQingPositiveOutcome.Heal
                    : AiQingPositiveOutcome.Escape;

        string? positiveModelId = null;
        var positiveRelicRarity = RelicRarity.None;
        if (positiveOutcome == AiQingPositiveOutcome.Card)
        {
            var canonical = GetRandomShaZhao(Owner);
            positiveModelId = canonical?.Id.ToString();
        }
        else if (positiveOutcome == AiQingPositiveOutcome.Relic)
        {
            Owner.PopulateRelicGrabBagIfNecessary(Owner.RunState.Rng.UpFront);
            var rarityRoll = Owner.RunState.Rng.CombatCardSelection.NextFloat();
            positiveRelicRarity = rarityRoll < 0.5f
                ? RelicRarity.Common
                : rarityRoll < 0.85f
                    ? RelicRarity.Uncommon
                    : RelicRarity.Rare;
            var relic = Owner.RelicGrabBag.PullFromFront(
                positiveRelicRarity,
                relic => IsAllowedRandomRelic(relic, Owner.RunState),
                Owner.RunState);
            positiveModelId = relic?.Id.ToString();
        }

        var negativeRoll = Owner.RunState.Rng.CombatCardSelection.NextFloat(100f);
        var negativeOutcome = negativeRoll < 25f
            ? AiQingNegativeOutcome.Damage
            : negativeRoll < 50f
                ? AiQingNegativeOutcome.LoseMaxHp
                : negativeRoll < 75f
                    ? AiQingNegativeOutcome.Strength
                    : AiQingNegativeOutcome.Dexterity;

        return new AiQingGuPayload(
            Owner.NetId,
            NetCombatCard.FromModel(this).CombatCardIndex,
            positiveOutcome,
            positiveModelId,
            positiveRelicRarity,
            negativeOutcome);
    }

    internal static async Task ExecuteManagedDrawAsync(
        RitsuLibManagedNetActionContext<AiQingGuPayload> context)
    {
        var owner = context.Player.RunState.Players
            .FirstOrDefault(player => player.NetId == context.Message.OwnerNetId);
        var card = NetCombatCard.ForTesting(context.Message.CardIndex).ToCardModelOrNull();
        if (owner is null
            || card is null
            || owner.Creature.IsDead
            || card.Pile?.Type != PileType.Hand
            || card.CombatState is null)
        {
            return;
        }

        var escapeAfterEffects = await ApplyPositiveEffect(
            context.PlayerChoiceContext,
            owner,
            context.Message);
        await ApplyNegativeEffect(
            context.PlayerChoiceContext,
            owner,
            context.Message.NegativeOutcome);
        await CardCmd.Exhaust(context.PlayerChoiceContext, card);

        if (escapeAfterEffects && owner.Creature.IsAlive)
        {
            await EscapeCombat(owner);
        }
    }

    private static async Task<bool> ApplyPositiveEffect(
        PlayerChoiceContext choiceContext,
        Player owner,
        AiQingGuPayload payload)
    {
        switch (payload.PositiveOutcome)
        {
            case AiQingPositiveOutcome.Card:
                await AddRandomShaZhao(choiceContext, owner, payload.PositiveModelId);
                return false;
            case AiQingPositiveOutcome.Relic:
                await ObtainRandomRelic(owner, payload);
                return false;
            case AiQingPositiveOutcome.Heal:
                await CreatureCmd.Heal(owner.Creature, 15m);
                return false;
            default:
                return true;
        }
    }

    private static async Task ApplyNegativeEffect(
        PlayerChoiceContext choiceContext,
        Player owner,
        AiQingNegativeOutcome outcome)
    {
        switch (outcome)
        {
            case AiQingNegativeOutcome.Damage:
                await CreatureCmd.Damage(
                    choiceContext,
                    owner.Creature,
                    6m,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    owner.Creature,
                    null,
                    null);
                break;
            case AiQingNegativeOutcome.LoseMaxHp:
                await CreatureCmd.LoseMaxHp(
                    choiceContext,
                    owner.Creature,
                    3m,
                    true);
                break;
            case AiQingNegativeOutcome.Strength:
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    owner.Creature,
                    -2,
                    owner.Creature,
                    null);
                break;
            case AiQingNegativeOutcome.Dexterity:
                await PowerCmd.Apply<DexterityPower>(
                    choiceContext,
                    owner.Creature,
                    -2,
                    owner.Creature,
                    null);
                break;
        }
    }

    private static CardModel? GetRandomShaZhao(Player owner)
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
        return owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
    }

    private static async Task AddRandomShaZhao(
        PlayerChoiceContext choiceContext,
        Player owner,
        string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId)
            || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        var canonical = ModelDb.GetByIdOrNull<CardModel>(
            ModelId.Deserialize(modelId));
        if (canonical is null || owner.Creature.CombatState is null)
        {
            return;
        }

        var copy = owner.Creature.CombatState.CreateCard(canonical, owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            copy,
            PileType.Hand,
            owner,
            CardPilePosition.Bottom);
    }

    private static async Task ObtainRandomRelic(
        Player owner,
        AiQingGuPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.PositiveModelId))
        {
            return;
        }

        var canonical = ModelDb.GetByIdOrNull<RelicModel>(
            ModelId.Deserialize(payload.PositiveModelId));
        if (canonical is null)
        {
            return;
        }

        await RelicCmd.Obtain(canonical.ToMutable(), owner);
    }

    private static async Task EscapeCombat(Player owner)
    {
        if (owner.RunState.CurrentRoom is not CombatRoom room
            || owner.Creature.CombatState is null)
        {
            return;
        }

        AiQingGuEscapeRewardPatch.SkipRewardsFor(room);

        foreach (var enemy in owner.Creature.CombatState.Enemies
                     .Where(enemy => enemy.IsAlive)
                     .ToList())
        {
            await CreatureCmd.Escape(enemy);
        }

        await CombatManager.Instance.CheckWinCondition();
    }

    private static bool IsAllowedRandomRelic(RelicModel relic, IRunState runState)
    {
        var entry = relic.Id.Entry;
        return relic.IsAllowed(runState)
            && !entry.Contains("Bottled", StringComparison.OrdinalIgnoreCase)
            && !entry.Contains("ChunQiuChan", StringComparison.OrdinalIgnoreCase);
    }
}
