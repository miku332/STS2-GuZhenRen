using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class ShaZhaoRewardPoolPatch : IPatchMethod
{
    public static string PatchId => "sha_zhao_reward_pool";

    public static string Description =>
        "Exclude Sha Zhao cards from ordinary card reward pools";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardCreationOptions),
            nameof(CardCreationOptions.GetPossibleCards),
            [typeof(Player)])
    ];

    public static void Postfix(ref IEnumerable<CardModel> __result)
    {
        __result = __result.Where(
            static card => card is not AbstractShaZhaoCard
                && card is not ChengGongGu);
    }
}

public sealed class ShaZhaoMerchantPoolPatch : IPatchMethod
{
    public static string PatchId => "sha_zhao_merchant_pool";

    public static string Description =>
        "Exclude Sha Zhao cards from merchant card pools";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardFactory),
            nameof(CardFactory.CreateForMerchant),
            [
                typeof(Player),
                typeof(IEnumerable<CardModel>),
                typeof(CardType)
            ]),
        new ModPatchTarget(
            typeof(CardFactory),
            nameof(CardFactory.CreateForMerchant),
            [
                typeof(Player),
                typeof(IEnumerable<CardModel>),
                typeof(CardRarity)
            ])
    ];

    public static void Prefix(ref IEnumerable<CardModel> options)
    {
        options = options.Where(
            static card => card is not AbstractShaZhaoCard
                && card is not ChengGongGu);
    }
}
