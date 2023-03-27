using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibV64Core
{
    public class Types
    {
        public enum GameState
        {
            Vanilla, Decomp
        }

        #region Camera
        public enum CamMoveFlags : uint
        {
            RETURN_TO_MIDDLE       = 0x0001,
            ZOOMED_OUT             = 0x0002,
            ROTATE_RIGHT           = 0x0004,
            ROTATE_LEFT            = 0x0008,
            ENTERED_ROTATE_SURFACE = 0x0010,
            METAL_BELOW_WATER      = 0x0020,
            FIX_IN_PLACE           = 0x0040,
            UNKNOWN_8              = 0x0080,
            CAM_MOVING_INTO_MODE   = 0x0100,
            STARTED_EXITING_C_UP   = 0x0200,
            UNKNOWN_11             = 0x0400,
            INIT_CAMERA            = 0x0800,
            ALREADY_ZOOMED_OUT     = 0x1000,
            C_UP_MODE              = 0x2000,
            SUBMERGED              = 0x4000,
            PAUSE_SCREEN           = 0x8000,
        }
        #endregion

        #region Mario States
        public enum EyeState {
            BLINKING, OPEN, HALF, CLOSED, LEFT, RIGHT, UP, DOWN, DEAD
        }
        public enum HandState {
            FISTS, OPEN, PEACE, WITH_CAP, WITH_WING_CAP, RIGHT_OPEN
        }
        public enum PowerUpState
        {
            DEFAULT, VANISH, METAL, METAL_VANISH
        }
        #endregion

        #region Color Codes
        public struct ColorCode
        {
            public string Name;
            public ColorPart Shirt;
            public ColorPart Overalls;
            public ColorPart Gloves;
            public ColorPart Shoes;
            public ColorPart Skin;
            public ColorPart Hair;
        }
        public struct ColorPart
        {
            public Light Main;
            public Light Shading;
        }
        public struct Light
        {
            public int R;
            public int G;
            public int B;
        }
        #endregion

        #region Controller

        public enum ButtonFlags : uint
        {
            A_BUTTON        = 0x8000,
            B_BUTTON	    = 0x4000,
            L_TRIG		    = 0x0020,
            R_TRIG		    = 0x0010,
            Z_TRIG		    = 0x2000,
            START_BUTTON	= 0x1000,
            U_JPAD		    = 0x0800,
            L_JPAD		    = 0x0200,
            R_JPAD		    = 0x0100,
            D_JPAD		    = 0x0400,
            U_CBUTTONS	    = 0x0008,
            L_CBUTTONS	    = 0x0004,
            R_CBUTTONS	    = 0x0002,
            D_CBUTTONS	    = 0x0001
        }

        #endregion
    }
}
