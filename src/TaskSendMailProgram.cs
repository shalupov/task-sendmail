using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace TaskSendMail {
  internal static class TaskSendMailProgram {
    public static int Main(string[] args) {
      try {
        return MainImpl(args);
      } catch (Exception e) {
        Console.Error.WriteLine(e);
        return 254;
      }
    }

    private static int MainImpl(string[] args) {
      if (args.Length == 0 || args[0] == "-h" || args[0] == "--help" || args[0] == "/?" || args[0] == "/h") {
        ShowHelpAndExit();
      }

      IniFile config = null;
      int i = 0;
      while (i < args.Length) {
        var arg = args[i];
        if (arg == "--config") {
          i++;
          config = new IniFile(args[i++]);
          continue;
        }

        if (arg == "--verbose") {
          i++;
          Logging.Verbose = true;
          continue;
        }

        if (!arg.StartsWith("--")) {
          break;
        }

        ShowHelpAndExit("Unknown command line key: " + arg);
      }

      if (config == null) {
        ShowHelpAndExit("Config file name is missing");
        Environment.Exit(1); // NOT-REACHED
      }

      if (i == args.Length) {
        ShowHelpAndExit("Process name is missing");
      }

      string processToRun = args[i++];
      StringBuilder processArguments = new StringBuilder();
      while (i < args.Length) {
        ProcessUtil.AddQuoted(processArguments, args[i++]);
      }

      var smtp = config.ReadBool("smtp", "enabled") ? new SmtpMailSender(config) : null;
      var file = config.ReadBool("file", "enabled") ? new FileReports(config) : null;

      var stderr = new StringBuilder();
      var stdout = new StringBuilder();
      var processStopwatch = new Stopwatch();
      processStopwatch.Start();

      Logging.Debug($"Starting process '{processToRun}' with arguments '{processArguments}'");

      var process = new Process {
        StartInfo = {
          FileName = processToRun,
          Arguments = processArguments.ToString(),
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
        },
      };
      process.ErrorDataReceived += (o, eventArgs) => { stderr.AppendLine(eventArgs.Data); };
      process.OutputDataReceived += (o, eventArgs) => { stdout.AppendLine(eventArgs.Data); };
      process.Start();
      process.BeginErrorReadLine();
      process.BeginOutputReadLine();
      process.WaitForExit();
      processStopwatch.Stop();
      
      Logging.Debug("Process exited with exit code " + process.ExitCode);

      EnsureNewLineAtTheEnd(stderr);
      EnsureNewLineAtTheEnd(stdout);

      string reportBody = $"STDOUT:{Environment.NewLine}{stdout}{Environment.NewLine}" +
                          $"STDERR:{Environment.NewLine}{stderr}{Environment.NewLine}" +
                          $"PROCESS RUN TIME: {processStopwatch.Elapsed.ToString(null, CultureInfo.InvariantCulture)}{Environment.NewLine}{Environment.NewLine}" +
                          $"PROCESS EXIT TIME: {process.ExitTime.ToString(DateTimeFormatInfo.InvariantInfo)}{Environment.NewLine}{Environment.NewLine}" +
                          $"PROCESS EXIT CODE: {process.ExitCode}{Environment.NewLine}{Environment.NewLine}";

      if (file != null) {
        var sb = new StringBuilder();
        sb.Append("Process: ");
        ProcessUtil.AddQuoted(sb, processToRun);
        sb.Append(" ");
        sb.Append(processArguments);
        sb.AppendLine();
        sb.AppendLine();
        sb.Append(reportBody);
        
        file.SaveReport(process.ExitCode == 0, sb.ToString());
      }

      if (process.ExitCode != 0) {
        SendMail(
          smtp,
          process.ExitCode,
          process.StartInfo.FileName,
          reportBody
        );
      }

      return process.ExitCode;
    }

    private static void EnsureNewLineAtTheEnd(StringBuilder builder) {
      if (builder.Length == 0 || builder[builder.Length - 1] != '\n') {
        builder.AppendLine();
      }
    }

    private static void ShowHelpAndExit(string errorMessage = null) {
      if (!string.IsNullOrEmpty(errorMessage)) {
        Console.Error.WriteLine(errorMessage);
        Console.Error.WriteLine();
      }

      Console.Error.WriteLine("Usage: task_sendmail.exe [--verbose] --config PATH_TO_CONFIG PROGRAM_TO_RUN [arguments]");
      Console.Error.WriteLine();
      Console.Error.WriteLine("Config file should be in the format of .ini file, example:");
      Console.Error.WriteLine("  [smtp]");
      Console.Error.WriteLine("  enabled = true");
      Console.Error.WriteLine("  host = smtp.gmail.com");
      Console.Error.WriteLine("  port = 465");
      Console.Error.WriteLine("  user = me@gmail.com");
      Console.Error.WriteLine("  password = some-pass");
      Console.Error.WriteLine("  StartTLS = true");
      Console.Error.WriteLine("  from = me@gmail.com");
      Console.Error.WriteLine("  to = me@gmail.com");
      Console.Error.WriteLine("  [file]");
      Console.Error.WriteLine("  enabled = true");
      Console.Error.WriteLine("  dir = C:\\my\\dir");
      Console.Error.WriteLine();
      Console.Error.WriteLine("NOTE: task_sendmail supports only unencrypted SMTP connections or SMTP with StartTLS");
      Console.Error.WriteLine("See https://sendgrid.com/blog/what-is-starttls for explanation");
      Console.Error.WriteLine();
      Environment.Exit(255);
    }

    private static void SendMail(SmtpMailSender sender, int exitCode, string program, string body) {
      if (sender == null) {
        Logging.Debug("Sending e-mail is disabled");
        return;
      }

      sender.SendMail($"[task-sendmail] Program '{program}' execution failure: exit code {exitCode}", body);
    }
  }
}
