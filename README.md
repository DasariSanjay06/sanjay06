# Shimnit Laser Marking System

A fully automated laser code marking system for vehicle number plates, designed specifically for Shimnit India. This professional Windows Forms application provides comprehensive laser marking capabilities with support for RTC3 and RTC4 laser controllers.

## Features

### 🎯 Core Functionality
- **Automated Laser Marking**: Complete automation of laser marking process for vehicle number plates
- **Dual Controller Support**: Full support for RTC3 and RTC4 laser controllers
- **Professional UI**: Modern, intuitive Windows Forms interface with tabbed navigation
- **Database Management**: SQLite database for job tracking, parameters, and system settings
- **Error Handling**: Comprehensive error handling and logging throughout the application

### 🔧 Laser Controller Features
- **RTC3 Support**: Complete parameter control for RTC3 controllers
- **RTC4 Support**: Advanced parameter control including acceleration, jerk, and arc settings
- **Real-time Communication**: TCP/IP communication with laser controllers
- **Status Monitoring**: Real-time status monitoring and system information display
- **Parameter Library**: Save and reuse laser parameters for different materials

### 📊 Job Management
- **Job Creation**: Create marking jobs with detailed vehicle and customer information
- **Job Queue**: Manage and prioritize marking jobs
- **Job History**: Track completed jobs and performance metrics
- **Priority System**: Assign priorities to jobs for efficient processing
- **Operator Tracking**: Track which operator created each job

### ⚙️ System Features
- **Settings Management**: Comprehensive system settings with database persistence
- **Logging System**: Detailed logging with Serilog for debugging and monitoring
- **Report Generation**: Export job reports and statistics
- **Pattern Editor**: Create and edit marking patterns (planned feature)
- **Parameter Library**: Save and manage laser parameters for different materials

## Installation

### Prerequisites
- Windows 10/11 or Windows Server 2019/2022
- .NET 6.0 Runtime or later
- SQLite (included in the application)
- Network access to laser controllers

### Installation Steps

1. **Download the Application**
   ```bash
   git clone <repository-url>
   cd LaserMarkingSystem
   ```

2. **Build the Application**
   ```bash
   dotnet restore
   dotnet build --configuration Release
   ```

3. **Run the Application**
   ```bash
   dotnet run --project LaserMarkingSystem
   ```

4. **First Time Setup**
   - The application will automatically create the database and log directories
   - Configure your laser controllers in the Controller Status tab
   - Set up default parameters in the Settings tab

## Usage Guide

### 1. Controller Setup
1. Navigate to the "Controller Status" tab
2. Add your laser controller details (IP address, port, type)
3. Click "Connect" to establish communication
4. Verify the controller status shows "Connected"

### 2. Creating a Marking Job
1. Go to the "Job Creation" tab
2. Fill in the job details:
   - Job Name: Descriptive name for the job
   - Number Plate: Vehicle number plate to be marked
   - Vehicle Type: Type of vehicle (Car, Motorcycle, Truck, etc.)
   - Customer Name: Customer information
   - Marking Text: Text to be marked on the number plate
   - Priority: Job priority (1-10)
   - Operator: Operator name

3. Configure laser parameters:
   - Power: Laser power in Watts
   - Speed: Marking speed in mm/s
   - Frequency: Laser frequency in Hz
   - Pulse Width: Pulse width in microseconds
   - Focus Position: Focus position in mm
   - Marking Depth: Depth of marking in mm
   - Repetitions: Number of marking passes
   - Spot Size: Laser spot size in mm
   - Enable Galvo: Enable/disable galvo scanner
   - Galvo Delay: Galvo delay in milliseconds
   - Material: Material type (Aluminum, Steel, Plastic, etc.)

4. Click "Create Job" to add the job to the queue

### 3. Running Jobs
1. Navigate to the "Job Queue" tab
2. Select a job from the list
3. Ensure the laser controller is connected
4. Click "Start Job" to begin marking
5. Monitor the job status in real-time
6. Click "Stop Job" if needed to stop the marking process

### 4. System Settings
1. Go to the "Settings" tab
2. Configure default parameters:
   - Default Controller: RTC3 or RTC4
   - Default Power: Default laser power
   - Default Speed: Default marking speed
   - Company Name: Your company name
   - Log Level: Application logging level
3. Click "Save Settings" to persist changes

## Laser Controller Parameters

### RTC3 Parameters
- **Power**: 0-1000 Watts
- **Speed**: 0-10000 mm/s
- **Frequency**: 0-100000 Hz
- **Pulse Width**: 0-1000 microseconds
- **Focus Position**: -100 to 100 mm
- **Marking Depth**: 0-10 mm
- **Repetitions**: 1-100 passes
- **Spot Size**: 0.01-10 mm
- **Galvo Enable**: On/Off
- **Galvo Delay**: 0-1000 ms

### RTC4 Advanced Parameters
All RTC3 parameters plus:
- **Acceleration**: 0-10000 mm/s²
- **Jerk**: 0-10000 mm/s³
- **Corner Speed**: 0-10000 mm/s
- **Jump Speed**: 0-10000 mm/s
- **Jump Delay**: 0-1000 ms
- **Mark Delay**: 0-1000 ms
- **Poly Delay**: 0-1000 ms
- **Laser Delay**: 0-1000 ms
- **Skip Delay**: 0-1000 ms
- **Arc Delay**: 0-1000 ms
- **Arc Angle**: 0-360 degrees
- **Arc Corner**: 0-1000 mm

## Database Schema

The application uses SQLite database with the following tables:

### LaserControllers
- Id (Primary Key)
- Name
- Type (RTC3/RTC4)
- IpAddress
- Port
- Status
- IsConnected
- LastConnected
- SerialNumber
- FirmwareVersion

### LaserParameters
- Id (Primary Key)
- Name
- Power, Speed, Frequency, PulseWidth
- FocusPosition, MarkingDepth, Repetitions
- SpotSize, EnableGalvo, GalvoDelay
- Material, Description
- CreatedDate, ModifiedDate

### MarkingJobs
- Id (Primary Key)
- JobName, NumberPlate, VehicleType
- CustomerName, MarkingText, MarkingPattern
- ParametersId (Foreign Key)
- ScheduledTime, CompletedTime
- Status, ErrorMessage, Priority
- OperatorName, Notes

### SystemSettings
- Key (Primary Key)
- Value
- Description

## Logging

The application uses Serilog for comprehensive logging:

- **Log Location**: `Logs/laser_marking_system.log`
- **Log Rotation**: Daily rotation with 30-day retention
- **Log Levels**: Debug, Information, Warning, Error, Fatal
- **Log Format**: Timestamp, Level, Message, Exception details

## Error Handling

The application includes comprehensive error handling:

- **Database Errors**: Graceful handling of database connection issues
- **Network Errors**: Timeout and connection error handling for laser controllers
- **Parameter Validation**: Input validation for all laser parameters
- **Job Errors**: Error tracking and reporting for failed jobs
- **System Errors**: Application crash handling with user notification

## Development

### Project Structure
```
LaserMarkingSystem/
├── Models/                 # Data models and enums
├── Services/              # Business logic services
├── Database/              # Database operations
├── Forms/                 # Windows Forms
├── Utils/                 # Utility classes
└── Resources/             # Application resources
```

### Building from Source
```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build --configuration Release

# Run tests (if available)
dotnet test

# Create deployment package
dotnet publish --configuration Release --output ./publish
```

## Troubleshooting

### Common Issues

1. **Controller Connection Failed**
   - Verify IP address and port settings
   - Check network connectivity
   - Ensure controller is powered on and ready

2. **Database Errors**
   - Check file permissions in application directory
   - Verify SQLite is properly installed
   - Check available disk space

3. **Parameter Errors**
   - Verify parameter ranges for your controller type
   - Check material compatibility
   - Review laser safety guidelines

4. **Job Execution Errors**
   - Verify controller connection
   - Check marking text format
   - Review parameter settings

### Support

For technical support or feature requests, please contact:
- **Company**: Shimnit India
- **Email**: support@shimnit.com
- **Phone**: +91-XXXXXXXXXX

## License

© 2024 Shimnit India. All rights reserved.

This software is proprietary and confidential. Unauthorized copying, distribution, or use is strictly prohibited.

## Version History

### v1.0.0 (Current)
- Initial release
- RTC3 and RTC4 controller support
- Complete job management system
- Professional Windows Forms UI
- SQLite database integration
- Comprehensive logging and error handling
- Parameter library and settings management

---

**Note**: This application is designed for professional laser marking operations. Always follow laser safety guidelines and ensure proper training before operation.