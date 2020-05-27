using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://trwiki.earvillage.net/doku.php?id=trsone

namespace TRLevelReader.Model
{
    public class TR2Level
    {
        /*
            uint32_t Version; // version (4 bytes)
            tr_colour Palette[256]; // 8-bit palette (768 bytes)
            tr_colour4 Palette16[256]; //  (1024 bytes)
            uint32_t NumImages; // number of texture images (4 bytes)
            tr_teximage8 Images8[NumImages]; // 8-bit (palettized) images (NumImages * 65536 bytes)
            tr_teximage16 Images16[NumImages]; // 16-bit (ARGB) images (NumImages * 131072 bytes)
            uint32_t Unused; // 32-bit unused value (4 bytes)
            uint16_t NumRooms; // number of rooms (2 bytes)
            tr2_room Rooms[NumRooms]; // room list (variable length)
            uint32_t NumFloorData; // number of floor data uint16_t's to follow (4 bytes)
            uint16_t FloorData[NumFloorData]; // floor data (NumFloorData * 2 bytes)
            uint32_t NumMeshData; // number of uint16_t's of mesh data to follow (=Meshes[]) (4 bytes)
            tr_mesh Meshes[NumMeshPointers]; // note that NumMeshPointers comes AFTER Meshes[]
            uint32_t NumMeshPointers; // number of mesh pointers to follow (4 bytes)
            uint32_t MeshPointers[NumMeshPointers]; // mesh pointer list (NumMeshPointers * 4 bytes)
            uint32_t NumAnimations; // number of animations to follow (4 bytes)
            tr_animation Animations[NumAnimations]; // animation list (NumAnimations * 32 bytes)
            uint32_t NumStateChanges; // number of state changes to follow (4 bytes)
            tr_state_change StateChanges[NumStateChanges]; // state-change list (NumStructures * 6 bytes)
            uint32_t NumAnimDispatches; // number of animation dispatches to follow (4 bytes)
            tr_anim_dispatch AnimDispatches[NumAnimDispatches]; // animation-dispatch list list (NumAnimDispatches * 8 bytes)
            uint32_t NumAnimCommands; // number of animation commands to follow (4 bytes)
            tr_anim_command AnimCommands[NumAnimCommands]; // animation-command list (NumAnimCommands * 2 bytes)
            uint32_t NumMeshTrees; // number of MeshTrees to follow (4 bytes)
            tr_meshtree_node MeshTrees[NumMeshTrees]; // MeshTree list (NumMeshTrees * 4 bytes)
            uint32_t NumFrames; // number of words of frame data to follow (4 bytes)
            uint16_t Frames[NumFrames]; // frame data (NumFrames * 2 bytes)
            uint32_t NumModels; // number of models to follow (4 bytes)
            tr_model Models[NumModels]; // model list (NumModels * 18 bytes)
            uint32_t NumStaticMeshes; // number of StaticMesh data records to follow (4 bytes)
            tr_staticmesh StaticMeshes[NumStaticMeshes]; // StaticMesh data (NumStaticMesh * 32 bytes)
            uint32_t NumObjectTextures; // number of object textures to follow (4 bytes)
            tr_object_texture ObjectTextures[NumObjectTextures]; // object texture list (NumObjectTextures * 20 bytes) (after AnimatedTextures in TR3)
            uint32_t NumSpriteTextures; // number of sprite textures to follow (4 bytes)
            tr_sprite_texture SpriteTextures[NumSpriteTextures]; // sprite texture list (NumSpriteTextures * 16 bytes)
            uint32_t NumSpriteSequences; // number of sprite sequences records to follow (4 bytes)
            tr_sprite_sequence SpriteSequences[NumSpriteSequences]; // sprite sequence data (NumSpriteSequences * 8 bytes)
            uint32_t NumCameras; // number of camera data records to follow (4 bytes)
            tr_camera Cameras[NumCameras]; // camera data (NumCameras * 16 bytes)
            uint32_t NumSoundSources; // number of sound source data records to follow (4 bytes)
            tr_sound_source SoundSources[NumSoundSources]; // sound source data (NumSoundSources * 16 bytes)
            uint32_t NumBoxes; // number of box data records to follow (4 bytes)
            tr2_box Boxes[NumBoxes]; // box data (NumBoxes * 8 bytes)
            uint32_t NumOverlaps; // number of overlap records to follow (4 bytes)
            uint16_t Overlaps[NumOverlaps]; // overlap data (NumOverlaps * 2 bytes)
            int16_t Zones[10*NumBoxes]; // zone data (NumBoxes * 20 bytes)
            uint32_t NumAnimatedTextures; // number of animated texture records to follow (4 bytes)
            uint16_t AnimatedTextures[NumAnimatedTextures]; // animated texture data (NumAnimatedTextures * 2 bytes)
            uint32_t NumEntities; // number of entities to follow (4 bytes)
            tr2_entity Entities[NumEntities]; // entity list (NumEntities * 24 bytes)
            uint8_t LightMap[32 * 256]; // light map (8192 bytes)
            uint16_t NumCinematicFrames; // number of cinematic frame records to follow (2 bytes)
            tr_cinematic_frame CinematicFrames[NumCinematicFrames]; // (NumCinematicFrames * 16 bytes)
            uint16_t NumDemoData; // number of demo data records to follow (2 bytes)
            uint8_t DemoData[NumDemoData]; // demo data (NumDemoData bytes)
            int16_t SoundMap[370]; // sound map (740 bytes)
            uint32_t NumSoundDetails; // number of sound-detail records to follow (4 bytes)
            tr_sound_details SoundDetails[NumSoundDetails]; // sound-detail list (NumSoundDetails * 8 bytes)
            uint32_t NumSampleIndices; // number of sample indices to follow (4 bytes)
            uint32_t SampleIndices[NumSampleIndices]; // sample indices (NumSampleIndices * 4 bytes)
         */
    }
}
