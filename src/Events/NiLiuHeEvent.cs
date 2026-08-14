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
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Events;

[RegisterSharedEvent]
public sealed class NiLiuHeEvent : ModEventTemplate
{
    private const int MaxProgress = 18;
    private const int EncounterLimit = 12;
    private const int MaxEncounterPity = 2;

    private int _progress;
    private int _consecutiveNoEncounter;
    private int _currentEncounter = -1;
    private int _encounterStep;
    private bool _skipNextEncounter;
    private readonly List<int> _availableEncounters = [];
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://GuZhenRen/images/events/NiLiuHe.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Progress", 0),
        new DynamicVar("MaxProgress", MaxProgress),
        new DynamicVar("HpLoss", 1),
        new DynamicVar("EncounterStep", 0),
        new DynamicVar("RewardGoldMin", 50),
        new DynamicVar("RewardGoldMax", 80),
        new DynamicVar("Heal", 20),
        new DynamicVar("MaxHpLoss", 8)
    ];

    // Match the original mod: rank 7 onward and at least 24 current HP.
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(player =>
            player.GetRelic<AbstractKongQiaoRelic>() is { Rank: >= 7 } &&
            player.Creature.CurrentHp >= 24);

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        _progress = 0;
        _consecutiveNoEncounter = 0;
        _currentEncounter = -1;
        _encounterStep = 0;
        _skipNextEncounter = false;
        _availableEncounters.Clear();
        _availableEncounters.AddRange([0, 1, 2, 3, 4, 5]);
        _availableEncounters.Add(
            Owner!.RunState.Rng.UpFront.NextFloat(100f) < 50f ? 6 : 7);
        UpdateProgressVars();
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
    [
        new(this, EnterRiver, InitialOptionKey("ENTER")),
        new(this, Leave, InitialOptionKey("LEAVE"))
    ];

    private Task EnterRiver()
    {
        ShowRiver();
        return Task.CompletedTask;
    }

    private async Task AdvanceRiver()
    {
        await LoseHp(GetHpLoss());
        _progress++;
        UpdateProgressVars();

        if (_progress >= MaxProgress)
        {
            ShowJianChiEnding();
            return;
        }

        if (_skipNextEncounter)
        {
            _skipNextEncounter = false;
            ShowRiver();
            return;
        }

        if (_progress < EncounterLimit
            && _availableEncounters.Count > 0
            && (_consecutiveNoEncounter >= MaxEncounterPity
                || Owner!.RunState.Rng.UpFront.NextFloat(100f) < 25f))
        {
            _consecutiveNoEncounter = 0;
            _currentEncounter = Owner!.RunState.Rng.UpFront.NextItem(_availableEncounters);
            _availableEncounters.Remove(_currentEncounter);
            _encounterStep = 0;
            UpdateEncounterVars();
            ShowEncounter();
            return;
        }

        _consecutiveNoEncounter++;
        ShowRiver();
    }

    private Task AdvanceAfterEncounter()
    {
        _currentEncounter = -1;
        _skipNextEncounter = true;
        return AdvanceRiver();
    }

    private Task Leave()
    {
        SetEventFinished(PageDescription("LEAVE"));
        return Task.CompletedTask;
    }

    private async Task HandleEncounterAction(string action)
    {
        switch (_currentEncounter)
        {
            case 0 when action == "LOOT":
                await PlayerCmd.GainGold(
                    Owner!.RunState.Rng.UpFront.NextInt(50, 81),
                    Owner);
                if (Owner.RunState.Rng.UpFront.NextFloat(100f) < 25f)
                {
                    await LoseHp(Math.Max(1, (int)(Owner.Creature.MaxHp * 0.10m)));
                }

                await ResolveEncounter();
                return;

            case 1 when action == "PUNCH":
                await AdvancePunchFight();
                return;

            case 2 when action == "ATTACK":
                await AdvanceExpertFight();
                return;

            case 3 when action == "REST":
                if (Owner!.RunState.Rng.UpFront.NextFloat(100f) < 75f)
                {
                    await CreatureCmd.Heal(
                        Owner.Creature,
                        Math.Max(1, (int)(Owner.Creature.MaxHp * 0.20m)));
                }

                await ResolveEncounter();
                return;

            case 4 when action == "LOOT_CARD":
                await LoseHp(3);
                await AddCardToDeck(CreateRandomGuCard());
                await ResolveEncounter();
                return;

            case 5 when action == "LOOT_RELIC":
                await LoseHp(3);
                await ObtainRandomGuRelic();
                await ResolveEncounter();
                return;

            case 6:
                await HandleRockAction(action);
                return;

            case 7 when action == "ESCAPE":
                await LoseHp(3);
                if (Owner!.RunState.Rng.UpFront.NextFloat(100f) < 50f)
                {
                    await RemoveRandomNonStarterRelic();
                }

                await ResolveEncounter();
                return;
        }
    }

    private async Task AdvancePunchFight()
    {
        _encounterStep++;
        UpdateEncounterVars();
        if (_encounterStep < 3)
        {
            await LoseHp(2);
            ShowEncounterStage();
            return;
        }

        await PlayerCmd.GainGold(Owner!.RunState.Rng.UpFront.NextInt(10, 21), Owner);
        await ResolveEncounter();
    }

    private async Task AdvanceExpertFight()
    {
        _encounterStep++;
        UpdateEncounterVars();
        if (_encounterStep < 3)
        {
            await LoseHp(4);
            ShowEncounterStage();
            return;
        }

        await PlayerCmd.GainGold(Owner!.RunState.Rng.UpFront.NextInt(25, 36), Owner);
        await ObtainRandomGuRelic();
        await ResolveEncounter();
    }

    private async Task HandleRockAction(string action)
    {
        switch (action)
        {
            case "DEFEND":
                await LoseHp(Math.Max(1, (int)(Owner!.Creature.MaxHp * 0.25m)));
                break;
            case "BREAK":
                await CreatureCmd.LoseMaxHp(
                    new ThrowingPlayerChoiceContext(),
                    Owner!.Creature,
                    DynamicVars["MaxHpLoss"].BaseValue,
                    false);
                break;
            case "DODGE":
                await AddInjury();
                break;
        }

        await ResolveEncounter();
    }

    private async Task ResolveEncounter()
    {
        _encounterStep = 0;
        UpdateEncounterVars();
        SetEventState(
            PageDescription("RESULT"),
            BuildResultOptions());
        await Task.CompletedTask;
    }

    private void ShowRiver()
    {
        UpdateProgressVars();
        SetEventState(PageDescription("RIVER"),
        [
            new(this, AdvanceRiver, ModOptionKey("RIVER", "ADVANCE")),
            new(this, Leave, ModOptionKey("RIVER", "LEAVE"))
        ]);
    }

    private void ShowEncounter()
    {
        SetEventState(
            PageDescription($"ENCOUNTER_{_currentEncounter}"),
            BuildEncounterOptions());
    }

    private void ShowEncounterStage()
    {
        SetEventState(
            PageDescription("ENCOUNTER_STAGE"),
            _currentEncounter == 1
                ? [
                    new(this, () => HandleEncounterAction("PUNCH"),
                        ModOptionKey("ENCOUNTER_STAGE", "PUNCH")),
                    new(this, Leave, ModOptionKey("ENCOUNTER_STAGE", "LEAVE"))
                ]
                : [
                    new(this, () => HandleEncounterAction("ATTACK"),
                        ModOptionKey("ENCOUNTER_STAGE", "ATTACK")),
                    new(this, Leave, ModOptionKey("ENCOUNTER_STAGE", "LEAVE"))
                ]);
    }

    private void ShowJianChiEnding()
    {
        SetEventState(
            PageDescription("JIAN_CHI"),
            [new(this, ObtainJianChiGu, ModOptionKey("JIAN_CHI", "OBTAIN"))]);
    }

    private async Task ObtainJianChiGu()
    {
        await RelicCmd.Obtain<JianChiGu>(Owner!);
        SetEventState(
            PageDescription("NI_LIU_HE"),
            [new(this, ObtainNiLiuHe, ModOptionKey("NI_LIU_HE", "OBTAIN"))]);
    }

    private async Task ObtainNiLiuHe()
    {
        await RelicCmd.Obtain<NiLiuHe>(Owner!);
        SetEventState(PageDescription("FINAL"), BuildFinalOptions());
    }

    private IReadOnlyList<EventOption> BuildFinalOptions()
    {
        var options = new List<EventOption>();
        if (Owner!.Deck.Cards.Any(card => card is WanLan))
        {
            options.Add(new(
                this,
                ExchangeWanLan,
                ModOptionKey("FINAL", "EXCHANGE")));
        }

        options.Add(new(this, Leave, ModOptionKey("FINAL", "LEAVE")));
        return options;
    }

    private async Task ExchangeWanLan()
    {
        var wanLan = Owner!.Deck.Cards.FirstOrDefault(card => card is WanLan);
        if (wanLan is not null)
        {
            await CardPileCmd.RemoveFromDeck(wanLan);
            await AddCardToDeck(ModelDb.Card<NiLiuHuShenYin>());
        }

        SetEventFinished(PageDescription("EXCHANGE"));
    }

    private IReadOnlyList<EventOption> BuildResultOptions() =>
    [
        new(this, AdvanceAfterEncounter, ModOptionKey("RESULT", "ADVANCE")),
        new(this, Leave, ModOptionKey("RESULT", "LEAVE"))
    ];

    private IReadOnlyList<EventOption> BuildEncounterOptions() =>
        _currentEncounter switch
        {
            0 => [
                new(this, () => HandleEncounterAction("LOOT"),
                    ModOptionKey("ENCOUNTER", "LOOT")),
                new(this, AdvanceAfterEncounter, ModOptionKey("ENCOUNTER", "ADVANCE")),
                new(this, Leave, ModOptionKey("ENCOUNTER", "LEAVE"))
            ],
            1 => [
                new(this, () => HandleEncounterAction("PUNCH"),
                    ModOptionKey("ENCOUNTER", "PUNCH")),
                new(this, Leave, ModOptionKey("ENCOUNTER", "LEAVE"))
            ],
            2 => [
                new(this, () => HandleEncounterAction("ATTACK"),
                    ModOptionKey("ENCOUNTER", "ATTACK")),
                new(this, Leave, ModOptionKey("ENCOUNTER", "LEAVE"))
            ],
            3 => [
                new(this, () => HandleEncounterAction("REST"),
                    ModOptionKey("ENCOUNTER", "REST"))
            ],
            4 => [
                new(this, () => HandleEncounterAction("LOOT_CARD"),
                    ModOptionKey("ENCOUNTER", "LOOT_CARD")),
                new(this, AdvanceAfterEncounter, ModOptionKey("ENCOUNTER", "ADVANCE")),
                new(this, Leave, ModOptionKey("ENCOUNTER", "LEAVE"))
            ],
            5 => [
                new(this, () => HandleEncounterAction("LOOT_RELIC"),
                    ModOptionKey("ENCOUNTER", "LOOT_RELIC")),
                new(this, AdvanceAfterEncounter, ModOptionKey("ENCOUNTER", "ADVANCE")),
                new(this, Leave, ModOptionKey("ENCOUNTER", "LEAVE"))
            ],
            6 => [
                new(this, () => HandleEncounterAction("DEFEND"),
                    ModOptionKey("ENCOUNTER", "DEFEND")),
                new(this, () => HandleEncounterAction("BREAK"),
                    ModOptionKey("ENCOUNTER", "BREAK")),
                new(this, () => HandleEncounterAction("DODGE"),
                    ModOptionKey("ENCOUNTER", "DODGE"))
            ],
            7 => [
                new(this, () => HandleEncounterAction("ESCAPE"),
                    ModOptionKey("ENCOUNTER", "ESCAPE"))
            ],
            _ => []
        };

    private int GetHpLoss() =>
        _progress < 6 ? 1 : _progress < 12 ? 2 : 3;

    private async Task LoseHp(int amount)
    {
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner!.Creature,
            amount,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            null,
            null);
    }

    private async Task AddInjury()
    {
        var injury = ModelDb.AllCards.FirstOrDefault(card => card.Id.Entry == "INJURY");
        if (injury is not null)
        {
            await AddCardToDeck(injury);
        }
    }

    private async Task ObtainRandomGuRelic()
    {
        var candidates = new RelicModel[]
        {
            ModelDb.Relic<JianMei>(),
            ModelDb.Relic<ShuiWenGu>(),
            ModelDb.Relic<TuDuiGu>(),
            ModelDb.Relic<ChiXiang>(),
            ModelDb.Relic<FengXiongHuaJi>(),
            ModelDb.Relic<GouShiYun>(),
            ModelDb.Relic<NongXuGu>(),
            ModelDb.Relic<SiXuRuDianGu>(),
            ModelDb.Relic<YanXinGu>(),
            ModelDb.Relic<CunGuangYin>(),
            ModelDb.Relic<FeiLiGu>(),
            ModelDb.Relic<HongYunQiTianGu>(),
            ModelDb.Relic<MuYa>(),
            ModelDb.Relic<NengLiGu>(),
            ModelDb.Relic<TouSheng>()
        };
        var available = candidates
            .Where(candidate => Owner!.Relics.All(owned => owned.Id != candidate.Id))
            .ToList();
        var selected = Owner!.RunState.Rng.UpFront.NextItem(available);
        if (selected is not null)
        {
            await RelicCmd.Obtain(selected.ToMutable(), Owner);
        }
    }

    private CardModel CreateRandomGuCard() =>
        Owner!.RunState.Rng.UpFront.NextItem(
            new CardModel[]
            {
                ModelDb.Card<WanLan>(),
                ModelDb.Card<ShangFangJieWa>(),
                ModelDb.Card<WuZhiQuanXinJian>(),
                ModelDb.Card<LiLiangGu>()
            })!;

    private async Task AddCardToDeck(CardModel canonical)
    {
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
        if (result.success)
        {
            result.cardAdded.Pile?.InvokeCardAddFinished();
            CardCmd.PreviewCardPileAdd([result], 1.5f);
        }
    }

    private async Task RemoveRandomNonStarterRelic()
    {
        var candidates = Owner!.Relics
            .Where(relic => relic.Rarity is not RelicRarity.Starter and not RelicRarity.Event)
            .ToList();
        var selected = Owner.RunState.Rng.UpFront.NextItem(candidates);
        if (selected is not null)
        {
            await RelicCmd.Remove(selected);
        }
    }

    private void UpdateProgressVars()
    {
        DynamicVars["Progress"].BaseValue = _progress;
        DynamicVars["HpLoss"].BaseValue = GetHpLoss();
    }

    private void UpdateEncounterVars()
    {
        DynamicVars["EncounterStep"].BaseValue = _encounterStep;
    }
}
