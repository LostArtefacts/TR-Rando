using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://trwiki.earvillage.net/doku.php?id=trsone

namespace TRLevelReader.Model
{
    public class TR2Level
    {
        public uint Version { get; set; }

        //public TRColour[] Palette { get; set; }

        //public TRColour4[] Palette16 { get; set; }

        public uint NumImages { get; set; }

        //public TRTexImage8[] Images8 { get; set; }

        //public TRTexImage16[] Images16 { get; set; }

        public uint Unused { get; set; }

        public ushort NumRooms { get; set; }

        //public TR2Room[] Rooms { get; set; }

        public uint NumFloorData { get; set; }

        public ushort[] FloorData { get; set; }

        public uint NumMeshData { get; set; }

        //public TRMesh[] Meshes { get; set; }

        public uint NumMeshPointers { get; set; }

        public uint[] MeshPointers { get; set; }

        public uint NumAnimations { get; set; }

        //public TRAnimation[] Animations { get; set; }

        public uint NumStateChanges { get; set; }

        //public TRStateChange[] StateChanges { get; set; }

        public uint NumAnimDispatches { get; set; }

        //public TRAnimDispatch[] AnimDispatches { get; set; }

        public uint NumAnimCommands { get; set; }

        //public TRAnimCommand[] AnimCommands { get; set; }

        public uint NumMeshTrees { get; set; }

        //public TRMeshTreeNode[] MeshTrees { get; set; }

        public uint NumFrames { get; set; }

        public ushort[] Frames { get; set; }

        public uint NumModels { get; set; }

        //public TRModel[] Models { get; set; }

        public uint NumStaticMeshes { get; set; }

        //public TRStaticMesh[] StaticMeshes { get; set; }

        public uint NumObjectTextures { get; set; }

        //public TRObjectTexture[] ObjectTextures { get; set; }

        public TR2Level()
        {

        }
    }
}
