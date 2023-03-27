using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LibV64Core.Types;

namespace LibV64Core
{
    public class Core
    {
        public static GameState State;

        // Quick Options

        /// <summary>The in-game HUD visibility.</summary>
        public static bool HUD = false;
        /// <summary>The in-game player's shadow visibility.</summary>
        public static bool Shadow = true;

        #region Camera
        public static bool CameraFrozen;
        private static int CamFlags;

        /// <summary>
        /// Freezes/unfreezes the game camera.
        /// </summary>
        public static void ToggleFreezeCamera()
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            if (CameraFrozen)
            {
                // Unfreeze the camera
                Memory.WriteBytes(Memory.BaseAddress + Memory.Map.CameraMovementFlags + 3, new byte[] {0x01});
                Console.WriteLine("[C] Camera Unfrozen");
            }
            else
            {
                // Freeze the camera
                Memory.WriteBytes(Memory.BaseAddress + Memory.Map.CameraMovementFlags + 3, new byte[] { 0x80 });
                Console.WriteLine("[C] Camera Frozen");
            }
        }

        /// <summary>
        /// Zero-fills the camera zoom out flags, allowing the camera to be frozen in any level.
        /// </summary>
        public static void FixCameraZoomOut()
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            Memory.WriteBytes(Memory.BaseAddress + Memory.Map.ZoomOutAreaMasks, new byte[20]);

            // Define movement flags
            CamFlags = Memory.SwapEndian(Memory.ReadBytes(Memory.BaseAddress + Memory.Map.CameraMovementFlags + 3, 1), 4)[0];

            /*
            CamFlags = (int)Types.CamMoveFlags.RETURN_TO_MIDDLE
                + (int)Types.CamMoveFlags.ZOOMED_OUT
                + (int)Types.CamMoveFlags.ROTATE_RIGHT
                + (int)Types.CamMoveFlags.ROTATE_LEFT
                + (int)Types.CamMoveFlags.ENTERED_ROTATE_SURFACE
                + (int)Types.CamMoveFlags.METAL_BELOW_WATER
                + (int)Types.CamMoveFlags.FIX_IN_PLACE
                + (int)Types.CamMoveFlags.UNKNOWN_8
                + (int)Types.CamMoveFlags.CAM_MOVING_INTO_MODE
                + (int)Types.CamMoveFlags.STARTED_EXITING_C_UP
                + (int)Types.CamMoveFlags.UNKNOWN_11
                + (int)Types.CamMoveFlags.INIT_CAMERA
                + (int)Types.CamMoveFlags.ALREADY_ZOOMED_OUT
                + (int)Types.CamMoveFlags.C_UP_MODE
                + (int)Types.CamMoveFlags.SUBMERGED
                + (int)Types.CamMoveFlags.PAUSE_SCREEN;
            */

            Console.WriteLine("[C] Applied Zoom Fix Patch");
        }
        #endregion

        #region Mario States
        /// <summary>
        /// Allows eye/hand/cap states to be manually overwritten.
        /// </summary>
        public static void FixResetBodyState()
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            // Disable on Decomp
            if (Core.State == GameState.Decomp) {
                return;
            }

            // Rewrite the mario_reset_bodystate function
            // This fix stops the game from resetting eye/hand/cap/power states each frame
            string newBodyStateFunction =   "27BDFFF8 8C8E0098 AFAE0004 00000000" +
                                            "00000000 00000000 00000000 00000000" +
                                            "00000000 00000000 8FAF0004 A5E00008" +
                                            "8FB80004 A3000007 8C990004 2401FFBF" +
                                            "03216024 AC884004 10000001 00000000" +
                                            "03E00008 27BD0008";

            // Replace the function with our new one
            Memory.WriteBytes(Memory.BaseAddress + Memory.Map.MarioResetBodystate, Utils.StringToByteArray(newBodyStateFunction), true);

            // Old Zero-Fill
            //Memory.WriteBytes(Memory.BaseAddress + 0x254338, new byte[88]);

            Console.WriteLine("[C] Applied BodyState Patch");
        }

        public static EyeState CurrentEyeState = 0;
        public static HandState CurrentHandState = 0;
        public static PowerUpState CurrentPowerUpState = 0;

        /// <summary>
        /// Sets the current eye state.
        /// </summary>
        /// <param name="eyeState"></param>
        public static void SetEyeState(EyeState eyeState)
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            byte[] writeEyeStateData = BitConverter.GetBytes((int)eyeState);
            Memory.WriteBytes(Memory.BaseAddress + Memory.Map.BodyStates + 6, writeEyeStateData);
            CurrentEyeState = eyeState;
        }

        /// <summary>
        /// Sets the current hand state.
        /// </summary>
        /// <param name="handState"></param>
        public static void SetHandState(HandState handState)
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            byte[] writeHandStateData = BitConverter.GetBytes((int)handState);
            Memory.WriteBytes(Memory.BaseAddress + Memory.Map.BodyStates + 5, writeHandStateData);
            CurrentHandState = handState;

            // SetHandState sometimes overrides the eye state, so we set it once more.
            SetEyeState(CurrentEyeState);
        }

        /// <summary>
        /// Sets the current powerup state.
        /// </summary>
        /// <param name="powerUpState"></param>
        public static void SetPowerUpState(PowerUpState powerUpState)
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            // Don't use flags or state data, set to exact value
            switch(powerUpState)
            {
                case PowerUpState.METAL:
                    Memory.WriteBytes(Memory.BaseAddress + 0x33B177 - 3, new byte[] { 0x14 });
                    break;

                case PowerUpState.VANISH:
                    Memory.WriteBytes(Memory.BaseAddress + 0x33B177 - 3, new byte[] { 0x12 });
                    break;

                case PowerUpState.METAL_VANISH:
                    Memory.WriteBytes(Memory.BaseAddress + 0x33B177 - 3, new byte[] { 0x16 });
                    break;

                default:
                    Memory.WriteBytes(Memory.BaseAddress + 0x33B177 - 3, new byte[] { 0x10 });
                    break;
            }

            CurrentPowerUpState = powerUpState;

            // SetPowerUpState sometimes overrides the hand state, so we set it once more.
            SetHandState(CurrentHandState);
        }
        #endregion

        #region Color Codes
        /// <summary>
        /// Applies a color code to the game.
        /// </summary>
        /// <param name="colorCode"></param>
        public static void ApplyColorCode(ColorCode colorCode)
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            // Disable on Decomp
            if (Core.State == GameState.Decomp) {
                Console.WriteLine("[C] This feature is only available on non-Decomp ROMs");
                return;
            }

            Utils.ApplyLightToAddress(0x07EC40, colorCode.Shirt.Main);
            Utils.ApplyLightToAddress(0x07EC38, colorCode.Shirt.Shading);
            Utils.ApplyLightToAddress(0x07EC28, colorCode.Overalls.Main);
            Utils.ApplyLightToAddress(0x07EC20, colorCode.Overalls.Shading);
            Utils.ApplyLightToAddress(0x07EC58, colorCode.Gloves.Main);
            Utils.ApplyLightToAddress(0x07EC50, colorCode.Gloves.Shading);
            Utils.ApplyLightToAddress(0x07EC70, colorCode.Shoes.Main);
            Utils.ApplyLightToAddress(0x07EC68, colorCode.Shoes.Shading);
            Utils.ApplyLightToAddress(0x07EC88, colorCode.Skin.Main);
            Utils.ApplyLightToAddress(0x07EC80, colorCode.Skin.Shading);
            Utils.ApplyLightToAddress(0x07ECA0, colorCode.Hair.Main);
            Utils.ApplyLightToAddress(0x07EC98, colorCode.Hair.Shading);
            if (string.IsNullOrEmpty(colorCode.Name)) colorCode.Name = "Color Code";

            Console.WriteLine("[C] Applied " + colorCode.Name);
        }

        /// <summary>
        /// Returns the current in-game color code as a ColorCode object.
        /// </summary>
        /// <returns>ColorCode</returns>
        public static ColorCode LoadColorCodeFromGame()
        {
            ColorCode colorCode = new ColorCode();

            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return colorCode;

            // Begin building color code.

            colorCode.Shirt.Main = Utils.LoadLightFromAddress(0x07EC40);
            colorCode.Shirt.Shading = Utils.LoadLightFromAddress(0x07EC38);
            colorCode.Overalls.Main = Utils.LoadLightFromAddress(0x07EC28);
            colorCode.Overalls.Shading = Utils.LoadLightFromAddress(0x07EC20);
            colorCode.Gloves.Main = Utils.LoadLightFromAddress(0x07EC58);
            colorCode.Gloves.Shading = Utils.LoadLightFromAddress(0x07EC50);
            colorCode.Shoes.Main = Utils.LoadLightFromAddress(0x07EC70);
            colorCode.Shoes.Shading = Utils.LoadLightFromAddress(0x07EC68);
            colorCode.Skin.Main = Utils.LoadLightFromAddress(0x07EC88);
            colorCode.Skin.Shading = Utils.LoadLightFromAddress(0x07EC80);
            colorCode.Hair.Main = Utils.LoadLightFromAddress(0x07ECA0);
            colorCode.Hair.Shading = Utils.LoadLightFromAddress(0x07EC98);

            return colorCode;
        }

        /// <summary>
        /// Resets all colors back to default.
        /// </summary>
        public static void ResetColorCode()
        {
            ColorCode defaultColorCode = GameSharkToColorCode("8107EC40 FF00\n" +  // Default color code
                                                              "8107EC42 0000\n" +
                                                              "8107EC38 7F00\n" +
                                                              "8107EC3A 0000\n" +
                                                              "8107EC28 0000\n" +
                                                              "8107EC2A FF00\n" +
                                                              "8107EC20 0000\n" +
                                                              "8107EC22 7F00\n" +
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
            defaultColorCode.Name = "Mario";
            ApplyColorCode(defaultColorCode);
        }

        /// <summary>
        /// Converts a GameShark color code string to a ColorCode object.
        /// </summary>
        /// <param name="gameshark"></param>
        /// <returns>ColorCode</returns>
        public static ColorCode GameSharkToColorCode(string gameshark)
        {
            ColorCode colorCode = new ColorCode();

            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0 || gameshark == null)
                return colorCode;

            // Begin building color code.
            colorCode.Shirt.Main.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC40 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shirt.Main.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC40 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shirt.Main.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC42 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shirt.Shading.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC38 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shirt.Shading.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC38 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shirt.Shading.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC3A ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Overalls.Main.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC28 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Overalls.Main.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC28 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Overalls.Main.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC2A ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Overalls.Shading.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC20 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Overalls.Shading.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC20 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Overalls.Shading.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC22 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Gloves.Main.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC58 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Gloves.Main.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC58 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Gloves.Main.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC5A ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Gloves.Shading.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC50 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Gloves.Shading.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC50 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Gloves.Shading.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC52 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shoes.Main.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC70 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shoes.Main.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC70 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shoes.Main.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC72 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shoes.Shading.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC68 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shoes.Shading.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC68 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Shoes.Shading.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC6A ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Skin.Main.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC88 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Skin.Main.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC88 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Skin.Main.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC8A ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Skin.Shading.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC80 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Skin.Shading.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC80 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Skin.Shading.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC82 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Hair.Main.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7ECA0 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Hair.Main.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7ECA0 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Hair.Main.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7ECA2 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Hair.Shading.R = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC98 ") + 6, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Hair.Shading.G = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC98 ") + 8, 2), System.Globalization.NumberStyles.HexNumber);
            colorCode.Hair.Shading.B = Int32.Parse(gameshark.Substring(gameshark.IndexOf("7EC9A ") + 6, 2), System.Globalization.NumberStyles.HexNumber);

            return colorCode;
        }

        /// <summary>
        /// Converts a ColorCode object to a GameShark color code string.
        /// </summary>
        /// <param name="colorCode"></param>
        /// <returns>string</returns>
        public static string ColorCodeToGameShark(ColorCode colorCode)
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return "";

            string gameshark = "8107EC40 " + colorCode.Shirt.Main.R.ToString("X2") + colorCode.Shirt.Main.G.ToString("X2") + "\n";
            gameshark += "8107EC42 " + colorCode.Shirt.Main.B.ToString("X2") + "00\n";
            gameshark += "8107EC38 " + colorCode.Shirt.Shading.R.ToString("X2") + colorCode.Shirt.Shading.G.ToString("X2") + "\n";
            gameshark += "8107EC3A " + colorCode.Shirt.Shading.B.ToString("X2") + "00\n";
            gameshark += "8107EC28 " + colorCode.Overalls.Main.R.ToString("X2") + colorCode.Overalls.Main.G.ToString("X2") + "\n";
            gameshark += "8107EC2A " + colorCode.Overalls.Main.B.ToString("X2") + "00\n";
            gameshark += "8107EC20 " + colorCode.Overalls.Shading.R.ToString("X2") + colorCode.Overalls.Shading.G.ToString("X2") + "\n";
            gameshark += "8107EC22 " + colorCode.Overalls.Shading.B.ToString("X2") + "00\n";
            gameshark += "8107EC58 " + colorCode.Gloves.Main.R.ToString("X2") + colorCode.Gloves.Main.G.ToString("X2") + "\n";
            gameshark += "8107EC5A " + colorCode.Gloves.Main.B.ToString("X2") + "00\n";
            gameshark += "8107EC50 " + colorCode.Gloves.Shading.R.ToString("X2") + colorCode.Gloves.Shading.G.ToString("X2") + "\n";
            gameshark += "8107EC52 " + colorCode.Gloves.Shading.B.ToString("X2") + "00\n";
            gameshark += "8107EC70 " + colorCode.Shoes.Main.R.ToString("X2") + colorCode.Shoes.Main.G.ToString("X2") + "\n";
            gameshark += "8107EC72 " + colorCode.Shoes.Main.B.ToString("X2") + "00\n";
            gameshark += "8107EC68 " + colorCode.Shoes.Shading.R.ToString("X2") + colorCode.Shoes.Shading.G.ToString("X2") + "\n";
            gameshark += "8107EC6A " + colorCode.Shoes.Shading.B.ToString("X2") + "00\n";
            gameshark += "8107EC88 " + colorCode.Skin.Main.R.ToString("X2") + colorCode.Skin.Main.G.ToString("X2") + "\n";
            gameshark += "8107EC8A " + colorCode.Skin.Main.B.ToString("X2") + "00\n";
            gameshark += "8107EC80 " + colorCode.Skin.Shading.R.ToString("X2") + colorCode.Skin.Shading.G.ToString("X2") + "\n";
            gameshark += "8107EC82 " + colorCode.Skin.Shading.B.ToString("X2") + "00\n";
            gameshark += "8107ECA0 " + colorCode.Hair.Main.R.ToString("X2") + colorCode.Hair.Main.G.ToString("X2") + "\n";
            gameshark += "8107ECA2 " + colorCode.Hair.Main.B.ToString("X2") + "00\n";
            gameshark += "8107EC98 " + colorCode.Hair.Shading.R.ToString("X2") + colorCode.Hair.Shading.G.ToString("X2") + "\n";
            gameshark += "8107EC9A " + colorCode.Hair.Shading.B.ToString("X2") + "00";

            return gameshark;
        }
        #endregion

        /// <summary>
        /// The main update function - should be called in a loop
        /// </summary>
        public static void CoreUpdate()
        {
            // If the camera contains 0x80 (the pause movement flag), the camera is already frozen
            CamFlags = BitConverter.ToUInt16(Memory.ReadBytes(Memory.BaseAddress + Memory.Map.CameraMovementFlags + 2, 2));
            CameraFrozen = ((Types.CamMoveFlags)CamFlags).HasFlag(Types.CamMoveFlags.PAUSE_SCREEN);

            if (CameraFrozen) {
                // Prevent the freeze camera from getting stuck
                if (((Types.CamMoveFlags)CamFlags).HasFlag(Types.CamMoveFlags.STARTED_EXITING_C_UP)) {
                    Memory.WriteBytes(Memory.BaseAddress + Memory.Map.CameraMovementFlags + 3, new byte[] { 0x80 });
                }
                // Re-align our movement perpendicular
                Memory.WriteBytes(Memory.BaseAddress + 0x33C77C + 3, new byte[] { 0x02 });
            }

            // HUD visibility
            byte[] livesBytes = (Core.HUD ? Utils.StringToByteArray("2C0000002A00000025640000") : new byte[12]);
            byte[] coinBytes = (Core.HUD ? Utils.StringToByteArray("2B0000002A00000025640000") : new byte[12]);
            byte[] starBytes = (Core.HUD ? Utils.StringToByteArray("2D0000002A00000025640000") : new byte[12]);
            byte[] powerBytes = (Core.HUD ? Utils.StringToByteArray("0100008C") : Utils.StringToByteArray("0100FF00"));
            byte[] cameraBytes = (Core.HUD ? Utils.StringToByteArray("0C0B8ECF") : new byte[] { 0, 0, 0, 0 });
            Memory.WriteBytes(Memory.BaseAddress + 0x338380, livesBytes, true);
            Memory.WriteBytes(Memory.BaseAddress + 0x33838C, coinBytes, true);
            Memory.WriteBytes(Memory.BaseAddress + 0x338398, starBytes, true);
            Memory.WriteBytes(Memory.BaseAddress + 0x3325F0, powerBytes, true);
            Memory.WriteBytes(Memory.BaseAddress + 0x2E3E18, cameraBytes, true);

            // Shadow visibility
            byte[] shadowBytes = (Core.Shadow) ? new byte[] { 0x00 } : new byte[] { 0x01 };
            Memory.WriteBytes(Memory.BaseAddress + 0x0F0860 + 3, shadowBytes);
        }
    }
}
