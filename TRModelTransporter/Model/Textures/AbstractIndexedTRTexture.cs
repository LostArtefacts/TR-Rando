using Newtonsoft.Json;
using System.Drawing;

namespace TRModelTransporter.Model.Textures;

public abstract class AbstractIndexedTRTexture
{
    protected Rectangle _bounds;
    private int _boundsXDiff, _boundsYDiff;

    public int Index { get; set; }
    public string Classification { get; set; } // a reference to the level the texture originated from
    [JsonIgnore]
    public abstract int Atlas { get; set; }
    [JsonIgnore]
    public int Area => _bounds.Width * _bounds.Height;
    [JsonIgnore]
    public Rectangle Bounds => _bounds;

    public AbstractIndexedTRTexture()
    {
        _boundsXDiff = 0;
        _boundsYDiff = 0;
    }

    public void MoveBy(int xDiff, int yDiff)
    {
        _bounds.X += xDiff;
        _bounds.Y += yDiff;

        _boundsXDiff = xDiff;
        _boundsYDiff = yDiff;
    }

    public void Commit(int tileIndex)
    {
        Atlas = tileIndex;
        ApplyBoundsDiff();
    }

    private void ApplyBoundsDiff()
    {
        if (_boundsXDiff != 0 || _boundsYDiff != 0)
        {
            ApplyBoundDiffToTexture(_boundsXDiff, _boundsYDiff);
            _boundsXDiff = _boundsYDiff = 0; // Don't apply again unless we move again
            //System.Diagnostics.Debug.WriteLine("[" + Index + "] => Atlas " + Atlas + ", " + Bounds);
        }
    }

    public virtual void Invalidate()
    {
        Atlas = ushort.MaxValue;
    }

    public virtual bool IsValid()
    {
        return Atlas != ushort.MaxValue;
    }

    protected abstract void GetBoundsFromTexture();
    protected abstract void ApplyBoundDiffToTexture(int xDiff, int yDiff);

    public abstract AbstractIndexedTRTexture Clone();
}
