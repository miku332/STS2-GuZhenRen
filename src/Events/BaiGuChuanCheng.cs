using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Events;

[RegisterSharedEvent]
public sealed class BaiGuChuanCheng : ModEventTemplate
{
    private const int HallOneChanceIncrease = 10;
    private const int HallTwoChanceIncrease = 15;
    private const int HallThreeChanceIncrease = 20;
    private const int PavilionChanceIncrease = 15;
    private const int JumpDamage = 12;
    private const int FlightDamage = 3;

    private bool _hallOneLooted;
    private bool _hallOneHasLuoXuan;
    private bool _hallTwoBoneGunLooted;
    private bool _hallTwoShieldLooted;
    private bool _hallTwoWingLooted;
    private bool _hallThreeLooted;
    private bool _pavilionLooted;
    private int _totalLootCount;
    private int _encounterChance;

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://GuZhenRen/images/events/BaiGuChuanCheng.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("EncounterChance", _encounterChance),
        new DynamicVar("JumpDamage", JumpDamage),
        new DynamicVar("FlightDamage", FlightDamage)
    ];

    // Match the original mod: this event is available only at ranks 2-5.
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(player =>
            player.GetRelic<AbstractKongQiaoRelic>() is { Rank: > 1 and <= 5 });

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        _hallOneHasLuoXuan = Owner!.RunState.Rng.UpFront.NextFloat(100f) < 50f;
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new(this, EnterInheritance, InitialOptionKey("ENTER")),
        new(this, Leave, InitialOptionKey("LEAVE"))
    ];

    private Task EnterInheritance()
    {
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_1.description"),
            BuildHallOneOptions());
        return Task.CompletedTask;
    }

    private async Task LootHallOne()
    {
        _hallOneLooted = true;
        AddEncounterChance(HallOneChanceIncrease);
        await AddCardToDeck(
            _hallOneHasLuoXuan
                ? ModelDb.Card<LuoXuanGuQiangGu>()
                : ModelDb.Card<GuQiangGu>());
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_1_AFTER_LOOT.description"),
            [new(this, EnterHallTwo, ModOptionKey("HALL_1", "CONTINUE"))]);
    }

    private Task EnterHallTwo()
    {
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_2.description"),
            BuildHallTwoOptions());
        return Task.CompletedTask;
    }

    private async Task LootHallTwoBoneGun()
    {
        _hallTwoBoneGunLooted = true;
        AddEncounterChance(GetHallTwoChanceIncrease());
        await AddCardToDeck(ModelDb.Card<LeiGuDunGu>());
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_2.description"),
            BuildHallTwoOptions());
    }

    private async Task LootHallTwoShield()
    {
        _hallTwoShieldLooted = true;
        AddEncounterChance(GetHallTwoChanceIncrease());
        await RelicCmd.Obtain<FeiGuDunGu>(Owner!);
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_2.description"),
            BuildHallTwoOptions());
    }

    private async Task LootHallTwoWing()
    {
        _hallTwoWingLooted = true;
        AddEncounterChance(GetHallTwoChanceIncrease());
        await RelicCmd.Obtain<BiGuYiGu>(Owner!);
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_2.description"),
            BuildHallTwoOptions());
    }

    private Task EnterHallThree()
    {
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_3.description"),
            BuildHallThreeOptions());
        return Task.CompletedTask;
    }

    private async Task LootHallThree()
    {
        _hallThreeLooted = true;
        AddEncounterChance(HallThreeChanceIncrease);
        await RelicCmd.Obtain<GuCiGu>(Owner!);
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.HALL_3_AFTER_LOOT.description"),
            [new(this, EnterPavilion, ModOptionKey("HALL_3", "CONTINUE"))]);
    }

    private Task EnterPavilion()
    {
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.PAVILION.description"),
            BuildPavilionOptions());
        return Task.CompletedTask;
    }

    private async Task LootPavilion()
    {
        _pavilionLooted = true;
        AddEncounterChance(PavilionChanceIncrease);
        await GiveRandomBoneLoot();
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.PAVILION_AFTER_LOOT.description"),
            [new(this, FinishExploration, ModOptionKey("PAVILION", "LEAVE"))]);
    }

    private async Task FinishExploration()
    {
        if (_totalLootCount == 0)
        {
            await AddCardToDeck(ModelDb.Card<ZhanGuCheLun>());
            await RelicCmd.Obtain<RecipeBaiGuZhanChe>(Owner!);
            SetEventFinished(
                L10NLookup($"{Id.Entry}.pages.SECRET_REWARD.description"));
            return;
        }

        SetEventState(
            L10NLookup($"{Id.Entry}.pages.ESCAPE.description"),
            BuildEscapeOptions());
    }

    private async Task TakeCurse()
    {
        var injury = ModelDb.AllCards.FirstOrDefault(
            card => card.Id.Entry == "INJURY");
        if (injury is not null)
        {
            await AddCardToDeck(injury);
        }

        SetEventFinished(L10NLookup($"{Id.Entry}.pages.CURSE.description"));
    }

    private async Task JumpFromCliff()
    {
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            DynamicVars["JumpDamage"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            null);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.JUMP.description"));
    }

    private async Task FlyAway()
    {
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            DynamicVars["FlightDamage"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            null);
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.FLY.description"));
    }

    private Task Leave()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
        return Task.CompletedTask;
    }

    private IReadOnlyList<EventOption> BuildHallOneOptions() =>
    [
        ..(!_hallOneLooted
            ? [new EventOption(
                this,
                LootHallOne,
                ModOptionKey("HALL_1", "LOOT"))]
            : Array.Empty<EventOption>()),
        new(this, EnterHallTwo, ModOptionKey("HALL_1", "CONTINUE"))
    ];

    private IReadOnlyList<EventOption> BuildHallTwoOptions()
    {
        var options = new List<EventOption>();
        if (!_hallTwoBoneGunLooted)
        {
            options.Add(new(this, LootHallTwoBoneGun,
                ModOptionKey("HALL_2", "LOOT_BONE_GUN")));
        }

        if (!_hallTwoShieldLooted)
        {
            options.Add(new(this, LootHallTwoShield,
                ModOptionKey("HALL_2", "LOOT_SHIELD")));
        }

        if (!_hallTwoWingLooted)
        {
            options.Add(new(this, LootHallTwoWing,
                ModOptionKey("HALL_2", "LOOT_WING")));
        }

        options.Add(new(this, EnterHallThree,
            ModOptionKey("HALL_2", "CONTINUE")));
        return options;
    }

    private IReadOnlyList<EventOption> BuildHallThreeOptions() =>
    [
        ..(!_hallThreeLooted
            ? [new EventOption(
                this,
                LootHallThree,
                ModOptionKey("HALL_3", "LOOT"))]
            : Array.Empty<EventOption>()),
        new(this, EnterPavilion, ModOptionKey("HALL_3", "CONTINUE"))
    ];

    private IReadOnlyList<EventOption> BuildPavilionOptions() =>
    [
        ..(!_pavilionLooted
            ? [new EventOption(
                this,
                LootPavilion,
                ModOptionKey("PAVILION", "LOOT"))]
            : Array.Empty<EventOption>()),
        new(this, FinishExploration, ModOptionKey("PAVILION", "LEAVE"))
    ];

    private IReadOnlyList<EventOption> BuildEscapeOptions()
    {
        var options = new List<EventOption>
        {
            new(this, TakeCurse, ModOptionKey("ESCAPE", "TAKE_CURSE")),
            new(this, JumpFromCliff, ModOptionKey("ESCAPE", "JUMP"))
        };

        if (Owner!.Deck.Cards.Any(card => card is WuZuNiao))
        {
            options.Add(new(this, FlyAway, ModOptionKey("ESCAPE", "FLY")));
        }

        return options;
    }

    private async Task GiveRandomBoneLoot()
    {
        var cardCandidates = new List<CardModel>
        {
            ModelDb.Card<LeiGuDunGu>(),
            ModelDb.Card<WuZuNiao>()
        };

        var relicCandidates = new List<RelicModel>
        {
            ModelDb.Relic<RouBaiGu>(),
            ModelDb.Relic<BiGuYiGu>(),
            ModelDb.Relic<TieGuGu>(),
            ModelDb.Relic<GuCiGu>(),
            ModelDb.Relic<FeiGuDunGu>()
        }.Where(relic => Owner!.Relics.All(
            owned => owned.Id != relic.Id)).ToList();

        if (cardCandidates.Count > 0
            && (relicCandidates.Count == 0
                || Owner!.PlayerRng.Rewards.NextFloat(100f) < 50f))
        {
            await AddCardToDeck(
                Owner!.PlayerRng.Rewards.NextItem(cardCandidates)!);
            return;
        }

        if (relicCandidates.Count > 0)
        {
            await RelicCmd.Obtain(
                Owner!.PlayerRng.Rewards.NextItem(relicCandidates)!.ToMutable(),
                Owner);
            return;
        }

        await AddCardToDeck(ModelDb.Card<LeiGuDunGu>());
    }

    private async Task AddCardToDeck(CardModel canonical)
    {
        _totalLootCount++;
        var card = Owner!.RunState.CreateCard(canonical, Owner);
        card.FloorAddedToDeck = Owner.RunState.TotalFloor;
        SaveManager.Instance.MarkCardAsSeen(card);
        if (!Owner.DiscoveredCards.Contains(card.Id))
        {
            Owner.DiscoveredCards.Add(card.Id);
        }

        var result = await CardPileCmd.Add(
            card,
            PileType.Deck,
            CardPilePosition.Bottom,
            null,
            false);
        if (!result.success)
        {
            Entry.Logger.Error($"Failed to add Bai Gu Chuan Cheng card {card.Id.Entry}.");
            return;
        }

        result.cardAdded.Pile?.InvokeCardAddFinished();
        CardCmd.PreviewCardPileAdd([result], 1.5f);
    }

    private void AddEncounterChance(int amount)
    {
        _encounterChance += amount;
        DynamicVars["EncounterChance"].BaseValue = _encounterChance;
    }

    private int GetHallTwoChanceIncrease() =>
        (_hallTwoBoneGunLooted ? 1 : 0)
        + (_hallTwoShieldLooted ? 1 : 0)
        + (_hallTwoWingLooted ? 1 : 0) switch
        {
            0 => HallTwoChanceIncrease,
            1 => 30,
            _ => 45
        };
}
