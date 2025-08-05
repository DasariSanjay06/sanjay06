using System.Data.SQLite;
using LaserMarkingSystem.Models;
using Serilog;

namespace LaserMarkingSystem.Database
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly ILogger _logger;

        public DatabaseService()
        {
            var dbPath = Path.Combine(Application.StartupPath, "LaserMarkingSystem.db");
            _connectionString = $"Data Source={dbPath};Version=3;";
            _logger = Log.ForContext<DatabaseService>();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            try
            {
                using var connection = new SQLiteConnection(_connectionString);
                connection.Open();

                // Create tables
                CreateTables(connection);
                _logger.Information("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize database");
                throw;
            }
        }

        private void CreateTables(SQLiteConnection connection)
        {
            // Laser Controllers table
            var createControllersTable = @"
                CREATE TABLE IF NOT EXISTS LaserControllers (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    IpAddress TEXT NOT NULL,
                    Port INTEGER NOT NULL,
                    Status TEXT NOT NULL,
                    IsConnected INTEGER NOT NULL,
                    LastConnected TEXT,
                    SerialNumber TEXT,
                    FirmwareVersion TEXT
                )";

            // Laser Parameters table
            var createParametersTable = @"
                CREATE TABLE IF NOT EXISTS LaserParameters (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Power REAL NOT NULL,
                    Speed REAL NOT NULL,
                    Frequency REAL NOT NULL,
                    PulseWidth REAL NOT NULL,
                    FocusPosition REAL NOT NULL,
                    MarkingDepth REAL NOT NULL,
                    Repetitions INTEGER NOT NULL,
                    SpotSize REAL NOT NULL,
                    EnableGalvo INTEGER NOT NULL,
                    GalvoDelay REAL NOT NULL,
                    Material TEXT NOT NULL,
                    Description TEXT,
                    CreatedDate TEXT NOT NULL,
                    ModifiedDate TEXT NOT NULL
                )";

            // Marking Jobs table
            var createJobsTable = @"
                CREATE TABLE IF NOT EXISTS MarkingJobs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    JobName TEXT NOT NULL,
                    NumberPlate TEXT NOT NULL,
                    VehicleType TEXT NOT NULL,
                    CustomerName TEXT NOT NULL,
                    MarkingText TEXT NOT NULL,
                    MarkingPattern TEXT,
                    ParametersId INTEGER,
                    ScheduledTime TEXT NOT NULL,
                    CompletedTime TEXT,
                    Status TEXT NOT NULL,
                    ErrorMessage TEXT,
                    Priority INTEGER NOT NULL,
                    OperatorName TEXT NOT NULL,
                    Notes TEXT,
                    FOREIGN KEY(ParametersId) REFERENCES LaserParameters(Id)
                )";

            // Marking Patterns table
            var createPatternsTable = @"
                CREATE TABLE IF NOT EXISTS MarkingPatterns (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Description TEXT,
                    Width REAL NOT NULL,
                    Height REAL NOT NULL,
                    PatternData TEXT NOT NULL,
                    CreatedDate TEXT NOT NULL,
                    ModifiedDate TEXT NOT NULL
                )";

            // System Settings table
            var createSettingsTable = @"
                CREATE TABLE IF NOT EXISTS SystemSettings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL,
                    Description TEXT
                )";

            using var command = connection.CreateCommand();
            command.CommandText = createControllersTable;
            command.ExecuteNonQuery();

            command.CommandText = createParametersTable;
            command.ExecuteNonQuery();

            command.CommandText = createJobsTable;
            command.ExecuteNonQuery();

            command.CommandText = createPatternsTable;
            command.ExecuteNonQuery();

            command.CommandText = createSettingsTable;
            command.ExecuteNonQuery();

            // Insert default settings
            InsertDefaultSettings(connection);
        }

        private void InsertDefaultSettings(SQLiteConnection connection)
        {
            var defaultSettings = new Dictionary<string, (string, string)>
            {
                { "DefaultController", ("RTC4", "Default laser controller type") },
                { "DefaultPower", ("50", "Default laser power in Watts") },
                { "DefaultSpeed", ("1000", "Default marking speed in mm/s") },
                { "DefaultFrequency", ("20000", "Default laser frequency in Hz") },
                { "CompanyName", ("Shimnit India", "Company name for reports") },
                { "LogLevel", ("Information", "Application log level") },
                { "AutoSaveInterval", ("300", "Auto save interval in seconds") }
            };

            foreach (var setting in defaultSettings)
            {
                var insertCommand = @"
                    INSERT OR IGNORE INTO SystemSettings (Key, Value, Description)
                    VALUES (@Key, @Value, @Description)";

                using var command = connection.CreateCommand();
                command.CommandText = insertCommand;
                command.Parameters.AddWithValue("@Key", setting.Key);
                command.Parameters.AddWithValue("@Value", setting.Value.Item1);
                command.Parameters.AddWithValue("@Description", setting.Value.Item2);
                command.ExecuteNonQuery();
            }
        }

        // Laser Controller operations
        public List<LaserController> GetLaserControllers()
        {
            var controllers = new List<LaserController>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM LaserControllers ORDER BY Name";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                controllers.Add(new LaserController
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Type = Enum.Parse<ControllerType>(reader.GetString(reader.GetOrdinal("Type"))),
                    IpAddress = reader.GetString(reader.GetOrdinal("IpAddress")),
                    Port = reader.GetInt32(reader.GetOrdinal("Port")),
                    Status = Enum.Parse<LaserStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                    IsConnected = reader.GetBoolean(reader.GetOrdinal("IsConnected")),
                    LastConnected = reader.IsDBNull(reader.GetOrdinal("LastConnected")) ? DateTime.MinValue : DateTime.Parse(reader.GetString(reader.GetOrdinal("LastConnected"))),
                    SerialNumber = reader.GetString(reader.GetOrdinal("SerialNumber")),
                    FirmwareVersion = reader.GetString(reader.GetOrdinal("FirmwareVersion"))
                });
            }

            return controllers;
        }

        public void SaveLaserController(LaserController controller)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            if (controller.Id == 0)
            {
                command.CommandText = @"
                    INSERT INTO LaserControllers (Name, Type, IpAddress, Port, Status, IsConnected, LastConnected, SerialNumber, FirmwareVersion)
                    VALUES (@Name, @Type, @IpAddress, @Port, @Status, @IsConnected, @LastConnected, @SerialNumber, @FirmwareVersion)";
            }
            else
            {
                command.CommandText = @"
                    UPDATE LaserControllers 
                    SET Name=@Name, Type=@Type, IpAddress=@IpAddress, Port=@Port, Status=@Status, 
                        IsConnected=@IsConnected, LastConnected=@LastConnected, SerialNumber=@SerialNumber, FirmwareVersion=@FirmwareVersion
                    WHERE Id=@Id";
                command.Parameters.AddWithValue("@Id", controller.Id);
            }

            command.Parameters.AddWithValue("@Name", controller.Name);
            command.Parameters.AddWithValue("@Type", controller.Type.ToString());
            command.Parameters.AddWithValue("@IpAddress", controller.IpAddress);
            command.Parameters.AddWithValue("@Port", controller.Port);
            command.Parameters.AddWithValue("@Status", controller.Status.ToString());
            command.Parameters.AddWithValue("@IsConnected", controller.IsConnected);
            command.Parameters.AddWithValue("@LastConnected", controller.LastConnected.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@SerialNumber", controller.SerialNumber);
            command.Parameters.AddWithValue("@FirmwareVersion", controller.FirmwareVersion);

            command.ExecuteNonQuery();
        }

        // Laser Parameters operations
        public List<LaserParameters> GetLaserParameters()
        {
            var parameters = new List<LaserParameters>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM LaserParameters ORDER BY Name";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                parameters.Add(new LaserParameters
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Power = reader.GetDouble(reader.GetOrdinal("Power")),
                    Speed = reader.GetDouble(reader.GetOrdinal("Speed")),
                    Frequency = reader.GetDouble(reader.GetOrdinal("Frequency")),
                    PulseWidth = reader.GetDouble(reader.GetOrdinal("PulseWidth")),
                    FocusPosition = reader.GetDouble(reader.GetOrdinal("FocusPosition")),
                    MarkingDepth = reader.GetDouble(reader.GetOrdinal("MarkingDepth")),
                    Repetitions = reader.GetInt32(reader.GetOrdinal("Repetitions")),
                    SpotSize = reader.GetDouble(reader.GetOrdinal("SpotSize")),
                    EnableGalvo = reader.GetBoolean(reader.GetOrdinal("EnableGalvo")),
                    GalvoDelay = reader.GetDouble(reader.GetOrdinal("GalvoDelay")),
                    Material = reader.GetString(reader.GetOrdinal("Material")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    CreatedDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedDate"))),
                    ModifiedDate = DateTime.Parse(reader.GetString(reader.GetOrdinal("ModifiedDate")))
                });
            }

            return parameters;
        }

        public void SaveLaserParameters(LaserParameters parameters)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            if (parameters.Id == 0)
            {
                command.CommandText = @"
                    INSERT INTO LaserParameters (Name, Power, Speed, Frequency, PulseWidth, FocusPosition, MarkingDepth, 
                        Repetitions, SpotSize, EnableGalvo, GalvoDelay, Material, Description, CreatedDate, ModifiedDate)
                    VALUES (@Name, @Power, @Speed, @Frequency, @PulseWidth, @FocusPosition, @MarkingDepth, 
                        @Repetitions, @SpotSize, @EnableGalvo, @GalvoDelay, @Material, @Description, @CreatedDate, @ModifiedDate)";
            }
            else
            {
                command.CommandText = @"
                    UPDATE LaserParameters 
                    SET Name=@Name, Power=@Power, Speed=@Speed, Frequency=@Frequency, PulseWidth=@PulseWidth, 
                        FocusPosition=@FocusPosition, MarkingDepth=@MarkingDepth, Repetitions=@Repetitions, 
                        SpotSize=@SpotSize, EnableGalvo=@EnableGalvo, GalvoDelay=@GalvoDelay, Material=@Material, 
                        Description=@Description, ModifiedDate=@ModifiedDate
                    WHERE Id=@Id";
                command.Parameters.AddWithValue("@Id", parameters.Id);
            }

            command.Parameters.AddWithValue("@Name", parameters.Name);
            command.Parameters.AddWithValue("@Power", parameters.Power);
            command.Parameters.AddWithValue("@Speed", parameters.Speed);
            command.Parameters.AddWithValue("@Frequency", parameters.Frequency);
            command.Parameters.AddWithValue("@PulseWidth", parameters.PulseWidth);
            command.Parameters.AddWithValue("@FocusPosition", parameters.FocusPosition);
            command.Parameters.AddWithValue("@MarkingDepth", parameters.MarkingDepth);
            command.Parameters.AddWithValue("@Repetitions", parameters.Repetitions);
            command.Parameters.AddWithValue("@SpotSize", parameters.SpotSize);
            command.Parameters.AddWithValue("@EnableGalvo", parameters.EnableGalvo);
            command.Parameters.AddWithValue("@GalvoDelay", parameters.GalvoDelay);
            command.Parameters.AddWithValue("@Material", parameters.Material);
            command.Parameters.AddWithValue("@Description", parameters.Description);
            command.Parameters.AddWithValue("@CreatedDate", parameters.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@ModifiedDate", parameters.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();
        }

        // Marking Jobs operations
        public List<MarkingJob> GetMarkingJobs()
        {
            var jobs = new List<MarkingJob>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT j.*, p.* FROM MarkingJobs j 
                LEFT JOIN LaserParameters p ON j.ParametersId = p.Id 
                ORDER BY j.ScheduledTime DESC";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var job = new MarkingJob
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    JobName = reader.GetString(reader.GetOrdinal("JobName")),
                    NumberPlate = reader.GetString(reader.GetOrdinal("NumberPlate")),
                    VehicleType = reader.GetString(reader.GetOrdinal("VehicleType")),
                    CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                    MarkingText = reader.GetString(reader.GetOrdinal("MarkingText")),
                    MarkingPattern = reader.GetString(reader.GetOrdinal("MarkingPattern")),
                    ScheduledTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("ScheduledTime"))),
                    Status = Enum.Parse<JobStatus>(reader.GetString(reader.GetOrdinal("Status"))),
                    Priority = reader.GetInt32(reader.GetOrdinal("Priority")),
                    OperatorName = reader.GetString(reader.GetOrdinal("OperatorName")),
                    Notes = reader.GetString(reader.GetOrdinal("Notes"))
                };

                if (!reader.IsDBNull(reader.GetOrdinal("CompletedTime")))
                    job.CompletedTime = DateTime.Parse(reader.GetString(reader.GetOrdinal("CompletedTime")));

                if (!reader.IsDBNull(reader.GetOrdinal("ErrorMessage")))
                    job.ErrorMessage = reader.GetString(reader.GetOrdinal("ErrorMessage"));

                if (!reader.IsDBNull(reader.GetOrdinal("ParametersId")))
                {
                    job.Parameters = new LaserParameters
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("ParametersId")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Power = reader.GetDouble(reader.GetOrdinal("Power")),
                        Speed = reader.GetDouble(reader.GetOrdinal("Speed")),
                        Frequency = reader.GetDouble(reader.GetOrdinal("Frequency")),
                        PulseWidth = reader.GetDouble(reader.GetOrdinal("PulseWidth")),
                        FocusPosition = reader.GetDouble(reader.GetOrdinal("FocusPosition")),
                        MarkingDepth = reader.GetDouble(reader.GetOrdinal("MarkingDepth")),
                        Repetitions = reader.GetInt32(reader.GetOrdinal("Repetitions")),
                        SpotSize = reader.GetDouble(reader.GetOrdinal("SpotSize")),
                        EnableGalvo = reader.GetBoolean(reader.GetOrdinal("EnableGalvo")),
                        GalvoDelay = reader.GetDouble(reader.GetOrdinal("GalvoDelay")),
                        Material = reader.GetString(reader.GetOrdinal("Material")),
                        Description = reader.GetString(reader.GetOrdinal("Description"))
                    };
                }

                jobs.Add(job);
            }

            return jobs;
        }

        public void SaveMarkingJob(MarkingJob job)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            // First save the parameters if it's new
            if (job.Parameters.Id == 0)
            {
                SaveLaserParameters(job.Parameters);
            }

            var command = connection.CreateCommand();
            if (job.Id == 0)
            {
                command.CommandText = @"
                    INSERT INTO MarkingJobs (JobName, NumberPlate, VehicleType, CustomerName, MarkingText, MarkingPattern, 
                        ParametersId, ScheduledTime, CompletedTime, Status, ErrorMessage, Priority, OperatorName, Notes)
                    VALUES (@JobName, @NumberPlate, @VehicleType, @CustomerName, @MarkingText, @MarkingPattern, 
                        @ParametersId, @ScheduledTime, @CompletedTime, @Status, @ErrorMessage, @Priority, @OperatorName, @Notes)";
            }
            else
            {
                command.CommandText = @"
                    UPDATE MarkingJobs 
                    SET JobName=@JobName, NumberPlate=@NumberPlate, VehicleType=@VehicleType, CustomerName=@CustomerName, 
                        MarkingText=@MarkingText, MarkingPattern=@MarkingPattern, ParametersId=@ParametersId, 
                        ScheduledTime=@ScheduledTime, CompletedTime=@CompletedTime, Status=@Status, ErrorMessage=@ErrorMessage, 
                        Priority=@Priority, OperatorName=@OperatorName, Notes=@Notes
                    WHERE Id=@Id";
                command.Parameters.AddWithValue("@Id", job.Id);
            }

            command.Parameters.AddWithValue("@JobName", job.JobName);
            command.Parameters.AddWithValue("@NumberPlate", job.NumberPlate);
            command.Parameters.AddWithValue("@VehicleType", job.VehicleType);
            command.Parameters.AddWithValue("@CustomerName", job.CustomerName);
            command.Parameters.AddWithValue("@MarkingText", job.MarkingText);
            command.Parameters.AddWithValue("@MarkingPattern", job.MarkingPattern);
            command.Parameters.AddWithValue("@ParametersId", job.Parameters.Id);
            command.Parameters.AddWithValue("@ScheduledTime", job.ScheduledTime.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@CompletedTime", job.CompletedTime?.ToString("yyyy-MM-dd HH:mm:ss") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Status", job.Status.ToString());
            command.Parameters.AddWithValue("@ErrorMessage", job.ErrorMessage ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Priority", job.Priority);
            command.Parameters.AddWithValue("@OperatorName", job.OperatorName);
            command.Parameters.AddWithValue("@Notes", job.Notes ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }

        // System Settings operations
        public string GetSetting(string key, string defaultValue = "")
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM SystemSettings WHERE Key = @Key";
            command.Parameters.AddWithValue("@Key", key);

            var result = command.ExecuteScalar();
            return result?.ToString() ?? defaultValue;
        }

        public void SetSetting(string key, string value, string description = "")
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO SystemSettings (Key, Value, Description)
                VALUES (@Key, @Value, @Description)";
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Value", value);
            command.Parameters.AddWithValue("@Description", description);

            command.ExecuteNonQuery();
        }
    }
}