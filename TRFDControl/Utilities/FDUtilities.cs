using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRFDControl.FDEntryTypes;

namespace TRFDControl.Utilities
{
    public static class FDUtilities
    {
        public static List<FDTriggerEntry> GetEntityTriggers(FDControl control, int entityIndex)
        {
            List<FDTriggerEntry> entries = new List<FDTriggerEntry>();

            foreach (List<FDEntry> entryList in control.Entries.Values)
            {
                foreach (FDEntry entry in entryList)
                {
                    if (entry is FDTriggerEntry triggerEntry)
                    {
                        int itemIndex = triggerEntry.TrigActionList.FindIndex
                        (
                            i => 
                                i.TrigAction == FDTrigAction.Object && i.Parameter == entityIndex
                        );
                        if (itemIndex != -1)
                        {
                            entries.Add(triggerEntry);
                        }
                    }
                }
            }

            return entries;
        }
    }
}