using TRLevelControl.Model;

namespace TRLevelControl.Build;

public interface IMeshProvider
{
    TRMesh GetObjectMesh(long pointer);
}
