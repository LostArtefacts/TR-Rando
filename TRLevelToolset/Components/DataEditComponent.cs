using ImGuiNET;
using TRLevelToolset.Controls.DataControls.TR;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Components;

public class DataEditComponent : IDrawable
{
    #region TR Controls
        private TRTexImage8Control _TRTexImage8Control = new();
        private TRRoomControl _TRRoomControl = new();
        private TRAnimatedTextureControl _TRAnimatedTextureControl = new();
        private TRFloorDataControl _TRFloorDataControl = new();
        private TRMeshControl _TRMeshControl = new();
        private TRANimationControl _TRAnimationControl = new();
        private TRModelControl _TRModelControl = new();
        private TRStaticMeshControls _TRStaticMeshControl = new();
        private TRSpriteAndObjTextureControl _TRSpriteAndObjTextureControl = new();
        private TRCamerasControl _TRCamerasControl = new();
        private TRSoundControl _TRSoundControl = new();
        private TRZoneControl _TRZoneControl = new();
        private TREntityControl _TREntityControl = new();
        private TRPaletteControl _TRPaletteControl = new();
        private TRCinematicsControl _TRCinematicsControl = new();
        private TRDemoDataControl _TRDemoDataControl = new();
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
