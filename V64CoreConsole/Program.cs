using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibV64Core;

namespace V64CoreConsole
{
    class Program
    {
        private static string? command;

        static void Main(string[] args)
        {
            Console.Title = "V64CoreConsole";

            // Initialize LibV64Core
            Console.WriteLine("Initializing LibV64Core\n");
            Process[] emulatorProcesses = Memory.GetEmulatorProcesses("Project64");
            if (emulatorProcesses.Length == 0)
                // Throw exception if Project64 is not open
                throw new InvalidOperationException("ERROR: Could not find active Project64 process");

            Memory.HookEmulatorProcess(emulatorProcesses[0]);
            Console.WriteLine(emulatorProcesses[0].MainWindowTitle);
            Memory.FindBaseAddress();
            Console.WriteLine("[M] Found BaseAddress");

            // Apply patches
            Core.FixCameraZoomOut();
            Console.WriteLine("[C] Applied Zoom Fix Patch");
            Core.FixResetBodyState();
            Console.WriteLine("[C] Applied BodyState Patch");

            // Run console stuff
            Commands.CreateWorkspace();
            Console.WriteLine("\nWelcome to V64CoreConsole. Type \"help\" for list of commands.\n");

            var x = Task.Run(() => Update());
            var y = Task.Run(() => Controller.Update());

            RunCommandLoop();
        }

        private static Task Update()
        {
            while (command != "")
            {
                Thread.Sleep(50);
                Core.CoreUpdate();

                // Freeze camera with D-Pad Up + L
                if (Controller.GetButton(Types.ButtonFlags.U_JPAD | Types.ButtonFlags.L_TRIG))
                    Core.ToggleFreezeCamera();
            }

            return Task.CompletedTask;
        }

        private static void RunCommandLoop()
        {
            while (command != "quit")
            {
                Console.Write("> ");
                command = Console.ReadLine()?.ToLower();
                switch (command)
                {
                    case "":
                        break;

                    case "quit":
                        return;

                    case "help":
                        Console.WriteLine("\n" +
                            "help - Display this message\n" +
                            "quit - Safely exits the application\n" +
                            "freeze - Toggles camera freeze/unfreeze\n" +
                            "resetgs - Resets the in-game color code\n" +
                            "getgs - Displays the current loaded color code\n" +
                            "loadgsfile - Loads a color code from a file (colorcodes\\)\n" +
                            "savegsfile - Saves a color code to a file (colorcodes\\)\n" +
                            "eyeswap - Changes the current eye state\n" +
                            "handswap - Changes the current hand state\n" +
                            "powerswap - Changes the current powerup");
                        break;

                    case "freeze":
                        Core.ToggleFreezeCamera();
                        break;

                    case "eyeswap":
                        Console.WriteLine("(blink, open, half, closed, left, right, up, down, dead)");
                        Console.Write("Enter one of the above > ");
                        string? chosenEyeName = Console.ReadLine();
                        if (chosenEyeName != null)
                            Commands.EyeSwap(chosenEyeName);
                        break;

                    case "handswap":
                        Console.WriteLine("(fists, open, peace, cap, wingcap, right)");
                        Console.Write("Enter one of the above > ");
                        string? chosenHandName = Console.ReadLine();
                        if (chosenHandName != null)
                            Commands.HandSwap(chosenHandName);
                        break;

                    case "powerswap":
                        Console.WriteLine("(default, vanish, metal, metalvanish)");
                        Console.Write("Enter one of the above > ");
                        string? chosenPowerUpName = Console.ReadLine();
                        if (chosenPowerUpName != null)
                            Commands.PowerUpSwap(chosenPowerUpName);
                        break;

                    case "loadgsfile":
                        Console.Write("Enter GS name > ");
                        string? loadGsName = Console.ReadLine();
                        if (loadGsName != null)
                        {
                            // Get GameShark text from file
                            string loadGameshark = System.IO.File.ReadAllText("colorcodes\\" + loadGsName + ".gs");
                            if (loadGameshark == null)
                            {
                                Console.WriteLine("ERROR: File \"colorcodes\\" + loadGsName + ".gs\" does not exist.");
                                break;
                            }

                            Types.ColorCode colorCode = Core.GameSharkToColorCode(loadGameshark);
                            Core.ApplyColorCode(colorCode);

                            Console.WriteLine("[C] Applied " + loadGsName);
                        }
                        break;

                    case "savegsfile":
                        Console.Write("Enter GS name > ");
                        string? saveGsName = Console.ReadLine();

                        Types.ColorCode saveColorCode = Core.LoadColorCodeFromGame();
                        string saveGameshark = Core.ColorCodeToGameShark(saveColorCode);

                        // Write GameShark text to file
                        System.IO.File.WriteAllText("colorcodes\\" + saveGsName + ".gs", saveGameshark);

                        Console.WriteLine("Saved to \"colorcodes\\" + saveGsName + ".gs\"");
                        break;

                    case "resetgs":
                        Core.ResetColorCode();
                        Console.WriteLine("[C] Reset ColorCode");
                        break;

                    case "getgs":
                        Types.ColorCode getColorCode = Core.LoadColorCodeFromGame();
                        string getGameshark = Core.ColorCodeToGameShark(getColorCode);
                        Console.WriteLine(getGameshark);
                        break;

                    default:
                        Console.WriteLine("ERROR: Unknown command. Type \"help\" for a list of valid commands.");
                        break;
                }
                Console.WriteLine("");
            }
        }
    }
}
