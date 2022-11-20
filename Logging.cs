using System;

namespace TaskSendMail {
  public static class Logging {
    public static bool Verbose;

    public static void Debug(string message) {
      if (Verbose) {
        Console.Error.WriteLine("DEBUG: " + message);
      }
    }
  }
}
