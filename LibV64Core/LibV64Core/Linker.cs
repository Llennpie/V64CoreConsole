using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibV64Core
{
    public class Linker
    {
        private static int ParseLinkerAddress(string line)
        {
            int result = int.Parse(line.Substring(28, 6), System.Globalization.NumberStyles.HexNumber);
            return result;
        }

        public struct Map
        {
            /// <summary>
            /// Create a Linker Map from a specified map file
            /// </summary>
            /// <param name="pathToLinkerMap"></param>
            public Map(string pathToLinkerMap) {
                if (File.Exists(pathToLinkerMap))
                {
                    // Begin parsing file
                    foreach (var line in File.ReadAllLines(pathToLinkerMap)) {
                        if (line.Contains("gCameraMovementFlags")) this.CameraMovementFlags = ParseLinkerAddress(line);
                        if (line.Contains("sZoomOutAreaMasks")) this.ZoomOutAreaMasks = ParseLinkerAddress(line);
                        if (line.Contains("mario_reset_bodystate")) this.MarioResetBodystate = ParseLinkerAddress(line);
                        if (line.Contains("gBodyStates")) this.BodyStates = ParseLinkerAddress(line);

                        // Disable Puppycam if it exists
                        if (line.Contains("configPuppyCam"))
                        {
                            int addr = ParseLinkerAddress(line);
                            Memory.WriteBytes(Memory.BaseAddress + (addr + 3) - 3, new byte[] { 0x00 });
                        }
                    }
                }
            }

            public int CameraMovementFlags = 0x33C848;      // gCameraMovementFlags
            public int ZoomOutAreaMasks = 0x32F870;         // sZoomOutAreaMasks
            public int MarioResetBodystate = 0x254338;      // mario_reset_bodystate
            public int BodyStates = 0x33B3B0;               // gBodyStates
        }
    }
}
