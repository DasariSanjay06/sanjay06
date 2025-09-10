# Modern Camera Capture Application

A beautifully designed Windows Forms application for camera capture with modern UI elements and smooth user experience.

## Features

### 🎨 Modern UI Design
- **Flat Design**: Clean, modern flat design with no borders
- **Custom Color Scheme**: Professional color palette with attractive gradients
- **Rounded Corners**: Smooth rounded corners for buttons and form
- **Hover Effects**: Interactive hover effects on all buttons
- **Smooth Animations**: Notification system with fade effects

### 📸 Camera Features
- **Connect/Disconnect**: Easy camera connection management
- **Live Preview**: Real-time camera feed display
- **Image Capture**: Capture images in multiple formats
- **Frame Rate Display**: Live FPS counter with stylish display
- **Format Selection**: Support for JPEG, PNG, BMP, GIF, and TIFF formats

### 🎯 UI Components

#### Main Window
- **Borderless Design**: Custom title bar with minimize and close buttons
- **Draggable Window**: Click and drag anywhere on the form to move
- **Shadow Effects**: Subtle shadows for depth perception

#### Control Panel
- **Connect Button** (Green - #2ECC71): Establishes camera connection
- **Test Button** (Blue - #3498DB): Tests camera functionality
- **Disconnect Button** (Red - #E74C3C): Safely disconnects camera
- **Capture Button** (Purple - #9B59B6): Captures current frame

#### Display Elements
- **Picture Box**: Large preview area with dark background
- **FPS Counter**: Real-time frame rate display with green text on dark background
- **Format Selector**: Dropdown for selecting save format
- **Notification System**: Toast-style notifications for user feedback

## Building the Application

### For .NET 6.0+ (Recommended)
```bash
dotnet build ModernCameraApp.csproj
dotnet run
```

### For .NET Framework 4.7.2
```bash
msbuild ModernCameraApp_Framework.csproj
```

Or open in Visual Studio and press F5 to build and run.

## Project Structure

```
ModernCameraApp/
├── MainForm.cs           # Main form logic and UI styling
├── MainForm.Designer.cs  # Form designer generated code
├── Program.cs            # Application entry point
├── ModernCameraApp.csproj         # .NET 6.0+ project file
└── ModernCameraApp_Framework.csproj  # .NET Framework project file
```

## UI Improvements Made

### Original Issues Fixed
1. ✅ **Poor Layout**: Reorganized controls into logical groups
2. ✅ **Inconsistent Styling**: Applied uniform modern design language
3. ✅ **No Visual Hierarchy**: Added clear visual hierarchy with colors and sizing
4. ✅ **Basic Buttons**: Replaced with styled buttons with hover effects
5. ✅ **Plain Labels**: Enhanced with better fonts and colors
6. ✅ **No User Feedback**: Added notification system

### New Features Added
1. **Custom Title Bar**: Borderless window with custom controls
2. **Drag Support**: Window can be moved by dragging
3. **Control Panel**: Grouped controls in a styled panel
4. **Real-time FPS**: Live frame rate counter
5. **Toast Notifications**: User feedback system
6. **Hover Effects**: Interactive button states
7. **Rounded Corners**: Modern aesthetic with rounded elements
8. **Shadow Effects**: Depth perception with shadows
9. **Color Coding**: Intuitive color scheme for different actions

## Color Palette

- **Primary Background**: #F0F2F7 (Light Gray)
- **Panel Background**: #FAFAFA (Off White)
- **Dark Elements**: #2C3E50 (Dark Blue-Gray)
- **Connect Button**: #2ECC71 (Emerald Green)
- **Disconnect Button**: #E74C3C (Alizarin Red)
- **Test Button**: #3498DB (Peter River Blue)
- **Capture Button**: #9B59B6 (Amethyst Purple)
- **FPS Display**: #80FF80 (Light Green on Dark)

## Customization

You can easily customize the appearance by modifying the colors in `MainForm.cs`:

```csharp
// Change button colors
ApplyModernButtonStyle(btnConnect, Color.FromArgb(46, 204, 113), Color.White);

// Change form background
this.BackColor = Color.FromArgb(240, 242, 247);

// Change notification colors
ShowNotification("Message", Color.FromArgb(46, 204, 113));
```

## Requirements

- Windows 7 or later
- .NET Framework 4.7.2 or .NET 6.0+
- Visual Studio 2019 or later (optional)

## Future Enhancements

Potential improvements for future versions:
- Add camera settings panel
- Implement video recording
- Add image filters and effects
- Include image preview before saving
- Add batch capture mode
- Implement camera selection for multiple devices

## License

This project is provided as-is for educational and commercial use.