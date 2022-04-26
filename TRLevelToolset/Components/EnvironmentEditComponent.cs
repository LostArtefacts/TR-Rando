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
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("All Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _allControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.ConditionalAll.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("Conditional All Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _conditionalAllControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.NonPurist.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("Non Purist Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _nonPuristControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.Any.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("Any Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _anyControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.AllWithin.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("All Within Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _allWithinControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.ConditionalAllWithin.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("Conditional All Within Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _conditionalAllWithinControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.OneOf.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("One Of Properties", ImGuiWindowFlags.HorizontalScrollbar);
                        _oneOfControl.Draw();
                        ImGui.End();
                    }
                    
                    if (_mapping.Mirrored.Any())
                    {
                        ImGui.SetNextWindowSize(new Vector2(400, 600));
                        ImGui.Begin("Mirrored Properties", ImGuiWindowFlags.HorizontalScrollbar);
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
            _allControl = new EMAllControl(_mapping.All);
            _conditionalAllControl = new EMConditionalAllControl(_mapping.ConditionalAll);
            _nonPuristControl = new EMNonPuristControl(_mapping.NonPurist);
            _anyControl = new EMAnyControl(_mapping.Any);
            _allWithinControl = new EMAllWithinControl(_mapping.AllWithin);
            _conditionalAllWithinControl = new EMConditionalAllWithinControl(_mapping.ConditionalAllWithin);
            _oneOfControl = new EMOneOfControl(_mapping.OneOf);
            _mirroredControl = new EMMirroredControl(_mapping.Mirrored);
        }
    }
}
