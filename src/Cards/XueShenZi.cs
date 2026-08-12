using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XueShenZi : GuZhenRenCardTemplate
{
    private const int InitialDamage = 6;
    private const int HpLoss = 6;
    private const int DamageGrowth = 6;

    private int _storedDamage = InitialDamage;

    public override int Rank => IsUpgraded ? 6 : 5;

    public override string Title =>
        IsUpgraded ? "血神子仙蛊" : base.Title;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XueShenZi.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [] : [CardKeyword.Exhaust];

    [SavedProperty]
    public int StoredDamage
    {
        get => _storedDamage;
        set
        {
            AssertMutable();
            _storedDamage = Math.Max(InitialDamage, value);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.ComputedDamage(
            "Damage",
            InitialDamage,
            static (card, _) => card is XueShenZi xueShenZi
                ? xueShenZi.StoredDamage
                : InitialDamage,
            ValueProp.Move),
        new DynamicVar("HpLoss", HpLoss),
        new DynamicVar("DamageGrowth", DamageGrowth)
    ];

    public XueShenZi()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, true)
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
            this);

        await DamageCmd.Attack(DynamicVars.GetComputedValue("Damage"))
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        GrowLinkedCards((int)DynamicVars["DamageGrowth"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }

    private void GrowLinkedCards(int amount)
    {
        var linkedCards = new HashSet<CardModel>();
        var deckVersion = DeckVersion ?? this;

        if (deckVersion is XueShenZi)
        {
            linkedCards.Add(deckVersion);
        }

        AddLinkedCardsFromPile(linkedCards, PileType.Hand, deckVersion);
        AddLinkedCardsFromPile(linkedCards, PileType.Draw, deckVersion);
        AddLinkedCardsFromPile(linkedCards, PileType.Discard, deckVersion);
        AddLinkedCardsFromPile(linkedCards, PileType.Exhaust, deckVersion);
        AddLinkedCardsFromPile(linkedCards, PileType.Play, deckVersion);
        linkedCards.Add(this);

        foreach (var card in linkedCards.OfType<XueShenZi>())
        {
            card.StoredDamage += amount;
        }
    }

    private void AddLinkedCardsFromPile(
        HashSet<CardModel> linkedCards,
        PileType pileType,
        CardModel deckVersion)
    {
        foreach (var card in pileType.GetPile(Owner).Cards)
        {
            if (card is XueShenZi &&
                (ReferenceEquals(card, this) ||
                 ReferenceEquals(card.DeckVersion, deckVersion) ||
                 ReferenceEquals(card, deckVersion)))
            {
                linkedCards.Add(card);
            }
        }
    }
}
