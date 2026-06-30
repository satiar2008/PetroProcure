using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;

namespace PetroProcure.Installer;

/// <summary>
/// Console installer for deploying PetroProcure (API + Web + optional Worker) onto IIS / Windows Server.
/// It verifies prerequisites, collects deployment settings, writes appsettings.Production.json files,
/// provisions the database (migrations + seed), configures IIS sites, and registers the Worker service.
/// </summary>
internal static class Program
{
    private static readonly string Windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
    private static string AppCmd => Path.Combine(Windir, "system32", "inetsrv", "appcmd.exe");

    // Possible locations of the ASP.NET Core Module V2 binary. Modern Hosting Bundles install it under
    // "Program Files\IIS\Asp.Net Core Module\V2"; older ones placed the shim in system32\inetsrv.
    private static IEnumerable<string> AncmModulePaths()
    {
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        yield return Path.Combine(programFiles, "IIS", "Asp.Net Core Module", "V2", "aspnetcorev2.dll");
        yield return Path.Combine(Windir, "system32", "inetsrv", "aspnetcorev2.dll");
    }

    private static string ApplicationHostConfig => Path.Combine(Windir, "system32", "inetsrv", "config", "applicationHost.config");

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        Banner();

        try
        {
            if (!IsAdministrator())
            {
                Error("This installer must be run as Administrator. Right-click and choose 'Run as administrator'.");
                return Pause(2);
            }

            // Payload layout: <release>\installer\PetroProcure.Installer.exe  +  <release>\{api,web,worker}
            var releaseRoot = Directory.GetParent(AppContext.BaseDirectory)!.FullName;
            var apiPayload = Path.Combine(releaseRoot, "api");
            var webPayload = Path.Combine(releaseRoot, "web");
            var workerPayload = Path.Combine(releaseRoot, "worker");

            if (!Directory.Exists(apiPayload) || !Directory.Exists(webPayload))
            {
                Error($"Payload folders not found next to the installer.\n  Expected: {apiPayload}\n            {webPayload}");
                return Pause(3);
            }

            if (!CheckPrerequisites())
            {
                // Per requirement: warn and STOP so the operator installs the prerequisite and re-runs.
                return Pause(4);
            }

            var settings = LoadOrPromptSettings(workerPayload);
            Confirm(settings);

            Step("1/7  Copying application files");
            CopyDirectory(apiPayload, Path.Combine(settings.InstallPath, "api"));
            CopyDirectory(webPayload, Path.Combine(settings.InstallPath, "web"));
            if (settings.InstallWorker && Directory.Exists(workerPayload))
                CopyDirectory(workerPayload, Path.Combine(settings.InstallPath, "worker"));

            Step("2/7  Writing configuration (appsettings.Production.json)");
            WriteApiSettings(settings);
            WriteWebSettings(settings);
            if (settings.InstallWorker)
                WriteWorkerSettings(settings);

            Step("3/7  Preparing file storage folders");
            Directory.CreateDirectory(settings.FileStorageRoot);
            Directory.CreateDirectory(Path.Combine(settings.FileStorageRoot, "Quarantine"));

            Step("4/7  Creating database (migrations + seed)");
            RunMigrations(settings);

            Step("5/7  Configuring IIS sites and application pools");
            ConfigureIis(settings);
            GrantPoolPermissions(settings);

            if (settings.InstallWorker)
            {
                Step("6/7  Installing the Worker Windows Service");
                InstallWorkerService(settings);
            }
            else
            {
                Step("6/7  Worker service skipped (not selected)");
            }

            Step("7/7  Starting sites");
            StartSites(settings);

            Success(settings);
            return Pause(0);
        }
        catch (Exception ex)
        {
            Error("Installation failed:\n" + ex.Message);
            Console.WriteLine();
            Console.WriteLine(ex);
            return Pause(1);
        }
    }

    // ---------------------------------------------------------------- prerequisites

    private static bool CheckPrerequisites()
    {
        Step("0/7  Checking prerequisites");
        var ok = true;

        if (File.Exists(AppCmd))
        {
            Info("IIS detected (appcmd.exe found).");
        }
        else
        {
            Warn("IIS was not detected. Enable the 'Web Server (IIS)' role before installing.\n"
                 + "  Server Manager > Add Roles and Features > Web Server (IIS).");
            ok = false;
        }

        if (AspNetCoreModuleAvailable())
        {
            Info("ASP.NET Core Module V2 (Hosting Bundle) detected.");
        }
        else if (HostingBundleProductInstalled())
        {
            // The bundle is in Add/Remove Programs but its IIS module is not registered — this happens
            // when the Hosting Bundle was installed BEFORE the IIS role was enabled.
            Warn("The .NET Hosting Bundle is installed, but its IIS module is NOT registered with IIS.\n"
                 + "  This happens when the Hosting Bundle was installed before the IIS role was enabled.\n"
                 + "  Fix: re-run the Hosting Bundle installer and choose 'Repair' (or:  dotnet-hosting-9.x.x-win.exe /repair),\n"
                 + "       then run:  net stop was /y && net start w3svc\n"
                 + "  Afterwards re-run this installer.");
            ok = false;
        }
        else
        {
            Warn("ASP.NET Core Hosting Bundle is NOT installed.\n"
                 + "  Download and install the .NET 9 Hosting Bundle, then run 'iisreset' and re-run this installer:\n"
                 + "  https://dotnet.microsoft.com/download/dotnet/9.0  (ASP.NET Core Runtime > Hosting Bundle)");
            ok = false;
        }

        if (!ok)
        {
            Console.WriteLine();
            Error("Prerequisites are missing. Installation stopped. Please install the items above and run the installer again.");
        }

        return ok;
    }

    // ---------------------------------------------------------------- settings

    private static Settings LoadOrPromptSettings(string workerPayload)
    {
        var answerFile = Path.Combine(AppContext.BaseDirectory, "install.config.json");
        if (File.Exists(answerFile))
        {
            Info($"Using answer file: {answerFile}");
            var loaded = JsonSerializer.Deserialize<Settings>(File.ReadAllText(answerFile), JsonOpts)
                         ?? throw new InvalidOperationException("install.config.json could not be parsed.");
            loaded.Normalize();
            if (string.IsNullOrWhiteSpace(loaded.JwtSigningKey)) loaded.JwtSigningKey = GenerateKey();
            return loaded;
        }

        Console.WriteLine();
        Console.WriteLine("Enter deployment settings (press Enter to accept the [default]):");
        Console.WriteLine();

        var s = new Settings();
        s.InstallPath = Prompt("Install folder", s.InstallPath);
        s.FileStorageRoot = Prompt("Document storage root", s.FileStorageRoot);

        Console.WriteLine();
        s.SqlServer = Prompt("SQL Server instance", s.SqlServer);
        s.SqlDatabase = Prompt("Database name", s.SqlDatabase);
        s.SqlIntegratedSecurity = PromptYesNo("Use Windows Integrated Security for SQL?", s.SqlIntegratedSecurity);
        if (!s.SqlIntegratedSecurity)
        {
            s.SqlUser = Prompt("SQL user", string.IsNullOrWhiteSpace(s.SqlUser) ? "sa" : s.SqlUser);
            s.SqlPassword = PromptSecret("SQL password");
        }

        Console.WriteLine();
        s.ApiSiteName = Prompt("IIS site name (API)", s.ApiSiteName);
        s.ApiPort = PromptInt("IIS port (API)", s.ApiPort);
        s.WebSiteName = Prompt("IIS site name (Web)", s.WebSiteName);
        s.WebPort = PromptInt("IIS port (Web)", s.WebPort);

        Console.WriteLine();
        s.AdminPassword = PromptSecret("Bootstrap admin password (user 'admin')");
        while (s.AdminPassword.Length < 8)
        {
            Warn("Password must be at least 8 characters.");
            s.AdminPassword = PromptSecret("Bootstrap admin password (user 'admin')");
        }

        var generated = Prompt("JWT signing key (Enter = auto-generate)", "");
        s.JwtSigningKey = string.IsNullOrWhiteSpace(generated) ? GenerateKey() : generated;

        s.InstallWorker = Directory.Exists(workerPayload)
                          && PromptYesNo("Install the AI Worker as a Windows Service?", s.InstallWorker);

        s.Normalize();
        return s;
    }

    private static void Confirm(Settings s)
    {
        Console.WriteLine();
        Console.WriteLine("──────────────────────────────────────────────");
        Console.WriteLine("  Review:");
        Console.WriteLine($"   Install path     : {s.InstallPath}");
        Console.WriteLine($"   Storage root     : {s.FileStorageRoot}");
        Console.WriteLine($"   SQL              : {s.SqlServer} / {s.SqlDatabase} ({(s.SqlIntegratedSecurity ? "Windows auth" : "user " + s.SqlUser)})");
        Console.WriteLine($"   API site         : {s.ApiSiteName}  http://localhost:{s.ApiPort}");
        Console.WriteLine($"   Web site         : {s.WebSiteName}  http://localhost:{s.WebPort}");
        Console.WriteLine($"   Install worker   : {(s.InstallWorker ? "yes (Windows Service)" : "no")}");
        Console.WriteLine("──────────────────────────────────────────────");
        if (!PromptYesNo("Proceed with installation?", true))
            throw new OperationCanceledException("Cancelled by operator.");
    }

    // ---------------------------------------------------------------- appsettings writers

    private static void WriteApiSettings(Settings s)
    {
        var node = new Dictionary<string, object?>
        {
            ["ConnectionStrings"] = new Dictionary<string, object?> { ["PetroProcureDb"] = s.ConnectionString() },
            ["Database"] = new Dictionary<string, object?> { ["MigrateOnStartup"] = true },
            ["Authentication"] = new Dictionary<string, object?>
            {
                ["Jwt"] = new Dictionary<string, object?> { ["SigningKey"] = s.JwtSigningKey }
            },
            ["Security"] = new Dictionary<string, object?>
            {
                ["BootstrapAdmin"] = new Dictionary<string, object?>
                {
                    ["Enabled"] = true,
                    ["Password"] = s.AdminPassword
                }
            },
            ["PetroProcure"] = new Dictionary<string, object?>
            {
                ["FileStorage"] = new Dictionary<string, object?>
                {
                    ["RootPath"] = s.FileStorageRoot,
                    ["QuarantinePath"] = Path.Combine(s.FileStorageRoot, "Quarantine")
                },
                ["AI"] = new Dictionary<string, object?>
                {
                    ["AiCore"] = new Dictionary<string, object?> { ["Mode"] = "SyncAiCoreDirect" }
                }
            }
        };
        WriteJson(Path.Combine(s.InstallPath, "api", "appsettings.Production.json"), node);
    }

    private static void WriteWebSettings(Settings s)
    {
        var node = new Dictionary<string, object?>
        {
            ["ApiBaseUrl"] = $"http://localhost:{s.ApiPort}"
        };
        WriteJson(Path.Combine(s.InstallPath, "web", "appsettings.Production.json"), node);
    }

    private static void WriteWorkerSettings(Settings s)
    {
        var node = new Dictionary<string, object?>
        {
            ["ConnectionStrings"] = new Dictionary<string, object?> { ["PetroProcureDb"] = s.ConnectionString() },
            ["PetroProcure"] = new Dictionary<string, object?>
            {
                ["AI"] = new Dictionary<string, object?>
                {
                    ["AiCore"] = new Dictionary<string, object?> { ["Mode"] = "SyncAiCoreDirect" }
                }
            }
        };
        WriteJson(Path.Combine(s.InstallPath, "worker", "appsettings.Production.json"), node);
    }

    // ---------------------------------------------------------------- database

    private static void RunMigrations(Settings s)
    {
        var apiExe = Path.Combine(s.InstallPath, "api", "PetroProcure.Api.exe");
        if (!File.Exists(apiExe))
            throw new FileNotFoundException($"API host executable not found: {apiExe}");

        var (code, output) = Run(apiExe, "--migrate", workingDir: Path.Combine(s.InstallPath, "api"));
        if (!string.IsNullOrWhiteSpace(output))
            Console.WriteLine(Indent(output));
        if (code != 0)
            throw new InvalidOperationException($"Database migration failed (exit code {code}). Check the SQL connection settings.");
        Info("Database is ready.");
    }

    // ---------------------------------------------------------------- IIS

    private static void ConfigureIis(Settings s)
    {
        CreateSite(s.ApiSiteName, Path.Combine(s.InstallPath, "api"), s.ApiPort);
        CreateSite(s.WebSiteName, Path.Combine(s.InstallPath, "web"), s.WebPort);
    }

    private static void CreateSite(string siteName, string physicalPath, int port)
    {
        var poolName = siteName;

        // Recreate idempotently so re-running the installer is safe.
        RunAppCmd($"delete site \"{siteName}\"", ignoreErrors: true);
        RunAppCmd($"delete apppool \"{poolName}\"", ignoreErrors: true);

        // "No Managed Code" pool, always-running for in-process ASP.NET Core hosting.
        RunAppCmd($"add apppool /name:\"{poolName}\" /managedRuntimeVersion:\"\" /startMode:AlwaysRunning");
        RunAppCmd($"add site /name:\"{siteName}\" /physicalPath:\"{physicalPath}\" /bindings:http/*:{port}:");
        RunAppCmd($"set app \"{siteName}/\" /applicationPool:\"{poolName}\"");
        Info($"Site '{siteName}' -> {physicalPath} on http://*:{port}");
    }

    private static void GrantPoolPermissions(Settings s)
    {
        foreach (var pool in new[] { s.ApiSiteName, s.WebSiteName })
        {
            GrantFolder(s.InstallPath, pool);
            GrantFolder(s.FileStorageRoot, pool);
        }
    }

    private static void GrantFolder(string path, string poolName)
    {
        // The in-process app pool identity reads its content folder and writes the document store.
        Run("icacls", $"\"{path}\" /grant \"IIS AppPool\\{poolName}\":(OI)(CI)M /T /C /Q", ignoreExit: true);
    }

    private static void StartSites(Settings s)
    {
        RunAppCmd($"start apppool \"{s.ApiSiteName}\"", ignoreErrors: true);
        RunAppCmd($"start apppool \"{s.WebSiteName}\"", ignoreErrors: true);
        RunAppCmd($"start site \"{s.ApiSiteName}\"", ignoreErrors: true);
        RunAppCmd($"start site \"{s.WebSiteName}\"", ignoreErrors: true);
    }

    // ---------------------------------------------------------------- worker service

    private static void InstallWorkerService(Settings s)
    {
        const string serviceName = "PetroProcureWorker";
        var workerExe = Path.Combine(s.InstallPath, "worker", "PetroProcure.Worker.exe");
        if (!File.Exists(workerExe))
        {
            Warn($"Worker executable not found ({workerExe}); skipping service install.");
            return;
        }

        // Remove a previous instance so re-running is safe.
        Run("sc.exe", $"stop {serviceName}", ignoreExit: true);
        Run("sc.exe", $"delete {serviceName}", ignoreExit: true);

        // sc requires a space after each '=' and the binPath quoted.
        var bin = $"\"\\\"{workerExe}\\\"\"";
        var (code, output) = Run("sc.exe", $"create {serviceName} binPath= {bin} start= auto DisplayName= \"PetroProcure AI Worker\"");
        if (code != 0)
            throw new InvalidOperationException($"Failed to create the Worker service (exit {code}).\n{output}");

        Run("sc.exe", $"description {serviceName} \"PetroProcure background worker for AI evaluation jobs.\"", ignoreExit: true);
        Run("sc.exe", $"start {serviceName}", ignoreExit: true);
        Info($"Worker service '{serviceName}' installed and started.");
    }

    // ---------------------------------------------------------------- process helpers

    private static void RunAppCmd(string arguments, bool ignoreErrors = false)
    {
        var (code, output) = Run(AppCmd, arguments);
        if (code != 0 && !ignoreErrors)
            throw new InvalidOperationException($"appcmd failed ({code}): {arguments}\n{output}");
    }

    private static (int code, string output) Run(string fileName, string arguments, string? workingDir = null, bool ignoreExit = false)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDir ?? Environment.CurrentDirectory
        };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException($"Could not start {fileName}");
        var stdout = p.StandardOutput.ReadToEnd();
        var stderr = p.StandardError.ReadToEnd();
        p.WaitForExit();
        var combined = (stdout + (string.IsNullOrWhiteSpace(stderr) ? "" : "\n" + stderr)).Trim();
        if (p.ExitCode != 0 && ignoreExit)
            return (0, combined);
        return (p.ExitCode, combined);
    }

    // ---------------------------------------------------------------- file helpers

    private static void CopyDirectory(string source, string dest)
    {
        Directory.CreateDirectory(dest);
        foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            Directory.CreateDirectory(dir.Replace(source, dest));
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            File.Copy(file, file.Replace(source, dest), overwrite: true);
    }

    private static void WriteJson(string path, object node)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(node, JsonOpts), new UTF8Encoding(false));
        Info($"Wrote {path}");
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    // ---------------------------------------------------------------- console helpers

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// True when IIS can load the ASP.NET Core Module. Prefers the definitive signal — a registration in
    /// applicationHost.config (readable because the installer runs elevated) — and falls back to probing the
    /// module binary on disk (covers both the modern "Program Files\IIS" and legacy "system32\inetsrv" paths).
    /// </summary>
    private static bool AspNetCoreModuleAvailable()
    {
        try
        {
            if (File.Exists(ApplicationHostConfig)
                && File.ReadAllText(ApplicationHostConfig).Contains("AspNetCoreModuleV2", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch
        {
            // Not readable (e.g. not elevated) — fall through to the on-disk probe.
        }

        return AncmModulePaths().Any(File.Exists);
    }

    /// <summary>True when a .NET Hosting Bundle / Windows Server Hosting product is registered in Add/Remove Programs.</summary>
    private static bool HostingBundleProductInstalled()
    {
        foreach (var root in new[]
                 {
                     @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                     @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
                 })
        {
            using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(root);
            if (key is null) continue;
            foreach (var subName in key.GetSubKeyNames())
            {
                using var sub = key.OpenSubKey(subName);
                var name = sub?.GetValue("DisplayName") as string;
                if (!string.IsNullOrEmpty(name)
                    && (name.Contains("Windows Server Hosting", StringComparison.OrdinalIgnoreCase)
                        || name.Contains("Hosting Bundle", StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static string GenerateKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes); // > 32 chars, suitable for HS256
    }

    private static string Prompt(string label, string? def)
    {
        Console.Write($"  {label}" + (string.IsNullOrEmpty(def) ? "" : $" [{def}]") + ": ");
        var input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? (def ?? "") : input.Trim();
    }

    private static int PromptInt(string label, int def)
    {
        while (true)
        {
            var raw = Prompt(label, def.ToString());
            if (int.TryParse(raw, out var v) && v is > 0 and < 65536) return v;
            Warn("Enter a valid port number (1-65535).");
        }
    }

    private static bool PromptYesNo(string label, bool def)
    {
        var raw = Prompt(label + " (y/n)", def ? "y" : "n").ToLowerInvariant();
        return raw is "y" or "yes" or "true" or "1";
    }

    private static string PromptSecret(string label)
    {
        Console.Write($"  {label}: ");
        var sb = new StringBuilder();
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(intercept: true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0) { sb.Length--; Console.Write("\b \b"); }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                sb.Append(key.KeyChar);
                Console.Write('*');
            }
        }
        Console.WriteLine();
        return sb.ToString();
    }

    private static string Indent(string text) =>
        string.Join('\n', text.Split('\n').Select(l => "    " + l));

    private static void Banner()
    {
        Console.WriteLine("══════════════════════════════════════════════");
        Console.WriteLine("   PetroProcure — IIS Installer");
        Console.WriteLine("══════════════════════════════════════════════");
    }

    private static void Step(string text)  { Console.WriteLine(); Console.ForegroundColor = ConsoleColor.Cyan;   Console.WriteLine("» " + text); Console.ResetColor(); }
    private static void Info(string text)  { Console.ForegroundColor = ConsoleColor.Gray;   Console.WriteLine("  - " + text); Console.ResetColor(); }
    private static void Warn(string text)  { Console.ForegroundColor = ConsoleColor.Yellow; Console.WriteLine("  ! " + text); Console.ResetColor(); }
    private static void Error(string text) { Console.ForegroundColor = ConsoleColor.Red;    Console.WriteLine("  X " + text); Console.ResetColor(); }

    private static void Success(Settings s)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("══════════════════════════════════════════════");
        Console.WriteLine("   Installation completed successfully.");
        Console.WriteLine("══════════════════════════════════════════════");
        Console.ResetColor();
        Console.WriteLine($"   Web  : http://localhost:{s.WebPort}");
        Console.WriteLine($"   API  : http://localhost:{s.ApiPort}");
        Console.WriteLine( "   Login: admin  /  (the bootstrap password you entered)");
        Console.WriteLine();
        Console.WriteLine("   If the server has a firewall, open the chosen ports for remote access.");
    }

    private static int Pause(int code)
    {
        Console.WriteLine();
        Console.Write("Press Enter to exit...");
        Console.ReadLine();
        return code;
    }
}

/// <summary>Deployment settings, collected interactively or from install.config.json.</summary>
internal sealed class Settings
{
    public string InstallPath { get; set; } = @"C:\inetpub\PetroProcure";
    public string FileStorageRoot { get; set; } = @"C:\PetroProcure\ProcurementRoot";

    public string SqlServer { get; set; } = "localhost";
    public string SqlDatabase { get; set; } = "PetroProcureDb";
    public bool SqlIntegratedSecurity { get; set; } = false;
    public string SqlUser { get; set; } = "sa";
    public string SqlPassword { get; set; } = "";

    public string ApiSiteName { get; set; } = "PetroProcureApi";
    public int ApiPort { get; set; } = 5080;
    public string WebSiteName { get; set; } = "PetroProcureWeb";
    public int WebPort { get; set; } = 5081;

    public string AdminPassword { get; set; } = "";
    public string JwtSigningKey { get; set; } = "";
    public bool InstallWorker { get; set; } = true;

    public void Normalize()
    {
        InstallPath = InstallPath.TrimEnd('\\', '/');
        FileStorageRoot = FileStorageRoot.TrimEnd('\\', '/');
    }

    public string ConnectionString()
    {
        var sb = new StringBuilder();
        sb.Append($"Server={SqlServer};Database={SqlDatabase};");
        if (SqlIntegratedSecurity)
            sb.Append("Trusted_Connection=True;");
        else
            sb.Append($"User Id={SqlUser};Password={SqlPassword};");
        sb.Append("Encrypt=True;TrustServerCertificate=True");
        return sb.ToString();
    }
}
