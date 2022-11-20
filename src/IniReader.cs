using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TaskSendMail {
  public class IniFile {
    // some totally random value to indicate that the value was not found in ini file
    // totally unlikely to clash with other values
    private readonly string _notFoundValue =
      "E3098E22-7179-4BAB-B314-10D8D3DA6A13-F3715DD8-4AA7-4885-AC76-61E3D4534C04";

    private readonly string _path;

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern uint GetPrivateProfileString(
      string lpAppName,
      string lpKeyName,
      string lpDefault,
      StringBuilder lpReturnedString,
      uint nSize,
      string lpFileName);

    public IniFile(string path) {
      if (!File.Exists(path)) {
        throw new ArgumentException("File was not found: " + path);
      }

      _path = path;
    }

    public string ReadString(string section, string key) {
      var retVal = new StringBuilder(65536);
      GetPrivateProfileString(section, key, _notFoundValue, retVal, (uint)retVal.Capacity, _path);
      var nativeErrorCode = Marshal.GetLastWin32Error();
      if (nativeErrorCode != 0) {
        throw new Win32Exception("Error parsing ini file '" + _path + "': " +
                                 new Win32Exception(nativeErrorCode).Message);
      }

      var value = retVal.ToString();
      if (value == _notFoundValue) {
        throw new KeyNotFoundException($"Key '{key}' was not found in section '{section}' of ini file '{_path}'");
      }

      return value;
    }

    public bool ReadBool(string section, string key) {
      var value = ReadString(section, key);
      switch (value) {
        case "true":
          return true;
        case "false":
          return false;
        default:
          throw new ArgumentException(
            $"Value '{value}' of key '{key}' in section '{section}' of ini file '{_path}' should be 'true' or 'false' (without quotes)");
      }
    }

    public int ReadInt(string section, string key) {
      var value = ReadString(section, key);

      if (!int.TryParse(value, out var result)) {
        throw new ArgumentException(
          $"Could not parse number value of key '{key}' in section '{section}' of ini file '{_path}': ${value}");
      }

      return result;
    }
  }
}
