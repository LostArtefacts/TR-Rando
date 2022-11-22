using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAdjustVisibilityPortalFunction : BaseEMFunction
    {
        public short BaseRoom { get; set; }
        public short AdjoiningRoom { get; set; }
        public Dictionary<int, TRVertex> VertexChanges { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = new EMLevelData { NumRooms = level.NumRooms };
            AdjustPortal(Array.Find(level.Rooms[data.ConvertRoom(BaseRoom)].Portals, p => p.AdjoiningRoom == data.ConvertRoom(AdjoiningRoom)));
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = new EMLevelData { NumRooms = level.NumRooms };
            AdjustPortal(Array.Find(level.Rooms[data.ConvertRoom(BaseRoom)].Portals, p => p.AdjoiningRoom == data.ConvertRoom(AdjoiningRoom)));
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = new EMLevelData { NumRooms = level.NumRooms };
            AdjustPortal(Array.Find(level.Rooms[data.ConvertRoom(BaseRoom)].Portals, p => p.AdjoiningRoom == data.ConvertRoom(AdjoiningRoom)));
        }

        private void AdjustPortal(TRRoomPortal portal)
        {
            if (portal == null)
            {
                return;
            }

            foreach (int vertexIndex in VertexChanges.Keys)
            {
                TRVertex currentVertex = portal.Vertices[vertexIndex];
                TRVertex vertexChange = VertexChanges[vertexIndex];
                currentVertex.X += vertexChange.X;
                currentVertex.Y += vertexChange.Y;
                currentVertex.Z += vertexChange.Z;
            }
        }
    }
}
