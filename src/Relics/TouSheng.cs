using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class TouSheng : ModRelicTemplate, IModRightClickableRelic
{
    private const int MaxUses = 3;
    private const int StealAmount = 2;
    private int _counter = MaxUses;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("StealAmount", StealAmount),
        new DynamicVar("Uses", MaxUses)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(value, 0, MaxUses);
            DynamicVars["Uses"].BaseValue = _counter;
            Status = _counter > 0
                ? RelicStatus.Normal
                : RelicStatus.Disabled;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/TouSheng.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/TouSheng.png",
        BigIconPath: "res://GuZhenRen/images/relics/TouSheng.png");

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        var combatState = Owner.Creature.CombatState;
        return context.Player == Owner
            && Counter > 0
            && Owner.Creature.IsAlive
            && combatState is not null
            && combatState.CurrentSide == CombatSide.Player;
    }

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (!CanHandleRightClickLocal(new ModRightClickContext(
                context.Player,
                context.Model,
                context.Trigger)))
        {
            return;
        }

        var combatState = Owner.Creature.CombatState;
        if (combatState is null)
        {
            return;
        }

        var stolen = 0m;
        foreach (var enemy in combatState.Enemies
                     .Where(static enemy => enemy.IsAlive)
                     .ToList())
        {
            var oldMaxHp = enemy.MaxHp;
            await CreatureCmd.LoseMaxHp(
                new ThrowingPlayerChoiceContext(),
                enemy,
                Math.Min(StealAmount, oldMaxHp),
                false);
            stolen += oldMaxHp - enemy.MaxHp;
        }

        if (stolen <= 0)
        {
            return;
        }

        Counter--;
        Flash();
        await CreatureCmd.GainMaxHp(Owner.Creature, stolen);
    }
}
