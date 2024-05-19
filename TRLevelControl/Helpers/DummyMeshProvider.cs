using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public class DummyMeshProvider : IMeshProvider
{
    private static readonly TRMesh _dummy = new();

    public TRMesh GetObjectMesh(long pointer)
        => _dummy;
}
