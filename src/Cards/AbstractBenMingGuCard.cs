using MegaCrit.Sts2.Core.Entities.Cards;

namespace GuZhenRen.Cards;

public abstract class AbstractBenMingGuCard : GuZhenRenCardTemplate
{
    private static readonly AsyncLocal<int> SynthesizingDepth = new();

    // Ingredients consumed while assembling a killer move do not trigger the
    // BenMingGu removal penalty in the original mod.
    internal static bool IsSynthesizing => SynthesizingDepth.Value > 0;

    internal static IDisposable EnterSynthesisScope()
    {
        SynthesizingDepth.Value++;
        return new SynthesisScope();
    }

    protected virtual int MaxRank => 9;

    public sealed override int Rank => CurrentUpgradeLevel + 1;

    public sealed override int MaxUpgradeLevel => Math.Max(1, MaxRank - 1);

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected AbstractBenMingGuCard(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType)
        : base(energyCost, cardType, rarity, targetType, true)
    {
    }

    private sealed class SynthesisScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            SynthesizingDepth.Value = Math.Max(0, SynthesizingDepth.Value - 1);
        }
    }
}
