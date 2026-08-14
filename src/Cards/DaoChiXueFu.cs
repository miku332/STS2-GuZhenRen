using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class DaoChiXueFu : GuZhenRenCardTemplate
{
    public override int Rank => 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/DaoChiXueFu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("Hits")
            .WithMultiplier(static (card, _) =>
                ((DaoChiXueFu)card).CountCombatCopies())
    ];

    public DaoChiXueFu()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hits = (int)((CalculatedVar)DynamicVars["Hits"])
            .Calculate(cardPlay.Target);

        for (var i = 0; i < hits; i++)
        {
            if (!cardPlay.Target.IsAlive)
            {
                break;
            }

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    private int CountCombatCopies()
    {
        var cards = new HashSet<CardModel>();

        AddCopiesFromPile(cards, PileType.Hand);
        AddCopiesFromPile(cards, PileType.Draw);
        AddCopiesFromPile(cards, PileType.Discard);
        AddCopiesFromPile(cards, PileType.Exhaust);

        cards.Add(this);
        return Math.Max(1, cards.Count);
    }

    private void AddCopiesFromPile(HashSet<CardModel> cards, PileType pileType)
    {
        foreach (var card in pileType.GetPile(Owner).Cards)
        {
            if (card is DaoChiXueFu)
            {
                cards.Add(card);
            }
        }
    }
}
