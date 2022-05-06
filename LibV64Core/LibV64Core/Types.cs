using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibV64Core
{
    public class Types
    {
        #region Mario States
        public enum EyeState {
            BLINKING, OPEN, HALF, CLOSED, LEFT, RIGHT, UP, DOWN, DEAD
        }
        public enum HandState {
            FISTS, OPEN, PEACE, WITH_CAP, WITH_WING_CAP, RIGHT_OPEN
        }
        #endregion

        #region Color Codes
        public struct ColorCode
        {
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
    }
}
