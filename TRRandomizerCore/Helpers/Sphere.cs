using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace TRRandomizerCore.Helpers;

public class Sphere
{
    public Vector3 centre { get; set; }

    public float radius { get; set; }

    public Sphere(Vector3 centre, float radius)
    {
        this.centre = centre;
        this.radius = radius;
    }

    public bool IsColliding(Sphere other)
    {
        Vector3 distance = this.centre - other.centre;

        float length = (float)Math.Sqrt((distance.X * distance.X) + (distance.Y * distance.Y) + (distance.Z * distance.Z));

        float sumRadius = this.radius + other.radius;

        return (length < sumRadius);
    }
}
