using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class BaMianWeiFengGu : GuZhenRenCardTemplate
{
    private static readonly HashSet<CardTag> DaoTags =
    [
        GuZhenRenTags.FengDao,
        GuZhenRenTags.GuangDao,
        GuZhenRenTags.JianDao,
        GuZhenRenTags.LiDao,
        GuZhenRenTags.MuDao,
        GuZhenRenTags.ShaDao,
        GuZhenRenTags.TuDao,
        GuZhenRenTags.XueDao,
        GuZhenRenTags.YanDao
    ];

    public override int Rank => IsUpgraded ? 8 : 7;

    public override string Title => IsUpgraded
        ? new LocString("cards", $"{Id.Entry}.upgradeTitle").GetFormattedText()
        : base.Title;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/BaMianWeiFengGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.FengDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("Cards", 7)
    ];

    public BaMianWeiFengGu()
        : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var handCount = PileType.Hand.GetPile(Owner).Cards.Count;
        var cardsToDraw = Math.Max(
            0,
            (int)DynamicVars["Cards"].BaseValue - handCount);
        var drawnCards = (await CardPileCmd.Draw(
            choiceContext,
            cardsToDraw,
            Owner)).ToList();

        var uniqueDaoCount = CountUniqueDaos(drawnCards);
        for (var i = 0; i < uniqueDaoCount; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Cards"].UpgradeValueBy(1);
    }

    private int CountUniqueDaos(IEnumerable<MegaCrit.Sts2.Core.Models.CardModel> cards)
    {
        var uniqueDaos = new HashSet<CardTag>();
        var treatsAllDaosAsJianDao =
            Owner.Creature.GetPower<RuiYiPower>() is not null;

        foreach (var card in cards)
        {
            var cardDaos = card.Tags.Where(DaoTags.Contains).ToList();
            if (cardDaos.Count == 0)
            {
                continue;
            }

            if (treatsAllDaosAsJianDao)
            {
                uniqueDaos.Add(GuZhenRenTags.JianDao);
            }
            else
            {
                uniqueDaos.UnionWith(cardDaos);
            }
        }

        return uniqueDaos.Count;
    }
}
