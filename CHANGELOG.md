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