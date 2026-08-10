using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HuoGu : AbstractBenMingGuCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuoGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            if (Rank == 1)
            {
                yield return CardKeyword.Ethereal;
            }

            if (Rank >= 6)
            {
                yield return CardKeyword.Retain;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new PowerVar<FenShaoPower>(1).WithPowerTooltip()
    ];

    public HuoGu()
        : base(1, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count == 0)
        {
            return;
        }

        var maxSelect = Math.Min(
            hand.Cards.Count,
            DynamicVars["Cards"].IntValue);
        var selectorPrefs = new CardSelectorPrefs(
            SelectionScreenPrompt,
            0,
            maxSelect)
        {
            Cancelable = true
        };
        var selectedCards = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            selectorPrefs,
            static _ => true,
            this)).ToList();

        foreach (var selected in selectedCards)
        {
            await CardCmd.Exhaust(choiceContext, selected);
            await PowerCmd.Apply<FenShaoPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["FenShaoPower"].IntValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(
            GetCards(Rank) - DynamicVars["Cards"].BaseValue);

        if (Rank == 1)
        {
            AddKeyword(CardKeyword.Ethereal);
        }
        else
        {
            RemoveKeyword(CardKeyword.Ethereal);
        }

        if (Rank >= 6)
        {
            AddKeyword(CardKeyword.Retain);
        }
        else
        {
            RemoveKeyword(CardKeyword.Retain);
        }
    }

    private static int GetCards(int rank) =>
        rank switch
        {
            <= 2 => 3,
            3 => 4,
            4 => 5,
            5 => 6,
            6 => 7,
            7 => 8,
            8 => 8,
            _ => 9
        };

}
