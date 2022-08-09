using System.Collections.Generic;
using TRLevelReader.Helpers;
using TRLevelReader.Model.Enums;

namespace TRRandomizerCore.Utilities
{
    public static class TR1EnemyUtilities
    {
        public static Dictionary<TREntities, TREntities> GetAliasPriority(string lvlName, List<TREntities> importEntities)
        {
            Dictionary<TREntities, TREntities> priorities = new Dictionary<TREntities, TREntities>();

            switch (lvlName)
            {
                // Essential MiscAnims - e.g. they contain level end triggers
                case TRLevelNames.QUALOPEC:
                    priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Qualopec;
                    break;
                case TRLevelNames.MIDAS:
                    priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Midas;
                    break;
                case TRLevelNames.SANCTUARY:
                    priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Sanctuary;
                    break;
                case TRLevelNames.ATLANTIS:
                    priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Atlantis;
                    break;

                // Lara's specific deaths:
                //    - Adam + LaraMiscAnim_H_Valley = a fairly wonky death
                //    - TRex + LaraMiscAnim_H_Pyramid = a very wonky death
                // So if both Adam and TRex are present, the TRex anim is chosen,
                // otherwise it's their corresponding anim.
                default:
                    if (importEntities.Contains(TREntities.TRex))
                    {
                        priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Valley;
                    }
                    else if (importEntities.Contains(TREntities.Adam))
                    {
                        priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_Valley;
                    }
                    else
                    {
                        priorities[TREntities.LaraMiscAnim_H] = TREntities.LaraMiscAnim_H_General;
                    }
                    break;
            }

            return priorities;
        }
    }
}