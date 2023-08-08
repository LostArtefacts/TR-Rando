using System.Numerics;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMGenerateLightFunction : BaseEMFunction
{
    public List<short> RoomIndices { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        foreach (short roomIndex in RoomIndices)
        {
            TRRoom room = level.Rooms[data.ConvertRoom(roomIndex)];
            if (room.NumLights == 0)
            {
                continue;
            }

            Dictionary<TRRoomLight, Vector3> lightPositions = new Dictionary<TRRoomLight, Vector3>();
            foreach (TRRoomVertex vertex in room.RoomData.Vertices)
            {
                // Several lights per room - for now just use whichever is nearest this point
                Vector3 vertexPosition = new Vector3(vertex.Vertex.X, vertex.Vertex.Y, vertex.Vertex.Z);
                double smallestDistance = double.MaxValue;
                TRRoomLight nearestLight = room.Lights[0];
                foreach (TRRoomLight light in room.Lights)
                {
                    if (!lightPositions.ContainsKey(light))
                    {
                        lightPositions[light] = new Vector3(light.X - room.Info.X, light.Y, light.Z - room.Info.Z);
                    }

                    Vector3 diff = vertexPosition - lightPositions[light];
                    double distance = Math.Sqrt(Math.Pow(diff.X, 2) + Math.Pow(diff.Y, 2) + Math.Pow(diff.Z, 2));
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        nearestLight = light;
                    }
                }

                vertex.Lighting = room.ContainsWater ? (short)(8192 - nearestLight.Intensity) :
                    GenerateLight(nearestLight.Intensity, nearestLight.Fade, smallestDistance);
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        foreach (short roomIndex in RoomIndices)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
            if (room.NumLights == 0)
            {
                continue;
            }

            Dictionary<TR2RoomLight, Vector3> lightPositions = new Dictionary<TR2RoomLight, Vector3>();
            foreach (TR2RoomVertex vertex in room.RoomData.Vertices)
            {
                // Several lights per room - for now just use whichever is nearest this point
                Vector3 vertexPosition = new Vector3(vertex.Vertex.X, vertex.Vertex.Y, vertex.Vertex.Z);
                double smallestDistance = double.MaxValue;
                TR2RoomLight nearestLight = room.Lights[0];
                foreach (TR2RoomLight light in room.Lights)
                {
                    if (!lightPositions.ContainsKey(light))
                    {
                        lightPositions[light] = new Vector3(light.X - room.Info.X, light.Y, light.Z - room.Info.Z);
                    }

                    Vector3 diff = vertexPosition - lightPositions[light];
                    double distance = Math.Sqrt(Math.Pow(diff.X, 2) + Math.Pow(diff.Y, 2) + Math.Pow(diff.Z, 2));
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        nearestLight = light;
                    }
                }

                vertex.Lighting = room.ContainsWater ? (short)(8192 - nearestLight.Intensity1) :
                    GenerateLight(nearestLight.Intensity1, nearestLight.Fade1, smallestDistance);
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        foreach (short roomIndex in RoomIndices)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
            if (room.NumLights == 0)
            {
                continue;
            }

            Dictionary<TR3RoomLight, Vector3> lightPositions = new Dictionary<TR3RoomLight, Vector3>();
            foreach (TR3RoomVertex vertex in room.RoomData.Vertices)
            {
                // Several lights per room - for now just use whichever is nearest this point
                Vector3 vertexPosition = new Vector3(vertex.Vertex.X, vertex.Vertex.Y, vertex.Vertex.Z);
                double smallestDistance = double.MaxValue;
                TR3RoomLight nearestLight = null;
                foreach (TR3RoomLight light in room.Lights)
                {
                    if (light.LightType == 0)
                    {
                        // Sun light
                        continue;
                    }

                    if (!lightPositions.ContainsKey(light))
                    {
                        lightPositions[light] = new Vector3(light.X - room.Info.X, light.Y, light.Z - room.Info.Z);
                    }

                    Vector3 diff = vertexPosition - lightPositions[light];
                    double distance = Math.Sqrt(Math.Pow(diff.X, 2) + Math.Pow(diff.Y, 2) + Math.Pow(diff.Z, 2));
                    if (distance < smallestDistance)
                    {
                        smallestDistance = distance;
                        nearestLight = light;
                    }
                }

                if (nearestLight != null)
                {
                    vertex.Lighting = GenerateTR3Light(nearestLight.LightProperties[0], nearestLight.LightProperties[2], smallestDistance);                        
                }
            }
        }
    }

    private short GenerateLight(ushort intensity, uint fade, double distance)
    {
        double lighting = intensity;
        lighting *= fade / distance;
        lighting *= 0.4;
        return (short)(8192 - lighting);
    }

    private short GenerateTR3Light(short intensity, short fade, double distance)
    {
        double lighting = intensity;
        lighting *= fade / distance;
        lighting *= 0.4;
        return (short)(8192 - lighting);
    }
}
