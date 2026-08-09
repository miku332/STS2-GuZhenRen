using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class BianTong : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 8 : 7;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/BianTong.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.BianHuaDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [CardKeyword.Retain] : [];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BianHuaDaoDaoHenPower>(1)
    ];

    public BianTong()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var debuffs = Owner.Creature.Powers
            .Where(power => power.TypeForCurrentAmount == PowerType.Debuff)
            .ToList();

        foreach (var debuff in debuffs)
        {
            await PowerCmd.Remove(debuff);
            await ZhuanYiPower.TriggerConversion(
                Owner.Creature,
                Owner.Creature,
                this);
        }

        if (debuffs.Count == 0)
        {
            return;
        }

        await PowerCmd.Apply<BianHuaDaoDaoHenPower>(
            choiceContext,
            Owner.Creature,
            debuffs.Count * DynamicVars["BianHuaDaoDaoHenPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
