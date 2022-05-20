using Newtonsoft.Json;
using System.IO;

namespace TREnvironmentEditor.Model.Types
{
    public abstract class BaseEMRoomImportFunction : BaseEMFunction
    {
        protected static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public string LevelID { get; set; }

        protected string ReadRoomResource(string versionID)
        {
            string path = string.Format(@"Resources\{0}\Rooms\{1}-Rooms.json", versionID, LevelID);
            if (!File.Exists(path))
            {
                throw new IOException("Missing room definition data: " + path);
            }

            return File.ReadAllText(path);
        }

        protected string ReadZoningResource(string versionID)
        {
            string path = string.Format(@"Resources\{0}\Rooms\{1}-Zoning.json", versionID, LevelID);
            if (!File.Exists(path))
            {
                throw new IOException("Missing room zoning data: " + path);
            }

            return File.ReadAllText(path);
        }
    }
}