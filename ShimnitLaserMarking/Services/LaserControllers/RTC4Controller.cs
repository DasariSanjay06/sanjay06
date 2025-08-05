using Serilog;
using ShimnitLaserMarking.Models;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ShimnitLaserMarking.Services.LaserControllers
{
    public class RTC4Controller : ILaserController
    {
        private SerialPort? _serialPort;
        private LaserParameters _parameters;
        private LaserStatus _status;
        private bool _disposed;
        private readonly object _lockObject = new object();
        private CancellationTokenSource? _operationCancellationSource;

        public LaserControllerType ControllerType => LaserControllerType.RTC4;
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

        public RTC4Controller(string name, string communicationPort)
        {
            Name = name;
            CommunicationPort = communicationPort;
            _parameters = LaserParameters.GetDefaultRTC4Parameters();
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
                        Log.Warning("RTC4 controller {Name} is already connected", Name);
                        return true;
                    }

                    _serialPort = new SerialPort(CommunicationPort, 115200, Parity.None, 8, StopBits.One);
                    _serialPort.Handshake = Handshake.None;
                    _serialPort.ReadTimeout = 5000;
                    _serialPort.WriteTimeout = 5000;
                    _serialPort.NewLine = "\r\n";
                    _serialPort.DtrEnable = true;
                    _serialPort.RtsEnable = true;
                }

                await Task.Run(() => _serialPort.Open());
                
                // RTC4 specific connection sequence
                await SendCommandAsync("RTC4_CONNECT");
                var response = await ReadResponseAsync();
                
                if (response.Contains("RTC4_OK"))
                {
                    SetStatus(LaserStatus.Connected);
                    Log.Information("RTC4 controller {Name} connected successfully", Name);
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
                Log.Error(ex, "Failed to connect to RTC4 controller {Name}", Name);
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (IsConnected)
                {
                    await SendCommandAsync("RTC4_DISCONNECT");
                    await Task.Delay(200); // Allow more time for RTC4 processing
                }

                lock (_lockObject)
                {
                    _serialPort?.Close();
                    _serialPort?.Dispose();
                    _serialPort = null;
                }

                SetStatus(LaserStatus.Disconnected);
                Log.Information("RTC4 controller {Name} disconnected", Name);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error disconnecting RTC4 controller {Name}", Name);
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

                // RTC4 specific initialization sequence
                await SendCommandAsync("RTC4_INIT_SCANNER");
                if (!await WaitForOk()) return false;

                // Load enhanced correction file for RTC4
                await SendCommandAsync("RTC4_LOAD_CORRECTION_FILE ENHANCED.cor");
                if (!await WaitForOk()) return false;

                // Initialize enhanced laser system
                await SendCommandAsync("RTC4_INIT_LASER_ENHANCED");
                if (!await WaitForOk()) return false;

                // Enable 3D capabilities if configured
                if (_parameters.Is3DMode)
                {
                    await SendCommandAsync("RTC4_ENABLE_3D_MODE");
                    if (!await WaitForOk()) return false;
                }

                // Set optical power control if enabled
                if (_parameters.OpticalPowerControl)
                {
                    await SendCommandAsync("RTC4_ENABLE_OPTICAL_POWER_CONTROL");
                    if (!await WaitForOk()) return false;
                }

                // Set safety parameters with RTC4 enhancements
                await SendCommandAsync($"RTC4_SET_LASER_ON_DELAY {_parameters.LaserOnDelay}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"RTC4_SET_LASER_OFF_DELAY {_parameters.LaserOffDelay}");
                if (!await WaitForOk()) return false;

                // Initialize temperature compensation
                if (_parameters.TemperatureCompensation)
                {
                    await SendCommandAsync("RTC4_ENABLE_TEMP_COMPENSATION");
                    if (!await WaitForOk()) return false;
                }

                SetStatus(LaserStatus.Ready);
                Log.Information("RTC4 controller {Name} initialized successfully", Name);
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Initialization error: {ex.Message}");
                Log.Error(ex, "Failed to initialize RTC4 controller {Name}", Name);
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

                // Set laser power with enhanced control
                if (parameters.OpticalPowerControl)
                {
                    await SendCommandAsync($"RTC4_SET_OPTICAL_POWER {parameters.Power}");
                }
                else
                {
                    await SendCommandAsync($"RTC4_SET_DIGITAL_POWER {parameters.Power}");
                }
                if (!await WaitForOk()) return false;

                // Set marking speed with acceleration control
                await SendCommandAsync($"RTC4_SET_MARK_SPEED {parameters.Speed} {parameters.ScannerAcceleration}");
                if (!await WaitForOk()) return false;

                // Set pulse frequency and width
                await SendCommandAsync($"RTC4_SET_PULSE_FREQ {parameters.PulseFrequency}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"RTC4_SET_PULSE_WIDTH {parameters.PulseWidth}");
                if (!await WaitForOk()) return false;

                // Set duty cycle
                await SendCommandAsync($"RTC4_SET_DUTY_CYCLE {parameters.DutyCycle}");
                if (!await WaitForOk()) return false;

                // Set enhanced timing parameters
                await SendCommandAsync($"RTC4_SET_JUMP_DELAY {parameters.JumpDelay}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"RTC4_SET_MARK_DELAY {parameters.MarkDelay}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"RTC4_SET_POLYGON_DELAY {parameters.PolygonDelay}");
                if (!await WaitForOk()) return false;

                // Set wobble parameters with enhanced control
                if (parameters.WobbleEnabled)
                {
                    await SendCommandAsync($"RTC4_ENABLE_WOBBLE {parameters.WobbleAmplitude} {parameters.WobbleFrequency}");
                    if (!await WaitForOk()) return false;
                }
                else
                {
                    await SendCommandAsync("RTC4_DISABLE_WOBBLE");
                    if (!await WaitForOk()) return false;
                }

                // Set 3D parameters if enabled
                if (parameters.Is3DMode)
                {
                    await SendCommandAsync($"RTC4_SET_Z_POSITION {parameters.ZAxisPosition}");
                    if (!await WaitForOk()) return false;
                }

                // Set line spacing and quality factor
                await SendCommandAsync($"RTC4_SET_LINE_SPACING {parameters.LineSpacing}");
                if (!await WaitForOk()) return false;

                await SendCommandAsync($"RTC4_SET_QUALITY_FACTOR {parameters.QualityFactor}");
                if (!await WaitForOk()) return false;

                _parameters = parameters;
                Log.Information("Enhanced parameters set successfully for RTC4 controller {Name}", Name);
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Parameter setting error: {ex.Message}");
                Log.Error(ex, "Failed to set parameters for RTC4 controller {Name}", Name);
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

                var startTime = DateTime.Now;

                // Enhanced positioning with 3D support
                if (_parameters.Is3DMode)
                {
                    await SendCommandAsync($"RTC4_MOVE_TO_3D {x} {y} {_parameters.ZAxisPosition}");
                }
                else
                {
                    await SendCommandAsync($"RTC4_MOVE_TO {x} {y}");
                }
                if (!await WaitForOk()) return false;

                // Set rotation with enhanced precision
                if (rotation != 0)
                {
                    await SendCommandAsync($"RTC4_SET_ROTATION {rotation}");
                    if (!await WaitForOk()) return false;
                }

                // Set focus position if specified
                if (_parameters.FocusPosition != 0)
                {
                    await SendCommandAsync($"RTC4_SET_FOCUS {_parameters.FocusPosition}");
                    if (!await WaitForOk()) return false;
                }

                // Start enhanced marking operation
                await SendCommandAsync($"RTC4_MARK_TEXT_ENHANCED \"{text}\" {_parameters.Passes}");

                // Enhanced progress monitoring
                var progressTask = Task.Run(async () =>
                {
                    int progress = 0;
                    int lastProgress = -1;

                    while (progress < 100 && !_operationCancellationSource.Token.IsCancellationRequested)
                    {
                        await Task.Delay(300, _operationCancellationSource.Token); // Faster updates for RTC4
                        
                        // Query enhanced progress with more details
                        await SendCommandAsync("RTC4_GET_PROGRESS_DETAILED");
                        var response = await ReadResponseAsync();
                        
                        var parts = response.Split(' ');
                        if (parts.Length >= 2 && int.TryParse(parts[0], out progress))
                        {
                            if (progress != lastProgress)
                            {
                                lastProgress = progress;
                                var elapsed = DateTime.Now - startTime;
                                var estimated = progress > 0 ? TimeSpan.FromTicks(elapsed.Ticks * 100 / progress) - elapsed : null;
                                
                                string operation = parts.Length > 1 ? parts[1] : "Marking text";
                                
                                MarkingProgress?.Invoke(this, new MarkingProgressEventArgs
                                {
                                    PercentComplete = progress,
                                    CurrentOperation = operation,
                                    ElapsedTime = elapsed,
                                    EstimatedTimeRemaining = estimated
                                });
                            }
                        }
                    }
                });

                await progressTask;

                var totalTime = DateTime.Now - startTime;

                if (!await WaitForOk())
                {
                    MarkingCompleted?.Invoke(this, new MarkingCompletedEventArgs
                    {
                        Success = false,
                        ErrorMessage = LastError,
                        TotalTime = totalTime
                    });
                    return false;
                }

                SetStatus(LaserStatus.Ready);
                
                // Enhanced quality assessment
                await SendCommandAsync("RTC4_GET_QUALITY_ASSESSMENT");
                var qualityResponse = await ReadResponseAsync();
                int qualityScore = 9; // Default high quality for RTC4
                
                if (int.TryParse(qualityResponse.Split(' ')[0], out int parsedQuality))
                {
                    qualityScore = parsedQuality;
                }

                MarkingCompleted?.Invoke(this, new MarkingCompletedEventArgs
                {
                    Success = true,
                    TotalTime = totalTime,
                    QualityScore = qualityScore
                });

                Log.Information("Enhanced marking completed successfully on RTC4 controller {Name}", Name);
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Marking error: {ex.Message}");
                SetStatus(LaserStatus.Ready);
                Log.Error(ex, "Failed to complete marking on RTC4 controller {Name}", Name);
                return false;
            }
        }

        public async Task<bool> StopMarkingAsync()
        {
            try
            {
                _operationCancellationSource?.Cancel();
                await SendCommandAsync("RTC4_STOP_MARKING");
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
                await SendCommandAsync("RTC4_PAUSE_MARKING");
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
                await SendCommandAsync("RTC4_RESUME_MARKING");
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
                if (_parameters.Is3DMode)
                {
                    await SendCommandAsync($"RTC4_MOVE_TO_3D {x} {y} {z}");
                }
                else
                {
                    await SendCommandAsync($"RTC4_MOVE_TO {x} {y}");
                }
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
                await SendCommandAsync("RTC4_GET_POSITION_3D");
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
                await SendCommandAsync("RTC4_TEST_LASER_ENHANCED");
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
                await SendCommandAsync("RTC4_CALIBRATE_ENHANCED");
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
                await SendCommandAsync("RTC4_GET_STATUS_ENHANCED");
                var response = await ReadResponseAsync();
                
                // Parse enhanced status response
                var parts = response.Split('|');
                
                return new ControllerStatus
                {
                    Status = _status,
                    Temperature = parts.Length > 1 && double.TryParse(parts[1], out double temp) ? temp : 23.0,
                    PowerLevel = _parameters.Power,
                    CurrentSpeed = _parameters.Speed,
                    LaserEnabled = true,
                    SafetySystemActive = true,
                    OperatingHours = parts.Length > 2 && double.TryParse(parts[2], out double hours) ? hours : 150.0,
                    MarkingJobsCompleted = parts.Length > 3 && int.TryParse(parts[3], out int jobs) ? jobs : 75,
                    FirmwareVersion = "RTC4-2.1.0",
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
                await SendCommandAsync("RTC4_RESET");
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
            Log.Debug("Sent command to RTC4: {Command}", command);
        }

        private async Task<string> ReadResponseAsync()
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                throw new InvalidOperationException("Serial port not open");

            var response = await Task.Run(() => _serialPort.ReadLine());
            Log.Debug("Received response from RTC4: {Response}", response);
            return response.Trim();
        }

        private async Task<bool> WaitForOk()
        {
            try
            {
                var response = await ReadResponseAsync();
                return response.Contains("RTC4_OK") || response.Contains("OK");
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