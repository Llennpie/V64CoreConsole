using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibV64Core;

namespace V64CoreConsole
{
    internal class Commands
    {
        public static void CreateWorkspace()
        {
            // Create "colorcodes" folder if it doesn't exist
            if (!System.IO.Directory.Exists("colorcodes"))
            {
                System.IO.Directory.CreateDirectory("colorcodes");
                System.IO.File.WriteAllText("colorcodes\\SMG4.gs", "8107EC40 0000\n" +  // SMG4's color code
                                                                   "8107EC42 BB00\n" +
                                                                   "8107EC38 0000\n" +
                                                                   "8107EC3A BB00\n" +
                                                                   "8107EC28 9CB0\n" +
                                                                   "8107EC2A BB00\n" +
                                                                   "8107EC20 9CB0\n" +
                                                                   "8107EC22 BB00\n" +
                                                                   "8107EC58 FFFF\n" +
                                                                   "8107EC5A FF00\n" +
                                                                   "8107EC50 7F7F\n" +
                                                                   "8107EC52 7F00\n" +
                                                                   "8107EC70 721C\n" +
                                                                   "8107EC72 0E00\n" +
                                                                   "8107EC68 390E\n" +
                                                                   "8107EC6A 0700\n" +
                                                                   "8107EC88 FEC1\n" +
                                                                   "8107EC8A 7900\n" +
                                                                   "8107EC80 7F60\n" +
                                                                   "8107EC82 3C00\n" +
                                                                   "8107ECA0 7306\n" +
                                                                   "8107ECA2 0000\n" +
                                                                   "8107EC98 3903\n" +
                                                                   "8107EC9A 0000");
            }
        }

        #region Commands
        public static void EyeSwap(string chosenEyeName)
        {
            switch (chosenEyeName)
            {
                case "blink":
                    Core.SetEyeState(Types.EyeState.BLINKING);
                    break;
                case "open":
                    Core.SetEyeState(Types.EyeState.OPEN);
                    break;
                case "half":
                    Core.SetEyeState(Types.EyeState.HALF);
                    break;
                case "closed":
                    Core.SetEyeState(Types.EyeState.CLOSED);
                    break;
                case "left":
                    Core.SetEyeState(Types.EyeState.LEFT);
                    break;
                case "right":
                    Core.SetEyeState(Types.EyeState.RIGHT);
                    break;
                case "up":
                    Core.SetEyeState(Types.EyeState.UP);
                    break;
                case "down":
                    Core.SetEyeState(Types.EyeState.DOWN);
                    break;
                case "dead":
                    Core.SetEyeState(Types.EyeState.DEAD);
                    break;
                default:
                    Core.SetEyeState(Types.EyeState.BLINKING);
                    break;
            }
        }

        public static void HandSwap(string chosenHandName)
        {
            switch (chosenHandName)
            {
                case "fists":
                    Core.SetHandState(Types.HandState.FISTS);
                    break;
                case "open":
                    Core.SetHandState(Types.HandState.OPEN);
                    break;
                case "peace":
                    Core.SetHandState(Types.HandState.PEACE);
                    break;
                case "cap":
                    Core.SetHandState(Types.HandState.WITH_CAP);
                    break;
                case "wingcap":
                    Core.SetHandState(Types.HandState.WITH_WING_CAP);
                    break;
                case "right":
                    Core.SetHandState(Types.HandState.RIGHT_OPEN);
                    break;
                default:
                    Core.SetHandState(Types.HandState.FISTS);
                    break;
            }
        }

        public static void PowerUpSwap(string chosenPowerUpName)
        {
            switch (chosenPowerUpName)
            {
                case "default":
                    Core.SetPowerUpState(Types.PowerUpState.DEFAULT);
                    break;
                case "vanish":
                    Core.SetPowerUpState(Types.PowerUpState.VANISH);
                    break;
                case "metal":
                    Core.SetPowerUpState(Types.PowerUpState.METAL);
                    break;
                default:
                    Core.SetPowerUpState(Types.PowerUpState.DEFAULT);
                    break;
            }
        }
        #endregion
    }
}
