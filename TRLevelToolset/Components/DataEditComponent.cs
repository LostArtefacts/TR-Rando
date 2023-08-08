using ImGuiNET;
using TRLevelToolset.Controls.DataControls.TR;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Components;

public class DataEditComponent : IDrawable
{
    #region TR Controls
        private readonly TRTexImage8Control _TRTexImage8Control = new();
        private readonly TRRoomControl _TRRoomControl = new();
        private readonly TRAnimatedTextureControl _TRAnimatedTextureControl = new();
        private readonly TRFloorDataControl _TRFloorDataControl = new();
        private readonly TRMeshControl _TRMeshControl = new();
        private readonly TRANimationControl _TRAnimationControl = new();
        private readonly TRModelControl _TRModelControl = new();
        private readonly TRStaticMeshControls _TRStaticMeshControl = new();
        private readonly TRSpriteAndObjTextureControl _TRSpriteAndObjTextureControl = new();
        private readonly TRCamerasControl _TRCamerasControl = new();
        private readonly TRSoundControl _TRSoundControl = new();
        private readonly TRZoneControl _TRZoneControl = new();
        private readonly TREntityControl _TREntityControl = new();
        private readonly TRPaletteControl _TRPaletteControl = new();
        private readonly TRCinematicsControl _TRCinematicsControl = new();
        private readonly TRDemoDataControl _TRDemoDataControl = new();
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
