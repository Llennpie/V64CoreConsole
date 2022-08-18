using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibV64Core
{
    public class Controller
    {
        private static int CurrentButtonFlags;
        public static bool alreadyPressed;

        /// <summary>
        /// Returns true if the button scheme was just pressed
        /// </summary>
        /// <param name="buttonFlags"></param>
        /// <returns></returns>
        public static bool GetButton(Types.ButtonFlags buttonFlags)
        {
            if (alreadyPressed) return false;

            CurrentButtonFlags = BitConverter.ToUInt16(Memory.ReadBytes(Memory.BaseAddress + 0x33AFA2, 2));
            if (((Types.ButtonFlags)CurrentButtonFlags).HasFlag(buttonFlags))
            {
                alreadyPressed = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the button scheme is being held
        /// </summary>
        /// <param name="buttonFlags"></param>
        /// <returns></returns>
        public static bool GetButtonHeld(Types.ButtonFlags buttonFlags)
        {
            CurrentButtonFlags = BitConverter.ToUInt16(Memory.ReadBytes(Memory.BaseAddress + 0x33AFA2, 2));
            if (((Types.ButtonFlags)CurrentButtonFlags).HasFlag(buttonFlags))
                return true;

            return false;
        }

        /// <summary>
        /// Controller update function. Should be called in a Task.Run() loop (not CoreUpdate)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Task Update(int time = 500)
        {
            while (true)
            {
                Thread.Sleep(time);
                alreadyPressed = false;
            }

            return Task.CompletedTask;
        }
    }
}
