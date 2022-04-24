using ImGuiNET;
using System.Numerics;
using TRLevelReader.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR
{
    internal class TRCamerasControl : IDrawable, IModelUpdater
    {
        private TRCamera _camera;

        private int _index = 0;
        private int _x;
        private int _y;
        private int _z;
        private int _room;
        private int _flags;

        public void Draw()
        {
            Clamp();
            
            if (ImGui.TreeNodeEx("Camera Data", ImGuiTreeNodeFlags.OpenOnArrow))
            {
                ImGui.Text("Camera count: " + IOManager.CurrentLevelAsTR1?.NumCameras);

                ImGui.BeginChild("cam_property_display", new Vector2(400, 200), true);
                
                if (ImGui.InputInt("Camera ID", ref _index, 1, 1))
                {
                    Clamp();
                    Populate();
                }
                
                ImGui.InputInt("X: ", ref _x);
                ImGui.InputInt("Y: ", ref _y);
                ImGui.InputInt("Z: ", ref _z);
                ImGui.InputInt("Room: ", ref _room);
                ImGui.InputInt("Flags: ", ref _flags);

                if (ImGui.Button("Update Current Camera Values"))
                    Apply();
                
                if (ImGui.Button("Reset Current Camera Values"))
                    Populate();
                
                ImGui.EndChild();
                
                ImGui.TreePop();
            }
        }

        public void Clamp()
        {
            if (IOManager.CurrentLevelAsTR1 is null)
            {
                _index = 0;
                return;
            }

            if (_index < 0)
                _index = 0;

            if (_index >= IOManager.CurrentLevelAsTR1?.NumCameras)
                _index = (int)(IOManager.CurrentLevelAsTR1?.NumCameras - 1);
        }

        public void Populate()
        {
            if (IOManager.CurrentLevelAsTR1 is null)
                return;
            
            if (IOManager.CurrentLevelAsTR1?.NumCameras == 0)
                return;
            
            _camera = IOManager.CurrentLevelAsTR1?.Cameras[_index];
            
            _x = _camera.X;
            _y = _camera.Y;
            _z = _camera.Z;
            _room = _camera.Room;
            _flags = _camera.Flag;
        }

        public void Apply()
        {
            if (IOManager.CurrentLevelAsTR1 is null)
                return;
            
            if (IOManager.CurrentLevelAsTR1?.NumCameras == 0)
                return;
            
            _camera.X = _x;
            _camera.Y = _y;
            _camera.Z = _z;
            _camera.Room = (short)_room;
            _camera.Flag = (ushort)_flags;
        }
    }
}
