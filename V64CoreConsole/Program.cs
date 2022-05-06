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
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing LibV64Core\n");

            // Initialize LibV64Core
            Process[] emulatorProcesses = Memory.GetEmulatorProcesses("Project64");
            Memory.HookEmulatorProcess(emulatorProcesses[0]);
            Console.WriteLine("[M] Found Project64");
            Memory.FindBaseAddress();
            Console.WriteLine("[M] Found BaseAddress");

            Core.FixCameraZoomOut();
            Console.WriteLine("[C] Applied Zoom Fix");
            Core.FixResetBodyState();
            Console.WriteLine("[C] Applied BodyState Patch");

            // Run console stuff
            RunCommandLoop();
        }

        public static void RunCommandLoop()
        {
            Console.WriteLine("\nWelcome to V64CoreConsole. Type \"help\" for list of commands.\n");

            string command;
            bool quitNow = false;
            while (!quitNow)
            {
                command = Console.ReadLine();
                switch (command)
                {
                    case "help":
                        Console.WriteLine("\nV64CoreConsole\n\n" + 
                            "help - Display this message\n" + 
                            "quit - Safely exits the application\n" + 
                            "freeze - Toggles camera freeze/unfreeze\n" + 
                            "getgs - Displays the current loaded color code\n" +
                            "loadgsfile - Loads a color code from a file (colorcodes\\)\n" +
                            "savegsfile - Saves a color code to a file (colorcodes\\)");
                        break;

                    case "quit":
                        quitNow = true;
                        break;

                    case "freeze":
                        Core.ToggleFreezeCamera();
                        break;

                    case "eyeswap":
                        Console.WriteLine("(blink, open, half, closed, left, right, up, down, dead)\nEnter one of the above: ");
                        string chosenEyeName = Console.ReadLine();

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

                        break;

                    case "handswap":
                        Console.WriteLine("(fists, open, peace, cap, wingcap, right)\nEnter one of the above: ");
                        string chosenHandName = Console.ReadLine();

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

                        break;

                    case "getgs":
                        Types.ColorCode colorCode1 = Core.LoadColorCodeFromGame();
                        string gameshark1 = Core.ColorCodeToGameShark(colorCode1);
                        Console.WriteLine(gameshark1);
                        break;

                    case "loadgsfile":
                        Console.Write("Enter GS name: ");
                        string gsName = Console.ReadLine();

                        string gameshark = System.IO.File.ReadAllText("colorcodes\\" + gsName + ".gs");
                        if (gameshark == null)
                        {
                            Console.WriteLine("ERROR: File \"colorcodes\\" + gsName + ".gs\" does not exist.");
                            break;
                        }

                        Types.ColorCode colorCode = Core.GameSharkToColorCode(gameshark);
                        Core.ApplyColorCode(colorCode);

                        Console.WriteLine("[C] Applied " + gsName);
                        break;

                    case "savegsfile":
                        Console.Write("Enter GS name: ");
                        string gsName1 = Console.ReadLine();

                        Types.ColorCode colorCode2 = Core.LoadColorCodeFromGame();
                        string gameshark2 = Core.ColorCodeToGameShark(colorCode2);
                        System.IO.File.WriteAllText("colorcodes\\" + gsName1 + ".gs", gameshark2);

                        Console.WriteLine("Saved to \"colorcodes\\" + gsName1 + ".gs\"");
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
