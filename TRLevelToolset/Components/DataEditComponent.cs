using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelToolset.Controls.DataControls.TR;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Components
{
    public class DataEditComponent : IDrawable
    {
        private TRTexImage8Control _TRTexImage8Control = new TRTexImage8Control();
        private TRRoomControl _TRRoomControl = new TRRoomControl();
        private TRAnimatedTextureControl _TRAnimatedTextureControl = new TRAnimatedTextureControl();
        private TRFloorDataControl _TRFloorDataControl = new TRFloorDataControl();

        public void Draw()
        {
            if (ImGui.TreeNodeEx("Level Data Editor", ImGuiTreeNodeFlags.Framed))
            {
                switch (IOManager.LoadedGame)
                {
                    case TRGame.TR1:
                        DrawTR1Controls();
                        break;
                    case TRGame.TR2:
                        break;
                    case TRGame.TR3:
                        break;
                    case TRGame.TR4:
                        break;
                    case TRGame.TR5:
                        break;
                    default:
                        break;
                }

                ImGui.TreePop();
            }
        }

        private void DrawTR1Controls()
        {
            _TRTexImage8Control.Draw();
            _TRRoomControl.Draw();
            _TRFloorDataControl.Draw();
            _TRAnimatedTextureControl.Draw();
        }
    }
}
