using GuZhenRen.CardPools;
using GuZhenRen.Relics;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ChiLi : GuZhenRenCardTemplate
{
    private const int InitialKillsRemaining = 2;

    private int _killsRemaining = InitialKillsRemaining;

    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ChiLi.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ShiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    [SavedProperty]
    public int KillsRemaining
    {
        get => _killsRemaining;
        set
        {
            AssertMutable();
            _killsRemaining = Math.Clamp(value, 1, InitialKillsRemaining);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new PowerVar<StrengthPower>(0).WithPowerTooltip(),
        new DynamicVar("KillsRemaining", KillsRemaining)
    ];

    public ChiLi()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash");
        await attack.Execute(choiceContext);

        var killedTarget = attack.Results
            .SelectMany(static resultSet => resultSet)
            .Any(result => result.Receiver == cardPlay.Target
                && result.WasTargetKilled);
        if (!killedTarget
            || cardPlay.Target.IsSecondaryEnemy
            || cardPlay.Target.GetPower<MinionPower>() is not null)
        {
            return;
        }

        var remaining = KillsRemaining - 1;
        if (remaining == 0)
        {
            remaining = InitialKillsRemaining;
            await GainPermanentLiDaoDaoHen(choiceContext);
        }

        SyncKillsRemaining(remaining);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    private void SyncKillsRemaining(int remaining)
    {
        var deckCard = DeckVersion as ChiLi;
        if (deckCard is null && Pile?.Type == PileType.Deck)
        {
            deckCard = this;
        }

        if (deckCard is null)
        {
            SetKillsRemaining(this, remaining);
            return;
        }

        SetKillsRemaining(deckCard, remaining);
        foreach (var pileType in new[]
                 {
                     PileType.Hand,
                     PileType.Draw,
                     PileType.Discard,
                     PileType.Exhaust,
                     PileType.Play
                 })
        {
            foreach (var combatCard in pileType
                         .GetPile(Owner)
                         .Cards
                         .OfType<ChiLi>())
            {
                if (ReferenceEquals(combatCard, this)
                    || ReferenceEquals(combatCard.DeckVersion, deckCard))
                {
                    SetKillsRemaining(combatCard, remaining);
                }
            }
        }
    }

    private static void SetKillsRemaining(ChiLi card, int remaining)
    {
        card.KillsRemaining = remaining;
        card.DynamicVars["KillsRemaining"].BaseValue = remaining;
    }

    private async Task GainPermanentLiDaoDaoHen(
        PlayerChoiceContext choiceContext)
    {
        var relic = Owner.GetRelic<LiDaoDaoHen>();
        if (relic is null)
        {
            await RelicCmd.Obtain<LiDaoDaoHen>(Owner);
        }
        else
        {
            relic.Counter++;
            relic.Flash();
        }

        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }
}
