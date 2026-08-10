using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class ShuiWenGu : ModRelicTemplate
{
    private const string ThresholdKey = "Threshold";
    private const string GoldKey = "Gold";
    private const int Threshold = 300;
    private const int Gold = 250;
    private int _counter;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ThresholdKey, Threshold),
        new DynamicVar(GoldKey, Gold)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(value, 0, Threshold);
            Status = _counter >= Threshold ? RelicStatus.Disabled : RelicStatus.Normal;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/ShuiWenGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/ShuiWenGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/ShuiWenGu.png");

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Counter >= Threshold || cardPlay.Card.Owner != Owner)
        {
            return;
        }

        Counter += CountTitleCharacters(cardPlay.Card.Title);
        if (Counter < Threshold)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainGold(DynamicVars[GoldKey].BaseValue, Owner);
        Counter = Threshold;
    }

    private static int CountTitleCharacters(string title)
    {
        var count = 0;
        foreach (var character in title)
        {
            if (!char.IsPunctuation(character) && !char.IsWhiteSpace(character) && !char.IsSymbol(character))
            {
                count++;
            }
        }

        return count;
    }
}
