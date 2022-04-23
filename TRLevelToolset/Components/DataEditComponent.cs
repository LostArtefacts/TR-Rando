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
        #region TR Controls
            private TRTexImage8Control _TRTexImage8Control = new TRTexImage8Control();
            private TRRoomControl _TRRoomControl = new TRRoomControl();
            private TRAnimatedTextureControl _TRAnimatedTextureControl = new TRAnimatedTextureControl();
            private TRFloorDataControl _TRFloorDataControl = new TRFloorDataControl();
            private TRMeshControl _TRMeshControl = new TRMeshControl();
            private TRANimationControl _TRAnimationControl = new TRANimationControl();
            private TRModelControl _TRModelControl = new TRModelControl();
            private TRStaticMeshControls _TRStaticMeshControl = new TRStaticMeshControls();
            private TRSpriteAndObjTextureControl _TRSpriteAndObjTextureControl = new TRSpriteAndObjTextureControl();
            private TRCamerasControl _TRCamerasControl = new TRCamerasControl();
            private TRSoundControl _TRSoundControl = new TRSoundControl();
            private TRZoneControl _TRZoneControl = new TRZoneControl();
            private TREntityControl _TREntityControl = new TREntityControl();
            private TRPaletteControl _TRPaletteControl = new TRPaletteControl();
            private TRCinematicsControl _TRCinematicsControl = new TRCinematicsControl();
            private TRDemoDataControl _TRDemoDataControl = new TRDemoDataControl();
        #endregion

        public void Draw()
        {
            if (ImGui.TreeNodeEx("Level Data Viewer/Editor", ImGuiTreeNodeFlags.Framed))
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
            _TRMeshControl.Draw();
            _TRAnimationControl.Draw();
            _TRModelControl.Draw();
            _TRStaticMeshControl.Draw();
            _TRSpriteAndObjTextureControl.Draw();
            _TRCamerasControl.Draw();
            _TRSoundControl.Draw();
            _TRZoneControl.Draw();
            _TREntityControl.Draw();
            _TRPaletteControl.Draw();
            _TRCinematicsControl.Draw();
            _TRDemoDataControl.Draw();
        }
    }
}
