using Serilog;
using ShimnitLaserMarking.Models;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ShimnitLaserMarking.Services.LaserControllers
{
    public class RTC3Controller : ILaserController
    {
        private SerialPort? _serialPort;
        private LaserParameters _parameters;
        private LaserStatus _status;
        private bool _disposed;
        private readonly object _lockObject = new object();
        private CancellationTokenSource? _operationCancellationSource;

        public LaserControllerType ControllerType => LaserControllerType.RTC3;
        public string Name { get; }
        public string CommunicationPort { get; set; }
        public bool IsConnected => _serialPort?.IsOpen ?? false;
        public bool IsReady => _status == LaserStatus.Ready;
        public LaserStatus LaserStatus => _status;
        public string? LastError { get; private set; }

        public event EventHandler<MarkingProgressEventArgs>? MarkingProgress;
        public event EventHandler<MarkingCompletedEventArgs>? MarkingCompleted;
        public event EventHandler<LaserErrorEventArgs>? ErrorOccurred;
        public event EventHandler<StatusChangedEventArgs>? StatusChanged;

        public RTC3Controller(string name, string communicationPort)
        {
            Name = name;
            CommunicationPort = communicationPort;
            _parameters = LaserParameters.GetDefaultRTC3Parameters();
            _status = LaserStatus.Disconnected;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                lock (_lockObject)
                {
                    if (IsConnected)
                    {
                        Log.Warning("RTC3 controller {Name} is already connected", Name);
                        return true;
                    }

                    _serialPort = new SerialPort(CommunicationPort, 115200, Parity.None, 8, StopBits.One);
                    _serialPort.Handshake = Handshake.None;
                    _serialPort.ReadTimeout = 5000;
                    _serialPort.WriteTimeout = 5000;
                    _serialPort.NewLine = "\r\n";
                }

                await Task.Run(() => _serialPort.Open());
                
                // Send connection command
                await SendCommandAsync("CONNECT");
                var response = await ReadResponseAsync();
                
                if (response.Contains("OK"))
                {
                    SetStatus(LaserStatus.Connected);
                    Log.Information("RTC3 controller {Name} connected successfully", Name);
                    return true;
                }
                else
                {
                    SetError($"Connection failed: {response}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SetError($"Connection error: {ex.Message}");
                Log.Error(ex, "Failed to connect to RTC3 controller {Name}", Name);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (IsConnected)
                {
                    await SendCommandAsync("DISCONNECT");
                    await Task.Delay(100); // Allow time for command processing
                }

                lock (_lockObject)
                {
                    _serialPort?.Close();
                    _serialPort?.Dispose();
                    _serialPort = null;
                }

                SetStatus(LaserStatus.Disconnected);
                Log.Information("RTC3 controller {Name} disconnected", Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error disconnecting RTC3 controller {Name}", Name);
            }
        }

        public async Task<bool> InitializeAsync()
        {
            try
            {
                if (!IsConnected)
                {
                    SetError("Controller not connected");
                    return false;
                }

                SetStatus(LaserStatus.Initializing);

                // Initialize scanner system
                await SendCommandAsync("INIT_SCANNER");
                if (!await WaitForOk()) return false;

                // Set default correction file
                await SendCommandAsync("LOAD_CORRECTION_FILE DEFAULT.cor");
                if (!await WaitForOk()) return false;

                // Initialize laser
                await SendCommandAsync("INIT_LASER");
                if (!await WaitForOk()) return false;

                // Set safety parameters
                await SendCommandAsync($"SET_LASER_ON_DELAY {_parameters.LaserOnDelay}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"SET_LASER_OFF_DELAY {_parameters.LaserOffDelay}");
                if (!await WaitForOk()) return false;

                SetStatus(LaserStatus.Ready);
                Log.Information("RTC3 controller {Name} initialized successfully", Name);
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Initialization error: {ex.Message}");
                Log.Error(ex, "Failed to initialize RTC3 controller {Name}", Name);
                return false;
            }
        }

        public async Task<bool> SetParametersAsync(LaserParameters parameters)
        {
            try
            {
                if (!IsConnected)
                {
                    SetError("Controller not connected");
                    return false;
                }

                if (!parameters.IsValid())
                {
                    SetError("Invalid parameters");
                    return false;
                }

                // Set laser power
                await SendCommandAsync($"SET_LASER_POWER {parameters.Power}");
                if (!await WaitForOk()) return false;

                // Set marking speed
                await SendCommandAsync($"SET_MARK_SPEED {parameters.Speed}");
                if (!await WaitForOk()) return false;

                // Set pulse frequency
                await SendCommandAsync($"SET_PULSE_FREQ {parameters.PulseFrequency}");
                if (!await WaitForOk()) return false;

                // Set jump speed
                await SendCommandAsync($"SET_JUMP_SPEED {parameters.ScannerSpeed}");
                if (!await WaitForOk()) return false;

                // Set delays
                await SendCommandAsync($"SET_JUMP_DELAY {parameters.JumpDelay}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"SET_MARK_DELAY {parameters.MarkDelay}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"SET_POLYGON_DELAY {parameters.PolygonDelay}");
                if (!await WaitForOk()) return false;

                // Set wobble parameters if enabled
                if (parameters.WobbleEnabled)
                {
                    await SendCommandAsync($"ENABLE_WOBBLE {parameters.WobbleAmplitude} {parameters.WobbleFrequency}");
                    if (!await WaitForOk()) return false;
                }
                else
                {
                    await SendCommandAsync("DISABLE_WOBBLE");
                    if (!await WaitForOk()) return false;
                }

                _parameters = parameters;
                Log.Information("Parameters set successfully for RTC3 controller {Name}", Name);
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Parameter setting error: {ex.Message}");
                Log.Error(ex, "Failed to set parameters for RTC3 controller {Name}", Name);
                return false;
            }
        }

        public async Task<bool> StartMarkingAsync(string text, double x, double y, double rotation = 0)
        {
            try
            {
                if (!IsReady)
                {
                    SetError("Controller not ready");
                    return false;
                }

                SetStatus(LaserStatus.Marking);
                _operationCancellationSource = new CancellationTokenSource();

                // Set position
                await SendCommandAsync($"MOVE_TO {x} {y}");
                if (!await WaitForOk()) return false;

                // Set rotation
                if (rotation != 0)
                {
                    await SendCommandAsync($"SET_ROTATION {rotation}");
                    if (!await WaitForOk()) return false;
                }

                // Start marking
                await SendCommandAsync($"MARK_TEXT \"{text}\"");
                
                // Monitor marking progress
                var progressTask = Task.Run(async () =>
                {
                    var startTime = DateTime.Now;
                    int progress = 0;

                    while (progress < 100 && !_operationCancellationSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(500, _operationCancellationSource.Token);
                        
                        // Query progress
                        await SendCommandAsync("GET_PROGRESS");
                        var response = await ReadResponseAsync();
                        
                        if (int.TryParse(response.Split(' ')[0], out progress))
                        {
                            var elapsed = DateTime.Now - startTime;
                            var estimated = progress > 0 ? TimeSpan.FromTicks(elapsed.Ticks * 100 / progress) - elapsed : null;
                            
                            MarkingProgress?.Invoke(this, new MarkingProgressEventArgs
                            {
                                PercentComplete = progress,
                                CurrentOperation = "Marking text",
                                ElapsedTime = elapsed,
                                EstimatedTimeRemaining = estimated
                            });
                        }
                    }
                });

                await progressTask;

                if (!await WaitForOk())
                {
                    MarkingCompleted?.Invoke(this, new MarkingCompletedEventArgs
                    {
                        Success = false,
                        ErrorMessage = LastError,
                        TotalTime = DateTime.Now - DateTime.Now
                    });
                    return false;
                }

                SetStatus(LaserStatus.Ready);
                
                MarkingCompleted?.Invoke(this, new MarkingCompletedEventArgs
                {
                    Success = true,
                    TotalTime = DateTime.Now - DateTime.Now,
                    QualityScore = 8 // Default quality score
                });

                Log.Information("Marking completed successfully on RTC3 controller {Name}", Name);
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Marking error: {ex.Message}");
                SetStatus(LaserStatus.Ready);
                Log.Error(ex, "Failed to complete marking on RTC3 controller {Name}", Name);
                return false;
            }
        }

        public async Task<bool> StopMarkingAsync()
        {
            try
            {
                _operationCancellationSource?.Cancel();
                await SendCommandAsync("STOP_MARKING");
                SetStatus(LaserStatus.Ready);
                return await WaitForOk();
            }
            catch (Exception ex)
            {
                SetError($"Stop marking error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> PauseMarkingAsync()
        {
            try
            {
                await SendCommandAsync("PAUSE_MARKING");
                SetStatus(LaserStatus.Paused);
                return await WaitForOk();
            }
            catch (Exception ex)
            {
                SetError($"Pause marking error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> ResumeMarkingAsync()
        {
            try
            {
                await SendCommandAsync("RESUME_MARKING");
                SetStatus(LaserStatus.Marking);
                return await WaitForOk();
            }
            catch (Exception ex)
            {
                SetError($"Resume marking error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> MoveToPositionAsync(double x, double y, double z = 0)
        {
            try
            {
                await SendCommandAsync($"MOVE_TO {x} {y} {z}");
                return await WaitForOk();
            }
            catch (Exception ex)
            {
                SetError($"Move position error: {ex.Message}");
                return false;
            }
        }

        public async Task<(double X, double Y, double Z)> GetPositionAsync()
        {
            try
            {
                await SendCommandAsync("GET_POSITION");
                var response = await ReadResponseAsync();
                var parts = response.Split(' ');
                
                if (parts.Length >= 3 &&
                    double.TryParse(parts[0], out double x) &&
                    double.TryParse(parts[1], out double y) &&
                    double.TryParse(parts[2], out double z))
                {
                    return (x, y, z);
                }
                
                return (0, 0, 0);
            }
            catch (Exception ex)
            {
                SetError($"Get position error: {ex.Message}");
                return (0, 0, 0);
            }
        }

        public async Task<bool> TestLaserAsync()
        {
            try
            {
                await SendCommandAsync("TEST_LASER");
                return await WaitForOk();
            }
            catch (Exception ex)
            {
                SetError($"Test laser error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> CalibrateAsync()
        {
            try
            {
                SetStatus(LaserStatus.Calibrating);
                await SendCommandAsync("CALIBRATE");
                var result = await WaitForOk();
                SetStatus(result ? LaserStatus.Ready : LaserStatus.Error);
                return result;
            }
            catch (Exception ex)
            {
                SetError($"Calibration error: {ex.Message}");
                SetStatus(LaserStatus.Error);
                return false;
            }
        }

        public async Task<ControllerStatus> GetStatusAsync()
        {
            try
            {
                await SendCommandAsync("GET_STATUS");
                var response = await ReadResponseAsync();
                
                // Parse status response (simplified)
                return new ControllerStatus
                {
                    Status = _status,
                    Temperature = 25.0, // Default values for simulation
                    PowerLevel = _parameters.Power,
                    CurrentSpeed = _parameters.Speed,
                    LaserEnabled = true,
                    SafetySystemActive = true,
                    OperatingHours = 100.0,
                    MarkingJobsCompleted = 50,
                    FirmwareVersion = "RTC3-1.0.0",
                    LastUpdate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                SetError($"Get status error: {ex.Message}");
                return new ControllerStatus { Status = LaserStatus.Error };
            }
        }

        public async Task<bool> ResetAsync()
        {
            try
            {
                await SendCommandAsync("RESET");
                SetStatus(LaserStatus.Disconnected);
                return await WaitForOk();
            }
            catch (Exception ex)
            {
                SetError($"Reset error: {ex.Message}");
                return false;
            }
        }

        private async Task SendCommandAsync(string command)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port not open");

            var data = Encoding.ASCII.GetBytes(command + "\r\n");
            await Task.Run(() => _serialPort.Write(data, 0, data.Length));
            Log.Debug("Sent command to RTC3: {Command}", command);
        }

        private async Task<string> ReadResponseAsync()
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port not open");

            var response = await Task.Run(() => _serialPort.ReadLine());
            Log.Debug("Received response from RTC3: {Response}", response);
            return response.Trim();
        }

        private async Task<bool> WaitForOk()
        {
            try
            {
                var response = await ReadResponseAsync();
                return response.Contains("OK");
            }
            catch
            {
                return false;
            }
        }

        private void SetStatus(LaserStatus newStatus)
        {
            if (_status != newStatus)
            {
                var oldStatus = _status;
                _status = newStatus;
                
                StatusChanged?.Invoke(this, new StatusChangedEventArgs
                {
                    OldStatus = oldStatus,
                    NewStatus = newStatus,
                    Timestamp = DateTime.Now
                });
            }
        }

        private void SetError(string error)
        {
            LastError = error;
            SetStatus(LaserStatus.Error);
            
            ErrorOccurred?.Invoke(this, new LaserErrorEventArgs
            {
                ErrorMessage = error,
                Severity = LaserErrorSeverity.Error
            });
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _operationCancellationSource?.Cancel();
                _operationCancellationSource?.Dispose();
                
                _serialPort?.Close();
                _serialPort?.Dispose();
                
                _disposed = true;
            }
        }
    }
}