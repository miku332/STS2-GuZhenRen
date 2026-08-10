using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class MuYa : ModRelicTemplate
{
    private const string ThresholdKey = "Threshold";
    private const string CounterKey = "Counter";
    private const int Threshold = 10;
    private int _counter;

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ThresholdKey, Threshold)
        ,new DynamicVar(CounterKey, 0)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Max(0, value);
            DynamicVars[CounterKey].BaseValue = _counter;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/MuYa.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/MuYa.png",
        BigIconPath: "res://GuZhenRen/images/relics/MuYa.png");

    public override Task BeforeCombatStart()
    {
        Status = Counter >= Threshold - 1 ? RelicStatus.Active : RelicStatus.Normal;
        return Task.CompletedTask;
    }

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card.Owner != Owner)
        {
            return;
        }

        Counter++;
        if (Counter < Threshold)
        {
            Status = Counter >= Threshold - 1 ? RelicStatus.Active : RelicStatus.Normal;
            return;
        }

        Counter -= Threshold;
        Status = Counter >= Threshold - 1 ? RelicStatus.Active : RelicStatus.Normal;
        Flash();

        var combatState = Owner.Creature.CombatState;
        if (combatState is not null)
        {
            var generated = combatState.CreateCard(CreateRandomMuDaoCard(), Owner);
            generated.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(
                generated,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }
    }

    private CardModel CreateRandomMuDaoCard()
    {
        var candidates = new CardModel[]
        {
            ModelDb.Card<TianYuanBaoLian>(),
            ModelDb.Card<MuJiaGu>(),
            ModelDb.Card<JiuYeShengJiCao>()
        };
        return Owner.RunState.Rng.CombatCardGeneration.NextItem(candidates)!;
    }
}
