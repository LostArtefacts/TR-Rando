namespace TRRandomizerCore.Randomizers;

public static class SecretConsts
{
    public static readonly string InvalidLocationMsg = "Cannot place a nonvalidated secret where a trigger already exists - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    public static readonly string TrapdoorLocationMsg = "Cannot place a secret on the same sector as a bridge/trapdoor - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    public static readonly string MidairErrorMsg = "Cannot place a secret in mid-air or on a breakable tile - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    public static readonly string TriggerWarningMsg = "Existing trigger object action with parameter {0} will be lost - {1} [X={2}, Y={3}, Z={4}, R={5}]";
    public static readonly string FlipMapWarningMsg = "Secret is being placed in a room that has a flipmap - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    public static readonly string FlipMapErrorMsg = "Secret cannot be placed in a flipped room - {0} [X={1}, Y={2}, Z={3}, R={4}]";
    public static readonly string EdgeInfoMsg = "Adding extra tile edge trigger for {0} [X={1}, Y={2}, Z={3}, R={4}]";

    public static readonly int TriggerEdgeLimit = 103; // Within ~10% of a tile edge, triggers should be copied into neighbours
}
