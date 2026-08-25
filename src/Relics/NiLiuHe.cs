using GuZhenRen.CardPools;
using GuZhenRen.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class NiLiuHe : ModRelicTemplate
{
    private sealed record PendingReflection(
        Creature Target,
        Creature Attacker,
        decimal Damage,
        CardModel? CardSource);

    public const int MaxWater = 9;

    private int _counter = 3;
    private readonly Queue<PendingReflection> _pendingReflections = [];

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Water", 3),
        new DynamicVar("MaxWater", MaxWater)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(value, 0, MaxWater);
            DynamicVars["Water"].BaseValue = _counter;
            Status = _counter > 0 ? RelicStatus.Normal : RelicStatus.Disabled;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/NiLiuHe.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/NiLiuHe.png",
        BigIconPath: "res://GuZhenRen/images/relics/NiLiuHe.png");

    public override Task AfterObtained()
    {
        Counter = Counter;
        return Task.CompletedTask;
    }

    public void ResetCombatState() => _pendingReflections.Clear();

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        AddWater(1);
        return Task.CompletedTask;
    }

    public bool AddWater(int amount)
    {
        if (amount <= 0 || Counter >= MaxWater)
        {
            return false;
        }

        Counter += amount;
        Flash();
        return true;
    }

    public bool TryConsumeWater()
    {
        if (Counter <= 0)
        {
            return false;
        }

        Counter--;
        Flash();
        return true;
    }

    public override decimal ModifyHpLostBeforeOsty(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature
            || dealer is null
            || !dealer.IsMonster
            || !props.IsPoweredAttack())
        {
            return amount;
        }

        NiLiuHeReflectionState.BeginAttack(target);
        if (amount <= 0 || !TryConsumeWater())
        {
            return amount;
        }

        _pendingReflections.Enqueue(new PendingReflection(
            target,
            dealer,
            amount,
            cardSource));
        NiLiuHeReflectionState.MarkAttackReflected(target, dealer);
        return 0m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (_pendingReflections.Count == 0)
        {
            return;
        }

        var pending = _pendingReflections.Peek();
        if (pending.Target != target || pending.Attacker != dealer)
        {
            return;
        }

        _pendingReflections.Dequeue();
        await CreatureCmd.Damage(
            choiceContext,
            pending.Attacker,
            pending.Damage,
            ValueProp.Unpowered,
            pending.Attacker,
            pending.CardSource);
    }
}
