using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TRViewInterop.Routes
{
    public static class RouteToLocationsConverter
    {
        public static List<TRViewLocation> Convert(string filepath)
        {
            Route route = JsonConvert.DeserializeObject<Route>(File.ReadAllText(filepath));

            List<TRViewLocation> locations = new List<TRViewLocation>();

            foreach (Waypoint point in route.waypoints)
            {
                string[] components = point.position.Split(',');

                Debug.Assert(components.Count() == 3);

                float X = System.Convert.ToSingle(components[0]);
                float Y = System.Convert.ToSingle(components[1]);
                float Z = System.Convert.ToSingle(components[2]);

                locations.Add(new TRViewLocation
                {
                    X = (int)(X * 1024),
                    Y = (int)(Y * 1024),
                    Z = (int)(Z * 1024),
                    Room = point.room
                });
            }

            return locations;
        }
    }

    public struct TRViewLocation
    {
        public int X;
        public int Y;
        public int Z;
        public int Room;
    }
}
