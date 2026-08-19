using GuZhenRen.CardPools;
using GuZhenRen.Relics;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ChiXin : AbstractShaZhaoCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ChiXin.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ShiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("MaxHpGain", 2)
    ];

    public ChiXin()
        : base(2, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        if (!cardPlay.Target.IsAlive
            || cardPlay.Target.CurrentHp >= Owner.Creature.MaxHp)
        {
            return;
        }

        await CreatureCmd.Kill(cardPlay.Target);
        if (cardPlay.Target.IsAlive || !Owner.Creature.IsAlive)
        {
            return;
        }

        var maxHpGain = DynamicVars["MaxHpGain"].BaseValue;
        if (Owner.GetRelic<ChiXiang>() is { } chiXiang)
        {
            chiXiang.Flash();
            maxHpGain *= 2;
        }

        await CreatureCmd.GainMaxHp(Owner.Creature, maxHpGain);
    }
}
