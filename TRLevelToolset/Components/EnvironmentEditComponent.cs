using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor;
using TRLevelToolset.Interfaces;
using TRLevelToolset.Controls.DataControls.EM;

namespace TRLevelToolset.Components
{
    public class EnvironmentEditComponent : IDrawable
    {
        private int _selectedIndex;
        private string[] _items;
        private string _name;
        private EMEditorMapping _mapping;

        private EMAllControl _allControl;
        private EMConditionalAllControl _conditionalAllControl;
        private EMNonPuristControl _nonPuristControl;
        private EMAnyControl _anyControl;
        private EMAllWithinControl _allWithinControl;
        private EMConditionalAllWithinControl _conditionalAllWithinControl;
        private EMOneOfControl _oneOfControl;
        private EMMirroredControl _mirroredControl;
        
        public void Draw()
        {
            _items = Directory.GetFiles("Environments");
            
            if (ImGui.TreeNodeEx("Environment Tools", ImGuiTreeNodeFlags.Framed))
            {
                if (ImGui.BeginListBox(""))
                {
                    for (int i = 0; i < _items.Count(); i++)
                    {
                        bool isSelected = (_selectedIndex == i);

                        if (ImGui.Selectable(_items[i], isSelected))
                            _selectedIndex = i;

                        if (isSelected)
                            ImGui.SetItemDefaultFocus();
                    }

                    ImGui.EndListBox();
                }

                if (ImGui.Button("Load"))
                {
                    Load();
                }

                if (_mapping is not null)
                {
                    if (_mapping.All.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("All Properties");
                        _allControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.ConditionalAll.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("Conditional All Properties");
                        _conditionalAllControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.NonPurist.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("Non Purist Properties");
                        _nonPuristControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.Any.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("Any Properties");
                        _anyControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.AllWithin.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("All Within Properties");
                        _allWithinControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.ConditionalAllWithin.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("Conditional All Within Properties");
                        _conditionalAllWithinControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.OneOf.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("One Of Properties");
                        _oneOfControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.Mirrored.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 800));
                        ImGui.Begin("Mirrored Properties");
                        _mirroredControl.Draw();
                        ImGui.End();
                    }
                }

                ImGui.TreePop();
            }
        }

        private void Load()
        {
            _name = _items[_selectedIndex];
            _mapping = EMEditorMapping.Get(_name);
            _allControl = new EMAllControl() { Data = _mapping.All };
            _conditionalAllControl = new EMConditionalAllControl() { Data = _mapping.ConditionalAll };
            _nonPuristControl = new EMNonPuristControl() { Data = _mapping.NonPurist };
            _anyControl = new EMAnyControl() { Data = _mapping.Any };
            _allWithinControl = new EMAllWithinControl() { Data = _mapping.AllWithin };
            _conditionalAllWithinControl = new EMConditionalAllWithinControl() { Data = _mapping.ConditionalAllWithin };
            _oneOfControl = new EMOneOfControl() { Data = _mapping.OneOf };
            _mirroredControl = new EMMirroredControl() { Data = _mapping.Mirrored };
        }
    }
}
