# PrintQueueWatcher

A simple Windows desktop application that monitors a print queue and shows a visual and audible alert once the queue is fully empty.

This is particularly useful when **sending many print jobs in quick succession** (for example, printing the pages of a PDF in batches). Sending a new job before the previous one has actually finished can, on some printer/driver combinations, result in corrupted or incomplete output. PrintQueueWatcher detects when the queue has been genuinely empty for a duration you configure, then alerts you that it's safe to send the next job.

*Türkçe için: [README.md](README.md)*

## Features

- **Runs from the system tray** — no taskbar clutter, lives next to the clock.
- **Real Windows Print API** (`System.Printing`) — not a fragile spool-folder file count; reads the job count for the specific printer you selected.
- **Large, hard-to-miss alert window** — small / medium size options.
- **Customizable colors** — choose the alert window's background and button color; text color is automatically computed for readability.
- **Notification sound choice** — silent or several tones, with a "Test Sound" button to preview.
- **Adjustable wait time** — how many seconds the queue must stay empty before alerting, set via a slider.
- **Adjustable check frequency** — how often the app polls the queue is also configurable; can be increased on lower-spec machines.
- **Light / Dark / System theme.**
- **Turkish and English language support** — asked once on first launch, persists, changeable later from Settings.
- **Launch with Windows** — via a registry key, no admin rights required. If a printer is selected, monitoring starts automatically on every launch.

## Screenshots

> _Screenshots of the main window and settings window can be added here._

## Download and Run

No installation required. Download the latest `PrintQueueWatcher.exe` from the [Releases](../../releases) page and run it directly. It's a single file; you don't need to install the .NET Runtime separately (everything needed is embedded in the exe).

> Windows SmartScreen may show a warning on first run since the exe is unsigned. Click "More info" → "Run anyway" to proceed.

## Requirements

- Windows 10 or later (64-bit)
- To build from source: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and Visual Studio 2022 (17.8+)

## Building from Source (Developer)

1. Clone this repository:
   ```
   git clone https://github.com/<your-username>/PrintQueueWatcher.git
   ```
2. Open `PrintQueueWatcher.sln` in Visual Studio.
3. Let NuGet restore packages (`Hardcodet.NotifyIcon.Wpf`).
4. Press F5 to build and run.

### Producing a single-file .exe (publish)

To produce a distributable single `.exe`, run the following in the repository root:

```
dotnet publish PrintQueueWatcher/PrintQueueWatcher.csproj -c Release
```

The output appears at `PrintQueueWatcher/bin/Release/net8.0-windows/win-x64/publish/PrintQueueWatcher.exe`. This file is self-contained (roughly 150-160 MB) and can be copied to another Windows machine and run directly.

## Usage

1. On first launch, you'll be asked to pick a language (one-time).
2. Open **Settings** and select the printer you want to monitor.
3. Optionally adjust the wait time, check frequency, notification sound, alert window appearance, and theme; click **Save**.
4. If a printer is selected, monitoring starts automatically. The app keeps running in the background (system tray). Send your print jobs as usual; once the queue is empty and stays empty for the configured duration, you'll get a large alert window plus a sound.
5. To fully quit, **right-click the tray icon and choose "Exit"** (closing the window with X only minimizes to tray; the app keeps running in the background).

## Project Structure

```
PrintQueueWatcher/
├── Models/            AppSettings and related enums
├── Services/           Business logic: printer reading, queue monitoring, settings, theme, language, sound, startup registration
├── Views/              WPF windows: MainWindow, SettingsWindow, AlertWindow, ColorPickerWindow
├── Localization/        Strings.tr.xaml, Strings.en.xaml
├── Resources/           Theme color dictionaries, shared styles, app icon
└── App.xaml(.cs)        Entry point, single-instance guard, service wiring
```

## Contributing

Issues and pull requests are welcome. For larger changes, please open an issue first to discuss what you'd like to change.

## License

This project is licensed under the [MIT License](LICENSE).
