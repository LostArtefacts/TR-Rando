namespace TREnvironmentEditor.Helpers
{
    public class EMLevelData
    {
        public uint NumCameras { get; set; }
        public uint NumEntities { get; set; }
        public uint NumRooms { get; set; }

        /// <summary>
        /// Negative values will imply a backwards search against NumCameras.
        /// e.g. camera = -2, NumCameras = 14 => Result = 12
        /// </summary>
        public short ConvertCamera(int camera)
        {
            return Convert(camera, NumCameras);
        }

        /// <summary>
        /// Negative values will imply a backwards search against NumEntities.
        /// e.g. entity = -2, NumEntities = 14 => Result = 12
        /// </summary>
        public short ConvertEntity(int entity)
        {
            return Convert(entity, NumEntities);
        }

        /// <summary>
        /// Negative values will imply a backwards search against NumRoows.
        /// e.g. room = -2, NumRooms = 14 => Result = 12
        /// </summary>
        public short ConvertRoom(int room)
        {
            return Convert(room, NumRooms);
        }

        public short Convert(int itemIndex, uint numItems)
        {
            return (short)(itemIndex < 0 ? numItems + itemIndex : itemIndex);
        }
    }
}