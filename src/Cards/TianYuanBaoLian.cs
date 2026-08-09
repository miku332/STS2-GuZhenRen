using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class TianYuanBaoLian : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 4 : 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/TianYuanBaoLian.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.MuDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Energy", 2)
    ];

    public TianYuanBaoLian()
        : base(0, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var energy = (int)DynamicVars["Energy"].BaseValue;
        if (energy > 0)
        {
            await PlayerCmd.GainEnergy(energy, Owner);
            DynamicVars["Energy"].BaseValue = Math.Max(0, energy - 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(1);
    }
}
