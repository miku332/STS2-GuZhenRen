using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZhiZhang : GuZhenRenCardTemplate
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZhiZhang.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.Nian];

    public ZhiZhang()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var nian = Owner.Creature.GetPower<NianPower>();
        if (nian is not null)
        {
            await PowerCmd.Remove(nian);
        }

        await PowerCmd.Apply<ZhiZhangPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
