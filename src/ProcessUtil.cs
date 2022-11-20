using System;
using System.Text;

namespace TaskSendMail {
  public static class ProcessUtil {
    private static readonly char[] ToQuote = { ' ', '\t', '"' };

    public static void AddQuoted(StringBuilder builder, string arg) {
      if (builder == null) throw new ArgumentNullException(nameof(builder));
      if (arg == null) throw new ArgumentNullException(nameof(arg));

      if (builder.Length > 0 && builder[builder.Length - 1] != ' ') {
        builder.Append(' ');
      }

      bool quote = arg.Length == 0 || arg.IndexOfAny(ToQuote) >= 0;
      if (quote) {
        builder.Append('"');
      }

      int argLen = arg.Length;
      for (int i = 0; i < argLen; ++i) {
        if (arg[i] == '\\') {
          int k = 1;
          while (++i < argLen && arg[i] == '\\')
            ++k;
          if (quote && i >= argLen) {
            builder.Append('\\', 2 * k);
            --i;
          } else if (i < argLen && arg[i] == '"') {
            builder.Append('\\', 2 * k + 1);
            builder.Append('"');
          } else {
            builder.Append('\\', k);
            --i;
          }
        } else if (arg[i] == '"') {
          builder.Append("\\\"");
        } else {
          builder.Append(arg[i]);
        }
      }

      if (quote) {
        builder.Append('"');
      }
    }
  }
}
