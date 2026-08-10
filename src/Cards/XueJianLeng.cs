using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XueJianLeng : AbstractShaZhaoCard
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XueJianLeng.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<LengXuePower>(5).WithPowerTooltip()
    ];

    public XueJianLeng()
        : base(1, CardType.Skill, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<LengXuePower>(
                choiceContext,
                enemy,
                DynamicVars["LengXuePower"].BaseValue,
                Owner.Creature,
                this);
        }
    }
}
