namespace TRLevelToolset.Interfaces;

public interface IModelUpdater
{
    public void Clamp();
    public void Populate();
    public void Apply();
}