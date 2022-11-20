using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace TaskSendMail {
  public class FileReports {
    private readonly string _dir;

    public FileReports(IniFile config) {
      _dir = config.ReadString("file", "dir");
      Directory.CreateDirectory(_dir);
    }

    public void SaveReport(bool success, string body) {
      var fileName = $"{(success ? "success-" : "failure-")}{DateTime.Now:yyyy-MM-dd-hhmmss}-{Process.GetCurrentProcess().Id}.txt";
      var file = Path.Combine(_dir, fileName);
      Logging.Debug("Writing report to: " + file);
      File.WriteAllText(file, body, Encoding.UTF8);
    }
  }
}
