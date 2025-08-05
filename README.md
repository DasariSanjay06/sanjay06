# Shimnit Laser Marking System

A comprehensive Windows Forms application for automated laser code marking on vehicle number plates, designed specifically for **Shimnit Company India**. This system supports both RTC3 and RTC4 laser controllers with advanced parameter control, database management, and professional UI.

## Features

### 🎯 Core Functionality
- **Automated Laser Marking** for vehicle number plates
- **Dual Controller Support** - RTC3 and RTC4 laser controllers
- **Real-time Progress Monitoring** with detailed status updates
- **Professional Windows Forms UI** with modern design

### 🔧 Laser Controller Support
- **RTC3 Controller**
  - Serial communication (COM1 default)
  - Basic and advanced marking parameters
  - Standard galvanometer control
  - Temperature compensation

- **RTC4 Controller** 
  - Enhanced serial communication (COM2 default)
  - 3D marking capabilities
  - Optical power control
  - Advanced wobble parameters
  - Enhanced quality assessment

### 📊 Database Management
- **SQL Server LocalDB** for data storage
- **Entity Framework Core** for data access
- Comprehensive job tracking and history
- System logging and audit trails
- Automatic database initialization

### 🎛️ Advanced Parameters
- **Power Control** (0-100%)
- **Speed Settings** (1-10,000 mm/min)
- **Pulse Frequency** (1,000-100,000 Hz)
- **Multi-pass Marking** (1-20 passes)
- **Position Control** (X, Y, Z coordinates)
- **Quality Settings** and validation
- **Safety Parameters** with delays

### 📈 Monitoring & Analytics
- **Real-time Status Display**
- **Job History** with filtering and export
- **System Logs** with level-based filtering
- **Performance Statistics** and metrics
- **Quality Tracking** and reporting

## System Requirements

- **Operating System**: Windows 10/11 (64-bit)
- **.NET Framework**: .NET 6.0 or later
- **Database**: SQL Server LocalDB (included)
- **Hardware**: Serial ports for laser controller communication
- **Memory**: Minimum 4GB RAM
- **Storage**: 100MB free space

## Installation

1. **Download** the application files
2. **Install .NET 6.0 Runtime** if not already installed
3. **Run** the application executable
4. **Database** will be automatically created on first run
5. **Configure** laser controller settings in the application

## Configuration

### Laser Controller Setup

#### RTC3 Controller
```json
{
  "RTC3": {
    "CommunicationPort": "COM1",
    "BaudRate": 115200,
    "Timeout": 5000,
    "MaxPower": 100,
    "DefaultSpeed": 1000,
    "DefaultPulseFrequency": 20000
  }
}
```

#### RTC4 Controller
```json
{
  "RTC4": {
    "CommunicationPort": "COM2",
    "BaudRate": 115200,
    "Timeout": 5000,
    "MaxPower": 100,
    "DefaultSpeed": 1200,
    "DefaultPulseFrequency": 25000
  }
}
```

### Database Configuration
- **Connection String**: Automatically configured for LocalDB
- **Auto-backup**: Enabled with 24-hour intervals
- **Log Retention**: 30 days default

## Usage

### 1. Starting the Application
- Launch the application
- The database will initialize automatically
- Default settings are loaded for both controllers

### 2. Creating a Marking Job
1. Enter **Vehicle Number Plate**
2. Select **Vehicle Type** from dropdown
3. Enter **Owner Name** (optional)
4. Specify **Marking Text** (defaults to number plate)
5. Choose **Controller Type** (RTC3/RTC4)
6. Set **Position** coordinates
7. Click **Create Job**

### 3. Connecting to Controller
1. Select controller type in the dropdown
2. Click **Connect** button
3. Wait for successful connection confirmation
4. Controller status will update in real-time

### 4. Starting Marking Operation
1. Select a job from the job list
2. Ensure controller is connected and ready
3. Click **Start Marking**
4. Monitor progress in real-time
5. Job status updates automatically

### 5. Parameter Configuration
1. Click **Parameters** button
2. Configure marking parameters per controller type:
   - **Basic**: Power, Speed, Frequency, Passes
   - **Advanced**: Scanner settings, delays, quality
   - **Position**: Coordinates, rotation, spacing
   - **RTC4 Features**: 3D mode, wobble, optical control

## Application Structure

```
ShimnitLaserMarking/
├── Data/                          # Database context and models
├── Forms/                         # Windows Forms UI
│   ├── MainForm.cs               # Primary application interface
│   ├── ParametersForm.cs         # Laser parameter configuration
│   ├── JobHistoryForm.cs         # Job history and filtering
│   ├── SystemLogsForm.cs         # System logs viewer
│   ├── StatisticsForm.cs         # Performance statistics
│   └── SettingsForm.cs           # Application settings
├── Models/                        # Data models and entities
│   ├── MarkingJob.cs             # Job entity
│   ├── LaserParameters.cs        # Parameter model
│   ├── LaserController.cs        # Controller entity
│   └── SystemLog.cs              # Logging entity
├── Services/                      # Business logic services
│   ├── DatabaseService.cs        # Database operations
│   ├── LaserControllerService.cs # Controller management
│   └── LaserControllers/         # Controller implementations
│       ├── ILaserController.cs   # Controller interface
│       ├── RTC3Controller.cs     # RTC3 implementation
│       └── RTC4Controller.cs     # RTC4 implementation
├── Resources/                     # Application resources
├── Logs/                          # Application logs
├── appsettings.json              # Configuration file
└── Program.cs                    # Application entry point
```

## Database Schema

### MarkingJobs Table
- Job tracking and management
- Vehicle information and owner details
- Laser parameters and settings
- Status tracking and timestamps
- Quality ratings and error logging

### SystemLogs Table
- Application event logging
- Error tracking and debugging
- User activity monitoring
- Performance metrics

### LaserControllers Table
- Controller configuration
- Connection status tracking
- Usage statistics

### VehicleTypes Table
- Vehicle categorization
- Default parameters per type

## Error Handling

The application implements comprehensive error handling:

- **Connection Errors**: Automatic retry and user notification
- **Parameter Validation**: Real-time validation with user feedback
- **Database Errors**: Graceful degradation and error logging
- **Hardware Failures**: Safe shutdown and recovery procedures
- **Logging**: All errors logged to database and file system

## Logging

Multi-level logging system:
- **Information**: Normal operations and status updates
- **Warning**: Non-critical issues and advisories
- **Error**: Operational errors and failures
- **Fatal**: Critical system failures

## Security Features

- **Input Validation**: All user inputs validated and sanitized
- **Database Security**: Parameterized queries prevent SQL injection
- **Access Control**: User-based operation tracking
- **Audit Trail**: Complete operation history maintenance

## Troubleshooting

### Common Issues

1. **Controller Connection Failed**
   - Verify COM port settings
   - Check physical connections
   - Ensure controller is powered on
   - Review communication parameters

2. **Database Connection Issues**
   - Verify LocalDB installation
   - Check file permissions
   - Review connection string configuration

3. **Parameter Validation Errors**
   - Check parameter ranges and limits
   - Verify controller-specific constraints
   - Review marking material compatibility

4. **Performance Issues**
   - Monitor system resources
   - Check database size and cleanup old logs
   - Verify hardware specifications

## Maintenance

### Regular Maintenance
- **Log Cleanup**: Old logs automatically cleaned (30+ days)
- **Database Backup**: Automatic daily backups
- **Parameter Validation**: Regular verification of controller settings

### Updates and Upgrades
- Check for application updates periodically
- Backup database before major updates
- Test controller functionality after updates

## Support

For technical support and issues:
- **Company**: Shimnit Company India
- **Application**: Laser Marking System v1.0
- **Documentation**: This README file
- **Logs**: Check system logs for detailed error information

## License

© 2024 Shimnit Company India. All rights reserved.
Proprietary software for internal use only.

---

**Version**: 1.0.0  
**Last Updated**: December 2024  
**Compatibility**: RTC3/RTC4 Laser Controllers  
**Platform**: Windows 10/11 (.NET 6.0)