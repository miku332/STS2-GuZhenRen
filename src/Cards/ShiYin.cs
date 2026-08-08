using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ShiYin : GuZhenRenCardTemplate
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ShiYin.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [] : [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<IntangiblePower>(1)
    ];

    protected override bool IsPlayable =>
        base.IsPlayable
        && IsOddPlayerTurn();

    public ShiYin()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner.Creature.GetPower<IntangiblePower>() is null)
        {
            await PowerCmd.Apply<IntangiblePower>(
                choiceContext,
                Owner.Creature,
                DynamicVars["IntangiblePower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
    }

    private bool IsOddPlayerTurn()
    {
        if (Owner?.PlayerCombatState is null)
        {
            return true;
        }

        return Owner.PlayerCombatState.TurnNumber % 2 == 1;
    }
}
