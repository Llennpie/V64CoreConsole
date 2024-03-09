using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibV64Core;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace V64CoreConsole
{
    class Program
    {
        private static string? command;

        static void Main(string[]? args)
        {
            Console.Title = "V64CoreConsole";

            // Initialize LibV64Core
            Console.WriteLine("Initializing LibV64Core");
            Console.Write("[M] Searching for Project64");
            Process[] emulatorProcesses = Memory.GetEmulatorProcesses("Project64");
            while (emulatorProcesses.Length == 0)
            {
                Thread.Sleep(1000);
                Console.Write(".");
                emulatorProcesses = Memory.GetEmulatorProcesses("Project64");
            }

            emulatorProcesses[0].EnableRaisingEvents = true;
            emulatorProcesses[0].Exited += new EventHandler(OnProcessExit);

            Memory.HookEmulatorProcess(emulatorProcesses[0]);
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r[M] " + emulatorProcesses[0].MainWindowTitle + "\n");

            Console.Write("[M] Searching for BaseAddress");
            Memory.FindBaseAddress();
            while (Memory.BaseAddress == 0)
            {
                Thread.Sleep(1000);
                Console.Write(".");
                Memory.FindBaseAddress();
            }
            Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r[M] Found BaseAddress\n");
            Memory.LoadLinkerMap("sm64.us.map");

            // Decomp ROM
            if (Core.State == Types.GameState.Decomp)
            {
                Console.WriteLine("\n==========\n[M] Decomp ROM Detected\nPlease specify a linker MAP file (or leave blank for default):");
                Console.Write("> ");
                string? path = Console.ReadLine();
                if (string.IsNullOrEmpty(path))
                    path = "sm64.us.map";

                Memory.LoadLinkerMap(path);
                Console.WriteLine("==========");
            }

            // Apply patches
            Core.FixCameraZoomOut();
            Core.FixResetBodyState();

            // Run console stuff
            Commands.CreateWorkspace();
            Console.WriteLine("\nWelcome to V64CoreConsole. Type \"help\" for list of commands.\n");

            var x = Task.Run(() => Update());
            var y = Task.Run(() => Controller.Update());

            RunCommandLoop();
        }

        static int cycleEye = 0;

        private static Task Update()
        {
            while (command != "")
            {
                Thread.Sleep(50);
                Core.CoreUpdate();

                // Freeze camera with D-Pad Up + L
                if (Controller.GetButton(Types.ButtonFlags.U_JPAD | Types.ButtonFlags.L_TRIG))
                    Core.ToggleFreezeCamera();

                // Toggle HUD with D-Pad Down + L
                if (Controller.GetButton(Types.ButtonFlags.D_JPAD | Types.ButtonFlags.L_TRIG))
                    Core.HUD = !Core.HUD;

                // Cycle eyes with D-Pad Left + L
                if (Controller.GetButton(Types.ButtonFlags.L_JPAD | Types.ButtonFlags.L_TRIG)) {
                    if (cycleEye < 8) cycleEye++;
                    else cycleEye = 0;

                    Core.SetEyeState((Types.EyeState)cycleEye);
                }
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
                            "powerswap - Changes the current powerup\n" +
                            "hud - Toggles the HUD\n" +
                            "shadow - Toggles the player shadow");
                        break;

                    case "refresh":
                        Console.Clear();
                        Main(null);
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
                            colorCode.Name = loadGsName;
                            Core.ApplyColorCode(colorCode);
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
                        break;

                    case "getgs":
                        Types.ColorCode getColorCode = Core.LoadColorCodeFromGame();
                        string getGameshark = Core.ColorCodeToGameShark(getColorCode);
                        Console.WriteLine(getGameshark);
                        break;

                    case "hud":
                        Core.HUD = !Core.HUD;
                        break;

                    case "shadow":
                        Core.Shadow = !Core.Shadow;
                        break;

                    default:
                        Console.WriteLine("ERROR: Unknown command. Type \"help\" for a list of valid commands.");
                        break;
                }
                Console.WriteLine("");
            }
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            Console.Clear();
            Main(null);
        }
    }
}
