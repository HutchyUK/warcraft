using Warcraft.Api.DTOs;

namespace Warcraft.Api.Services;

public static class VaultService
{
    // Great Vault slot thresholds
    private static readonly int[] MythicPlusThresholds = [1, 4, 8];
    private static readonly int[] RaidThresholds = [2, 4, 8];

    public static int ComputeSlots(int count, int[] thresholds)
    {
        int slots = 0;
        foreach (var threshold in thresholds)
            if (count >= threshold) slots++;
        return slots;
    }

    public static VaultProgressDto Compute(int mpRunCount, int raidBossKills, bool delvesDone)
    {
        int mpSlots = ComputeSlots(mpRunCount, MythicPlusThresholds);
        int raidSlots = ComputeSlots(raidBossKills, RaidThresholds);
        int delveSlots = delvesDone ? 1 : 0;

        return new VaultProgressDto(
            mpRunCount, mpSlots,
            raidBossKills, raidSlots,
            delvesDone, delveSlots,
            mpSlots + raidSlots + delveSlots);
    }
}
