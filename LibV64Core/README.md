# LibV64Core

- **LibV64Core** is an experimental C# class library aiming to assist developers with creating memory-based *Super Mario 64* machinima tools.
- This library was designed to be as simple and lightweight as possible, while keeping core features intact.
- *Check out [V64CoreConsole](https://github.com/Llennpie/V64CoreConsole), an example console application built using this library.*

## Usage

First, add **LibV64Core** to your existing tool's project. In **Visual Studio 2019**:
- Right click your Solution in the Solution Explorer and navigate to *Add -> Existing Project...*
- Select `LibV64Core.csproj`.
- Right click your tool's project in the Solution Explorer and navigate to *Add -> Reference...*
- Under *Project -> Solution* select **LibV64Core**.

**LibV64Core** should now be ready for use. Here's a quick example in **Windows Forms**:

```csharp
using System;
using System.Diagnostics;
using System.Windows.Forms;
using LibV64Core;

namespace SampleToolGui
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            Process[] emulatorProcesses = Memory.GetEmulatorProcesses("Project64");
            Memory.HookEmulatorProcess(emulatorProcesses[0]);
            Memory.FindBaseAddress();

            if (Core.CameraFrozen)
                Debug.WriteLine("Camera was already frozen!");

            Core.FixCameraZoomOut();
        }
    }
}
```

Documentation is coming soon.

## Credits

- *[Project Comet](https://github.com/projectcomet64)* and *[GlitchyPSI](https://github.com/GlitchyPSIX)* for creating [M64MM3](https://github.com/projectcomet64/M64MM).
- *[James "CaptainSwag101" Pelster](https://github.com/jpmac26)* for creating [M64MM2](https://github.com/jpmac26/M64MM2).
