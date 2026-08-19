using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class RenRuGu : GuZhenRenCardTemplate
{
    private static readonly Dictionary<ulong, List<int>> TurnStartHpByPlayer = [];

    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/RenRuGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhouDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [CardKeyword.Exhaust, CardKeyword.Retain] : [CardKeyword.Exhaust];

    public RenRuGu()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    public static void ResetCombatHistory()
    {
        TurnStartHpByPlayer.Clear();
    }

    public static void RecordPlayerTurnStart(ICombatState combatState)
    {
        foreach (var player in combatState.Players)
        {
            if (!TurnStartHpByPlayer.TryGetValue(player.NetId, out var history))
            {
                history = [];
                TurnStartHpByPlayer[player.NetId] = history;
            }

            history.Add(player.Creature.CurrentHp);
        }
    }

    private static int GetPreviousTurnStartHp(Player player)
    {
        if (!TurnStartHpByPlayer.TryGetValue(player.NetId, out var history)
            || history.Count == 0)
        {
            return player.Creature.CurrentHp;
        }

        return history.Count == 1
            ? history[0]
            : history[^2];
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.SetCurrentHp(
            Owner.Creature,
            GetPreviousTurnStartHp(Owner));
    }

    protected override void OnUpgrade()
    {
        if (IsUpgraded)
        {
            AddKeyword(CardKeyword.Retain);
        }
        else
        {
            RemoveKeyword(CardKeyword.Retain);
        }
    }
}
