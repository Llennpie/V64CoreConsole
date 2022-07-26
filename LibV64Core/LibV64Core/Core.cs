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
        #region Camera
        public static bool CameraFrozen;

        /// <summary>
        /// Freezes/unfreezes the game camera.
        /// </summary>
        public static void ToggleFreezeCamera()
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            // First, fetch the address to determine the current state of the camera
            int freezeCameraData = Memory.SwapEndian(Memory.ReadBytes(Memory.BaseAddress + 0x33C84B, 1), 4)[0];

            // If the camera contains 0x80 (the pause movement flag), the camera is already frozen
            CameraFrozen = (freezeCameraData & 128) == 128;

            if (CameraFrozen)
            {
                // Unfreeze the camera
                byte[] writeFreezeData = BitConverter.GetBytes(freezeCameraData - 128);
                Memory.WriteBytes(Memory.BaseAddress + 0x33C84B, writeFreezeData);
                Console.WriteLine("[C] Camera Unfrozen");
            }
            else
            {
                // Freeze the camera
                byte[] writeFreezeData = BitConverter.GetBytes(freezeCameraData + 128);
                Memory.WriteBytes(Memory.BaseAddress + 0x33C84B, writeFreezeData);
                Console.WriteLine("[C] Camera Frozen");
            }

            // One last check for the pause movement flag
            CameraFrozen = (freezeCameraData & 128) == 128;
        }

        /// <summary>
        /// Zero-fills the camera zoom out flags, allowing the camera to be frozen in any level.
        /// </summary>
        public static void FixCameraZoomOut()
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            Memory.WriteBytes(Memory.BaseAddress + 0x32F870, new byte[20]);
        }
        #endregion

        #region Mario States
        /// <summary>
        /// Zero-fills the function "mario_reset_bodystate", allowing eye/hand/cap states to be manually overwritten.
        /// </summary>
        public static void FixResetBodyState()
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            Memory.WriteBytes(Memory.BaseAddress + 0x254338, new byte[88]);
        }

        public static EyeState CurrentEyeState;
        public static HandState CurrentHandState;

        /// <summary>
        /// Sets the current eye state.
        /// </summary>
        /// <param name="eyeState"></param>
        public static void SetEyeState(EyeState eyeState)
        {
            if (!Memory.IsEmulatorOpen || Memory.BaseAddress == 0)
                return;

            byte[] writeEyeStateData = BitConverter.GetBytes((int)eyeState);
            Memory.WriteBytes(Memory.BaseAddress + 0x33B3B6, writeEyeStateData);
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
            Memory.WriteBytes(Memory.BaseAddress + 0x33B3B5, writeHandStateData);
            CurrentHandState = handState;

            // SetHandState sometimes overrides the eye state, so we set it once more.
            SetEyeState(CurrentEyeState);
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
                return null;

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
            gameshark += "8107EC9A " + colorCode.Hair.Shading.B.ToString("X2") + "00\n";

            return gameshark;
        }
        #endregion
    }
}
