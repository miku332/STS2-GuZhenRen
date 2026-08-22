namespace GuZhenRen.Systems;

public interface IProbabilityCard
{
    void IncreaseBaseChance(decimal percentagePoints);

    bool InvertProbabilityModifiers => false;
}
