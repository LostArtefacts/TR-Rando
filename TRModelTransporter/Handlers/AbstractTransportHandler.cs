using TRLevelReader.Model;
using TRModelTransporter.Model;

namespace TRModelTransporter.Handlers
{
    public abstract class AbstractTransportHandler
    {
        public TRModelDefinition Definition { get; set; }
        public TR2Level Level { get; set; }

        public abstract void Export();
        public abstract void Import();
    }
}