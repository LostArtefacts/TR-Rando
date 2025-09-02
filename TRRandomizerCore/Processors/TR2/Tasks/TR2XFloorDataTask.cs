using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XFloorDataTask : ITR2ProcessorTask
{
    public void Run(TR2CombinedLevel level)
    {
        foreach (var sector in level.Data.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0)))
        {
            var trigger = level.Data.FloorData[sector.FDIndex]
                .OfType<FDTriggerEntry>()
                .FirstOrDefault();
            if (trigger == null)
            {
                continue;
            }

            // Glide cameras are restored in TR2X, but some triggers retain faulty values
            if (trigger.Actions.Find(a => a.Action == FDTrigAction.Camera) is FDActionItem cameraAction
                && cameraAction.CamAction.Timer != 0
                && cameraAction.CamAction.MoveTimer > 1
                && !trigger.Actions.Any(a => a.Action == FDTrigAction.LookAtItem))
            {
                var glide = (byte)(cameraAction.CamAction.MoveTimer + 2);
                cameraAction.CamAction.Timer = Math.Max(glide, cameraAction.CamAction.Timer);
            }

            // Get rid of stale secret triggers
            trigger.Actions.RemoveAll(a => a.Action == FDTrigAction.SecretFound);
            if (trigger.Actions.Count == 0)
            {
                level.Data.FloorData[sector.FDIndex].Remove(trigger);
            }
        }

        if (level.Is(TR2LevelNames.MONASTERY))
        {
            level.Data.Cameras[7].Y -= TRConsts.Step2 + 16;
        }
    }
}
