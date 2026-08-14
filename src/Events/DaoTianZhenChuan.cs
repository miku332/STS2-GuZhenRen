using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Events;

[RegisterSharedEvent]
public sealed class DaoTianZhenChuan : ModEventTemplate
{
    private const int HpLoss = 8;
    private const int MinGold = 90;
    private const int MaxGold = 120;

    private int _goldReward;
    private bool _hasKeyCard;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://GuZhenRen/images/events/DaoTianZhenChuan_1.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpLoss", HpLoss),
        new DynamicVar("MinGold", MinGold),
        new DynamicVar("MaxGold", MaxGold)
    ];

    // Match the original mod: this event is available from rank 6 onward.
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(player =>
            player.GetRelic<AbstractKongQiaoRelic>() is { Rank: >= 6 });

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        _goldReward = Owner!.RunState.Rng.UpFront.NextInt(MinGold, MaxGold + 1);
        _hasKeyCard = Owner.Deck.Cards.Any(card =>
            card is BaShan or WanWoDaShouYin);
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new EventOption(this, GoToZhongzhou, InitialOptionKey("GO_TO_ZHONGZHOU")),
        new EventOption(this, GoToBeiyuan, InitialOptionKey("GO_TO_BEIYUAN")),
        new EventOption(this, Leave, InitialOptionKey("LEAVE"))
    ];

    private async Task GoToZhongzhou()
    {
        await LoseHealth();
        SetEventState(L10NLookup($"{Id.Entry}.pages.ZHONGZHOU.description"),
        [
            new EventOption(this, ObtainShenBuZhi, ModOptionKey("ZHONGZHOU", "OBTAIN_SHEN_BU_ZHI")),
            new EventOption(this, ObtainGold, ModOptionKey("ZHONGZHOU", "OBTAIN_GOLD"))
        ]);
    }

    private async Task GoToBeiyuan()
    {
        await LoseHealth();
        var options = new List<EventOption>
        {
            new(this, ObtainGuiBuJue, ModOptionKey("BEIYUAN", "OBTAIN_GUI_BU_JUE"))
        };

        if (_hasKeyCard)
        {
            options.Add(new EventOption(
                this,
                ObtainLuoPoGu,
                ModOptionKey("BEIYUAN", "OBTAIN_LUO_PO_GU")));
        }

        SetEventState(L10NLookup($"{Id.Entry}.pages.BEIYUAN.description"), options);
    }

    private Task Leave()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
        return Task.CompletedTask;
    }

    private async Task ObtainShenBuZhi()
    {
        await RelicCmd.Obtain<ShenBuZhi>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.SHEN_BU_ZHI_OBTAINED.description"));
    }

    private async Task ObtainGold()
    {
        await PlayerCmd.GainGold(_goldReward, Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GOLD_OBTAINED.description"));
    }

    private async Task ObtainGuiBuJue()
    {
        await RelicCmd.Obtain<GuiBuJue>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.GUI_BU_JUE_OBTAINED.description"));
    }

    private async Task ObtainLuoPoGu()
    {
        await RelicCmd.Obtain<LuoPoGu>(Owner!);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LUO_PO_GU_OBTAINED.description"));
    }

    private async Task LoseHealth()
    {
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            DynamicVars["HpLoss"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            null,
            null);
    }
}
