using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class TaiChuGuangGu : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XiaoGuangGu.png",
        VisualStyle: CardVisualStyle.Ancient);

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShanYaoPower>(3).WithPowerTooltip(),
        new PowerVar<WeakPower>(2).WithPowerTooltip(),
        new PowerVar<TaiChuGuangPower>(1).WithPowerTooltip()
    ];

    public TaiChuGuangGu()
        : base(0, CardType.Skill, CardRarity.Ancient, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var owner = Owner.Creature;

        await ShanYaoPower.Apply(
            choiceContext,
            owner,
            DynamicVars["ShanYaoPower"].BaseValue,
            owner,
            this);

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            CombatState!.HittableEnemies,
            DynamicVars["WeakPower"].BaseValue,
            owner,
            this);

        await PowerCmd.Apply<TaiChuGuangPower>(
            choiceContext,
            owner,
            DynamicVars["TaiChuGuangPower"].BaseValue,
            owner,
            this);
    }
}
