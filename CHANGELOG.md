# 0.1.99-demo-2
- Tweak button icons by moving to WPF.UI icons vs converted SVGs. This resulted in much cleaner XAML
- Add Info button which will eventually show version information and more.
- Add LabelValidation() function to manage labels with ease using Dispatching.
- Port DebugConsole() and ConsoleWriteLine() functions to new interface.
- Version identification, TODO: Add version info to debug logs.

# 0.1.99-demo
- Interface 2.0 basic layout.
- Tooltips added.
- Setting validation (NF)
- Functionally unusable, demo is for layout to use set MainWindow2.xaml as entrypoint.

# 0.1.2 (hotfix-2)
- [Hotfix]Fix a hang with MSFSLayoutGenerator.exe
- [Hotfix]Handle errors from MSFSLayoutGenerator.exe

# 0.1.2 (hotfix-1)
- Add BuilderLogError.txt opener to provide info on failures.. (Incomplete) (Note: No Steam Support yet)
- Dialog at end asks to open log file on error.
- Move main subroutines to new class for readability.
- [Hotfix]Fix a bug causing JSON to always fail.
- [Hotfix]Fix a bug causing all textures to be processed as ALBD.

# 0.1.2
- Automatic detection of JSON file for DDS texture format. Fallback to filename detection if not
- Refactor GenerateXMLs() to handle JJSON and some minor efficiencies.
- Cleanup code by moving some unnecessary repeated items to either subroutine or outside loops.
- Check for KTX2 conversion failure. Unspecified error for now.
- Add logging.

# 0.1.1
- Cleanup Code.
- Cleaner 'debug' console window call.
- Progress wheel now correctly stops spinning after texture conversion complete
- Error Handling for existing textures already in Project Directory
- Remove additional MainWindow references by having ExeClass constructor use one MainWindow reference
- Disable upload button when no texture path is set.
- Added a helper tooltip to upload button.
- Error handling for missing SDK
- Error handling for missing layoutgenerator.
- Window no longer resizable
- Debug output now readonly.

# 0.1.0
Initial Release.