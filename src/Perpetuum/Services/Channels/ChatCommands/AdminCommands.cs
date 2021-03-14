using Perpetuum.GenXY;
using System.Collections.Generic;

namespace Perpetuum.Services.Channels.ChatCommands
{
    /*
     * Will contain all Chat Commands available to Admins
     */
    public class AdminCommands
    {
        public static string ZoneAddDecor(int? zoneId, int definition, int x, int y, int z, double qx, double qy, double qz, double qw, double scale, int cat)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>()
                {
                    { "definition", definition },
                    { "x", x*256 },
                    { "y", y*256 },
                    { "z", z*256 },
                    { "quaternionX", qx },
                    { "quaternionY", qy },
                    { "quaternionZ", qz },
                    { "quaternionW", qw },
                    { "scale", scale },
                    { "category", cat }
                };

            /* This foreach is unecessary in this particular case,
             *  But we could allow for certain params to stay null (if they're optional) 
             *  by splitting them into a required and a optional dictionary, validate the params
             *  to then build and return the final result/command 
             */
            foreach (var entry in dictionary)
            {
                if (entry.Value == null)
                {
                    throw PerpetuumException.Create(ErrorCodes.RequiredArgumentIsNotSpecified);
                }
            }

            return string.Format("zoneDecorAdd:zone_{0}:{1}", zoneId, GenxyConverter.Serialize(dictionary));
        }
    }
}
