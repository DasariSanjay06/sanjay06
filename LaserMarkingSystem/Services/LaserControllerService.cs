using System.Net.Sockets;
using System.Text;
using LaserMarkingSystem.Models;
using Serilog;

namespace LaserMarkingSystem.Services
{
    public class LaserControllerService
    {
        private readonly ILogger _logger;
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private readonly object _lockObject = new();
        private bool _isConnected;

        public LaserControllerService()
        {
            _logger = Log.ForContext<LaserControllerService>();
        }

        public async Task<bool> ConnectAsync(LaserController controller)
        {
            try
            {
                lock (_lockObject)
                {
                    if (_isConnected)
                    {
                        Disconnect();
                    }

                    _tcpClient = new TcpClient();
                    _tcpClient.Connect(controller.IpAddress, controller.Port);
                    _stream = _tcpClient.GetStream();
                    _isConnected = true;
                }

                // Test connection with controller-specific commands
                var isConnected = await TestConnectionAsync(controller.Type);
                if (isConnected)
                {
                    controller.IsConnected = true;
                    controller.Status = LaserStatus.Ready;
                    controller.LastConnected = DateTime.Now;
                    _logger.Information("Connected to laser controller {ControllerName} at {IpAddress}:{Port}", 
                        controller.Name, controller.IpAddress, controller.Port);
                }
                else
                {
                    Disconnect();
                }

                return isConnected;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to connect to laser controller {ControllerName}", controller.Name);
                Disconnect();
                return false;
            }
        }

        public void Disconnect()
        {
            lock (_lockObject)
            {
                _stream?.Close();
                _tcpClient?.Close();
                _stream = null;
                _tcpClient = null;
                _isConnected = false;
            }
            _logger.Information("Disconnected from laser controller");
        }

        private async Task<bool> TestConnectionAsync(ControllerType controllerType)
        {
            try
            {
                var response = await SendCommandAsync(GetControllerSpecificCommand(controllerType, "GET_STATUS"));
                return !string.IsNullOrEmpty(response) && response.Contains("OK");
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> SendCommandAsync(string command)
        {
            if (!_isConnected || _stream == null)
            {
                throw new InvalidOperationException("Laser controller is not connected");
            }

            try
            {
                var commandBytes = Encoding.ASCII.GetBytes(command + "\r\n");
                await _stream.WriteAsync(commandBytes, 0, commandBytes.Length);
                await _stream.FlushAsync();

                // Read response
                var buffer = new byte[1024];
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                _logger.Debug("Command: {Command}, Response: {Response}", command, response);
                return response.Trim();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send command: {Command}", command);
                throw;
            }
        }

        public async Task<bool> SetLaserParametersAsync(LaserParameters parameters, ControllerType controllerType)
        {
            try
            {
                var commands = GetParameterCommands(parameters, controllerType);
                foreach (var command in commands)
                {
                    var response = await SendCommandAsync(command);
                    if (!response.Contains("OK"))
                    {
                        _logger.Error("Failed to set parameter: {Command}, Response: {Response}", command, response);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to set laser parameters");
                return false;
            }
        }

        public async Task<bool> StartMarkingAsync(string markingData, ControllerType controllerType)
        {
            try
            {
                // Set marking mode
                var modeCommand = GetControllerSpecificCommand(controllerType, "SET_MARKING_MODE");
                await SendCommandAsync(modeCommand);

                // Send marking data
                var dataCommand = GetControllerSpecificCommand(controllerType, "SEND_MARKING_DATA");
                await SendCommandAsync($"{dataCommand} {markingData}");

                // Start marking
                var startCommand = GetControllerSpecificCommand(controllerType, "START_MARKING");
                var response = await SendCommandAsync(startCommand);

                return response.Contains("OK");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start marking");
                return false;
            }
        }

        public async Task<bool> StopMarkingAsync(ControllerType controllerType)
        {
            try
            {
                var stopCommand = GetControllerSpecificCommand(controllerType, "STOP_MARKING");
                var response = await SendCommandAsync(stopCommand);
                return response.Contains("OK");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop marking");
                return false;
            }
        }

        public async Task<LaserStatus> GetStatusAsync(ControllerType controllerType)
        {
            try
            {
                var statusCommand = GetControllerSpecificCommand(controllerType, "GET_STATUS");
                var response = await SendCommandAsync(statusCommand);

                if (response.Contains("BUSY"))
                    return LaserStatus.Busy;
                else if (response.Contains("ERROR"))
                    return LaserStatus.Error;
                else if (response.Contains("READY"))
                    return LaserStatus.Ready;
                else
                    return LaserStatus.Disconnected;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get laser status");
                return LaserStatus.Error;
            }
        }

        public async Task<Dictionary<string, object>> GetSystemInfoAsync(ControllerType controllerType)
        {
            var info = new Dictionary<string, object>();

            try
            {
                var commands = GetSystemInfoCommands(controllerType);
                foreach (var command in commands)
                {
                    var response = await SendCommandAsync(command.Key);
                    info[command.Value] = response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get system info");
            }

            return info;
        }

        private string GetControllerSpecificCommand(ControllerType controllerType, string command)
        {
            return controllerType switch
            {
                ControllerType.RTC3 => GetRTC3Command(command),
                ControllerType.RTC4 => GetRTC4Command(command),
                _ => throw new ArgumentException($"Unsupported controller type: {controllerType}")
            };
        }

        private string GetRTC3Command(string command)
        {
            return command switch
            {
                "GET_STATUS" => "STATUS",
                "SET_MARKING_MODE" => "MODE MARKING",
                "SEND_MARKING_DATA" => "DATA",
                "START_MARKING" => "START",
                "STOP_MARKING" => "STOP",
                "GET_POWER" => "POWER?",
                "SET_POWER" => "POWER",
                "GET_SPEED" => "SPEED?",
                "SET_SPEED" => "SPEED",
                "GET_FREQUENCY" => "FREQ?",
                "SET_FREQUENCY" => "FREQ",
                "GET_PULSE_WIDTH" => "PULSE?",
                "SET_PULSE_WIDTH" => "PULSE",
                "GET_FOCUS" => "FOCUS?",
                "SET_FOCUS" => "FOCUS",
                "GET_DEPTH" => "DEPTH?",
                "SET_DEPTH" => "DEPTH",
                "GET_REPETITIONS" => "REP?",
                "SET_REPETITIONS" => "REP",
                "GET_SPOT_SIZE" => "SPOT?",
                "SET_SPOT_SIZE" => "SPOT",
                "GET_GALVO_ENABLE" => "GALVO?",
                "SET_GALVO_ENABLE" => "GALVO",
                "GET_GALVO_DELAY" => "GALVO_DELAY?",
                "SET_GALVO_DELAY" => "GALVO_DELAY",
                "GET_SERIAL_NUMBER" => "SERIAL?",
                "GET_FIRMWARE_VERSION" => "FIRMWARE?",
                "GET_TEMPERATURE" => "TEMP?",
                "GET_VOLTAGE" => "VOLTAGE?",
                "GET_CURRENT" => "CURRENT?",
                _ => throw new ArgumentException($"Unknown RTC3 command: {command}")
            };
        }

        private string GetRTC4Command(string command)
        {
            return command switch
            {
                "GET_STATUS" => "STATUS",
                "SET_MARKING_MODE" => "MODE MARKING",
                "SEND_MARKING_DATA" => "DATA",
                "START_MARKING" => "START",
                "STOP_MARKING" => "STOP",
                "GET_POWER" => "POWER?",
                "SET_POWER" => "POWER",
                "GET_SPEED" => "SPEED?",
                "SET_SPEED" => "SPEED",
                "GET_FREQUENCY" => "FREQ?",
                "SET_FREQUENCY" => "FREQ",
                "GET_PULSE_WIDTH" => "PULSE?",
                "SET_PULSE_WIDTH" => "PULSE",
                "GET_FOCUS" => "FOCUS?",
                "SET_FOCUS" => "FOCUS",
                "GET_DEPTH" => "DEPTH?",
                "SET_DEPTH" => "DEPTH",
                "GET_REPETITIONS" => "REP?",
                "SET_REPETITIONS" => "REP",
                "GET_SPOT_SIZE" => "SPOT?",
                "SET_SPOT_SIZE" => "SPOT",
                "GET_GALVO_ENABLE" => "GALVO?",
                "SET_GALVO_ENABLE" => "GALVO",
                "GET_GALVO_DELAY" => "GALVO_DELAY?",
                "SET_GALVO_DELAY" => "GALVO_DELAY",
                "GET_SERIAL_NUMBER" => "SERIAL?",
                "GET_FIRMWARE_VERSION" => "FIRMWARE?",
                "GET_TEMPERATURE" => "TEMP?",
                "GET_VOLTAGE" => "VOLTAGE?",
                "GET_CURRENT" => "CURRENT?",
                "GET_ACCELERATION" => "ACCEL?",
                "SET_ACCELERATION" => "ACCEL",
                "GET_JERK" => "JERK?",
                "SET_JERK" => "JERK",
                "GET_CORNER_SPEED" => "CORNER_SPEED?",
                "SET_CORNER_SPEED" => "CORNER_SPEED",
                "GET_JUMP_SPEED" => "JUMP_SPEED?",
                "SET_JUMP_SPEED" => "JUMP_SPEED",
                "GET_JUMP_DELAY" => "JUMP_DELAY?",
                "SET_JUMP_DELAY" => "JUMP_DELAY",
                "GET_MARK_DELAY" => "MARK_DELAY?",
                "SET_MARK_DELAY" => "MARK_DELAY",
                "GET_POLY_DELAY" => "POLY_DELAY?",
                "SET_POLY_DELAY" => "POLY_DELAY",
                "GET_LASER_DELAY" => "LASER_DELAY?",
                "SET_LASER_DELAY" => "LASER_DELAY",
                "GET_SKIP_DELAY" => "SKIP_DELAY?",
                "SET_SKIP_DELAY" => "SKIP_DELAY",
                "GET_ARC_DELAY" => "ARC_DELAY?",
                "SET_ARC_DELAY" => "ARC_DELAY",
                "GET_ARC_ANGLE" => "ARC_ANGLE?",
                "SET_ARC_ANGLE" => "ARC_ANGLE",
                "GET_ARC_CORNER" => "ARC_CORNER?",
                "SET_ARC_CORNER" => "ARC_CORNER",
                "GET_ARC_CORNER_SPEED" => "ARC_CORNER_SPEED?",
                "SET_ARC_CORNER_SPEED" => "ARC_CORNER_SPEED",
                "GET_ARC_CORNER_ACCEL" => "ARC_CORNER_ACCEL?",
                "SET_ARC_CORNER_ACCEL" => "ARC_CORNER_ACCEL",
                "GET_ARC_CORNER_JERK" => "ARC_CORNER_JERK?",
                "SET_ARC_CORNER_JERK" => "ARC_CORNER_JERK",
                "GET_ARC_CORNER_DELAY" => "ARC_CORNER_DELAY?",
                "SET_ARC_CORNER_DELAY" => "ARC_CORNER_DELAY",
                "GET_ARC_CORNER_MARK_DELAY" => "ARC_CORNER_MARK_DELAY?",
                "SET_ARC_CORNER_MARK_DELAY" => "ARC_CORNER_MARK_DELAY",
                "GET_ARC_CORNER_LASER_DELAY" => "ARC_CORNER_LASER_DELAY?",
                "SET_ARC_CORNER_LASER_DELAY" => "ARC_CORNER_LASER_DELAY",
                "GET_ARC_CORNER_SKIP_DELAY" => "ARC_CORNER_SKIP_DELAY?",
                "SET_ARC_CORNER_SKIP_DELAY" => "ARC_CORNER_SKIP_DELAY",
                "GET_ARC_CORNER_ARC_DELAY" => "ARC_CORNER_ARC_DELAY?",
                "SET_ARC_CORNER_ARC_DELAY" => "ARC_CORNER_ARC_DELAY",
                "GET_ARC_CORNER_ARC_ANGLE" => "ARC_CORNER_ARC_ANGLE?",
                "SET_ARC_CORNER_ARC_ANGLE" => "ARC_CORNER_ARC_ANGLE",
                "GET_ARC_CORNER_ARC_CORNER" => "ARC_CORNER_ARC_CORNER?",
                "SET_ARC_CORNER_ARC_CORNER" => "ARC_CORNER_ARC_CORNER",
                "GET_ARC_CORNER_ARC_CORNER_SPEED" => "ARC_CORNER_ARC_CORNER_SPEED?",
                "SET_ARC_CORNER_ARC_CORNER_SPEED" => "ARC_CORNER_ARC_CORNER_SPEED",
                "GET_ARC_CORNER_ARC_CORNER_ACCEL" => "ARC_CORNER_ARC_CORNER_ACCEL?",
                "SET_ARC_CORNER_ARC_CORNER_ACCEL" => "ARC_CORNER_ARC_CORNER_ACCEL",
                "GET_ARC_CORNER_ARC_CORNER_JERK" => "ARC_CORNER_ARC_CORNER_JERK?",
                "SET_ARC_CORNER_ARC_CORNER_JERK" => "ARC_CORNER_ARC_CORNER_JERK",
                "GET_ARC_CORNER_ARC_CORNER_DELAY" => "ARC_CORNER_ARC_CORNER_DELAY?",
                "SET_ARC_CORNER_ARC_CORNER_DELAY" => "ARC_CORNER_ARC_CORNER_DELAY",
                "GET_ARC_CORNER_ARC_CORNER_MARK_DELAY" => "ARC_CORNER_ARC_CORNER_MARK_DELAY?",
                "SET_ARC_CORNER_ARC_CORNER_MARK_DELAY" => "ARC_CORNER_ARC_CORNER_MARK_DELAY",
                "GET_ARC_CORNER_ARC_CORNER_LASER_DELAY" => "ARC_CORNER_ARC_CORNER_LASER_DELAY?",
                "SET_ARC_CORNER_ARC_CORNER_LASER_DELAY" => "ARC_CORNER_ARC_CORNER_LASER_DELAY",
                "GET_ARC_CORNER_ARC_CORNER_SKIP_DELAY" => "ARC_CORNER_ARC_CORNER_SKIP_DELAY?",
                "SET_ARC_CORNER_ARC_CORNER_SKIP_DELAY" => "ARC_CORNER_ARC_CORNER_SKIP_DELAY",
                "GET_ARC_CORNER_ARC_CORNER_ARC_DELAY" => "ARC_CORNER_ARC_CORNER_ARC_DELAY?",
                "SET_ARC_CORNER_ARC_CORNER_ARC_DELAY" => "ARC_CORNER_ARC_CORNER_ARC_DELAY",
                "GET_ARC_CORNER_ARC_CORNER_ARC_ANGLE" => "ARC_CORNER_ARC_CORNER_ARC_ANGLE?",
                "SET_ARC_CORNER_ARC_CORNER_ARC_ANGLE" => "ARC_CORNER_ARC_CORNER_ARC_ANGLE",
                "GET_ARC_CORNER_ARC_CORNER_ARC_CORNER" => "ARC_CORNER_ARC_CORNER_ARC_CORNER?",
                "SET_ARC_CORNER_ARC_CORNER_ARC_CORNER" => "ARC_CORNER_ARC_CORNER_ARC_CORNER",
                _ => throw new ArgumentException($"Unknown RTC4 command: {command}")
            };
        }

        private List<string> GetParameterCommands(LaserParameters parameters, ControllerType controllerType)
        {
            var commands = new List<string>();

            // Basic parameters
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_POWER")} {parameters.Power}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_SPEED")} {parameters.Speed}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_FREQUENCY")} {parameters.Frequency}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_PULSE_WIDTH")} {parameters.PulseWidth}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_FOCUS")} {parameters.FocusPosition}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_DEPTH")} {parameters.MarkingDepth}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_REPETITIONS")} {parameters.Repetitions}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_SPOT_SIZE")} {parameters.SpotSize}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_GALVO_ENABLE")} {(parameters.EnableGalvo ? "1" : "0")}");
            commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_GALVO_DELAY")} {parameters.GalvoDelay}");

            // RTC4 specific parameters
            if (controllerType == ControllerType.RTC4)
            {
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ACCELERATION")} 1000");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_JERK")} 100");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_CORNER_SPEED")} 500");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_JUMP_SPEED")} 2000");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_JUMP_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_MARK_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_POLY_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_LASER_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_SKIP_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_ANGLE")} 90");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_SPEED")} 500");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ACCEL")} 1000");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_JERK")} 100");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_MARK_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_LASER_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_SKIP_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_ANGLE")} 90");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_SPEED")} 500");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_ACCEL")} 1000");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_JERK")} 100");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_MARK_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_LASER_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_SKIP_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_ARC_DELAY")} 0.1");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_ARC_ANGLE")} 90");
                commands.Add($"{GetControllerSpecificCommand(controllerType, "SET_ARC_CORNER_ARC_CORNER_ARC_CORNER")} 0.1");
            }

            return commands;
        }

        private Dictionary<string, string> GetSystemInfoCommands(ControllerType controllerType)
        {
            var commands = new Dictionary<string, string>
            {
                { GetControllerSpecificCommand(controllerType, "GET_SERIAL_NUMBER"), "SerialNumber" },
                { GetControllerSpecificCommand(controllerType, "GET_FIRMWARE_VERSION"), "FirmwareVersion" },
                { GetControllerSpecificCommand(controllerType, "GET_TEMPERATURE"), "Temperature" },
                { GetControllerSpecificCommand(controllerType, "GET_VOLTAGE"), "Voltage" },
                { GetControllerSpecificCommand(controllerType, "GET_CURRENT"), "Current" }
            };

            return commands;
        }

        public bool IsConnected => _isConnected;
    }
}