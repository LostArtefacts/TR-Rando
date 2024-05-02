using TRLevelControl;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Secrets;

public class TRSecretPlacement<E> where E : Enum
{
    public Location Location { get; set; }
    public E PickupType { get; set; }
    public short EntityIndex { get; set; }
    public short SecretIndex { get; set; }
    public short DoorIndex { get; set; }
    public short CameraIndex { get; set; }
    public short CameraTarget { get; set; }
    public byte TriggerMask { get; set; }
    public bool TriggersDoor => DoorIndex != short.MaxValue;
    public bool TriggersCamera => CameraIndex != short.MaxValue;

    public TRSecretPlacement()
    {
        // Default to standard mask and no door
        TriggerMask = TRConsts.FullMask;
        DoorIndex = short.MaxValue;
        CameraIndex = short.MaxValue;
    }

    public void SetMaskAndDoor(int secretCount, List<int> doorItems)
    {
        if (doorItems.Count == 0)
        {
            return;
        }

        // This will assign one bit for each secret. For the final secret in
        // a group, its bits will be "topped-up" to ensure the full activation 
        // mask is set. So for example, if there are 3 secrets in a level, the
        // masks will be:
        //     00001
        //     00010
        //     11100

        int split = (int)Math.Ceiling((double)secretCount / doorItems.Count);

        int position = SecretIndex % split;
        int mask = 1 << position; // 1,2,4,8,16

        // Make sure to top-up the final mask in this group
        int alignedIndex = SecretIndex + 1;
        if (alignedIndex == secretCount || alignedIndex % split == 0)
        {
            while (++position < TRConsts.MaskBits)
            {
                mask |= 1 << position;
            }
        }

        TriggerMask = (byte)mask;
        DoorIndex = (short)doorItems[SecretIndex / split];
    }
}
