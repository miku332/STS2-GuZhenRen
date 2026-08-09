using GuZhenRen.CardPools;
using GuZhenRen.Patches;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class AiQingGu : GuZhenRenCardTemplate
{
    public override int Rank => 9;

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/AiQingGu.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
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
        if (CombatState is null)
        {
            return;
        }

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new AiQingGuDrawAction(Owner, this));
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) => Task.CompletedTask;

    private sealed class AiQingGuDrawAction : GameAction
    {
        private readonly Player _owner;
        private readonly AiQingGu _card;

        public AiQingGuDrawAction(Player owner, AiQingGu card)
        {
            _owner = owner;
            _card = card;
        }

        public override ulong OwnerId => _owner.NetId;

        public override GameActionType ActionType => GameActionType.Combat;

        public override bool RecordableToReplay => false;

        protected override async Task ExecuteAction()
        {
            if (_owner.Creature.IsDead || _card.Pile?.Type != PileType.Hand)
            {
                return;
            }

            var choiceContext = new GameActionPlayerChoiceContext(this);
            var escapeAfterEffects = await ResolvePositiveEffect(choiceContext);
            await ResolveNegativeEffect(choiceContext);
            await CardCmd.Exhaust(choiceContext, _card);

            if (escapeAfterEffects && _owner.Creature.IsAlive)
            {
                await EscapeCombat();
            }
        }

        private async Task<bool> ResolvePositiveEffect(
            PlayerChoiceContext choiceContext)
        {
            var roll = _owner.RunState.Rng.CombatCardSelection.NextFloat(100f);
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
                await CreatureCmd.Heal(_owner.Creature, 15m);
                return false;
            }

            return true;
        }

        private async Task ResolveNegativeEffect(
            PlayerChoiceContext choiceContext)
        {
            var roll = _owner.RunState.Rng.CombatCardSelection.NextFloat(100f);
            if (roll < 25f)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    _owner.Creature,
                    6m,
                    ValueProp.Unblockable | ValueProp.Unpowered,
                    _owner.Creature,
                    _card);
                return;
            }

            if (roll < 50f)
            {
                await CreatureCmd.LoseMaxHp(
                    choiceContext,
                    _owner.Creature,
                    3m,
                    true);
                return;
            }

            if (roll < 75f)
            {
                await PowerCmd.Apply<StrengthPower>(
                    choiceContext,
                    _owner.Creature,
                    -2,
                    _owner.Creature,
                    _card);
                return;
            }

            await PowerCmd.Apply<DexterityPower>(
                choiceContext,
                _owner.Creature,
                -2,
                _owner.Creature,
                _card);
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
            var canonical = _owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (canonical is null || _card.CombatState is null)
            {
                return;
            }

            var copy = _card.CombatState.CreateCard(canonical, _owner);
            await CardPileCmd.AddGeneratedCardToCombat(
                copy,
                PileType.Hand,
                _owner,
                CardPilePosition.Bottom);
        }

        private async Task ObtainRandomRelic()
        {
            _owner.PopulateRelicGrabBagIfNecessary(_owner.RunState.Rng.UpFront);

            var roll = _owner.RunState.Rng.CombatCardSelection.NextFloat();
            var rarity = roll < 0.5f
                ? RelicRarity.Common
                : roll < 0.85f
                    ? RelicRarity.Uncommon
                    : RelicRarity.Rare;
            var relic = _owner.RelicGrabBag.PullFromFront(
                rarity,
                IsAllowedRandomRelic,
                _owner.RunState);
            if (relic is not null)
            {
                await RelicCmd.Obtain(relic.ToMutable(), _owner);
            }
        }

        private async Task EscapeCombat()
        {
            if (_card.CombatState is null
                || _owner.RunState.CurrentRoom is not CombatRoom room)
            {
                return;
            }

            AiQingGuEscapeRewardPatch.SkipRewardsFor(room);

            foreach (var enemy in _card.CombatState.Enemies
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
            return relic.IsAllowed(_owner.RunState)
                && !entry.Contains("Bottled", StringComparison.OrdinalIgnoreCase)
                && !entry.Contains("ChunQiuChan", StringComparison.OrdinalIgnoreCase);
        }

        public override INetAction ToNetAction()
        {
            throw new NotSupportedException(
                "GuZhenRen AiQingGu is single-player only for now.");
        }
    }
}
