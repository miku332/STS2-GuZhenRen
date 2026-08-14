using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ShaGu : AbstractBenMingGuCard
{
    private const int InitialDamage = 8;

    private int _permanentDamageBonus;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ShaGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ShaDao];

    [SavedProperty]
    public int PermanentDamageBonus
    {
        get => _permanentDamageBonus;
        set
        {
            AssertMutable();
            _permanentDamageBonus = Math.Max(0, value);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            InitialDamage,
            static (card, _) => card is ShaGu shaGu
                ? GetBaseDamage(shaGu.Rank) + shaGu.PermanentDamageBonus
                : InitialDamage,
            ValueProp.Move),
        new DynamicVar("HpLoss", 1),
        new DynamicVar("DamageGrowth", 1)
    ];

    public ShaGu()
        : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars["HpLoss"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            this,
            cardPlay);

        await DamageCmd.Attack(DynamicVars.GetComputedValue("Damage"))
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        var growth = DynamicVars["DamageGrowth"];
        growth.UpgradeValueBy(GetDamageGrowth(Rank) - growth.BaseValue);
    }

    public static void AfterCreatureDied(CreatureDiedEvent evt)
    {
        if (evt.WasRemovalPrevented
            || evt.Creature.IsSecondaryEnemy
            || evt.CombatState is null
            || !evt.CombatState.Enemies.Contains(evt.Creature))
        {
            return;
        }

        foreach (var player in evt.CombatState.Players)
        {
            foreach (var deckCard in player.Deck.Cards.OfType<ShaGu>().ToList())
            {
                GrowLinkedCards(
                    deckCard,
                    deckCard.DynamicVars["DamageGrowth"].IntValue);
            }
        }
    }

    private static void GrowLinkedCards(ShaGu deckCard, int amount)
    {
        var newBonus = deckCard.PermanentDamageBonus + amount;
        deckCard.PermanentDamageBonus = newBonus;

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
                         .GetPile(deckCard.Owner)
                         .Cards
                         .OfType<ShaGu>())
            {
                if (ReferenceEquals(combatCard.DeckVersion, deckCard))
                {
                    combatCard.PermanentDamageBonus = newBonus;
                }
            }
        }
    }

    private static int GetBaseDamage(int rank) =>
        rank switch
        {
            1 => 8,
            2 => 9,
            3 => 10,
            4 => 11,
            5 or 6 => 12,
            7 => 13,
            8 => 14,
            _ => 15
        };

    private static int GetDamageGrowth(int rank) =>
        rank switch
        {
            <= 5 => 1,
            <= 8 => 2,
            _ => 3
        };
}
