using System.Numerics;

namespace TRRandomizerCore.Helpers;

public class Sphere
{
    public Vector3 Centre { get; set; }

    public float Radius { get; set; }

    public Sphere(Vector3 centre, float radius)
    {
        Centre = centre;
        Radius = radius;
    }

    public bool IsColliding(Sphere other)
    {
        Vector3 distance = Centre - other.Centre;

        float length = (float)Math.Sqrt((distance.X * distance.X) + (distance.Y * distance.Y) + (distance.Z * distance.Z));

        float sumRadius = Radius + other.Radius;

        return (length < sumRadius);
    }
}
