using System.Net;
using System.Net.Mail;

namespace TaskSendMail {
  public class SmtpMailSender {
    private readonly SmtpClient _smtpClient;
    private readonly string _from;
    private readonly string _to;

    public SmtpMailSender(IniFile config) {
      var host = config.ReadString("smtp", "host");
      var port = config.ReadInt("smtp", "port");
      var user = config.ReadString("smtp", "user");
      var password = config.ReadString("smtp", "password");
      var tls = config.ReadBool("smtp", "StartTLS");

      _from = config.ReadString("smtp", "from");
      _to = config.ReadString("smtp", "to");

      _smtpClient = new SmtpClient(host) {
        Port = port,
        EnableSsl = tls,
      };

      if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(password)) {
        _smtpClient.Credentials = new NetworkCredential(user, password);
      }
    }

    public void SendMail(string subject, string body) {
      _smtpClient.Send(_from, _to, subject, body);
    }
  }
}
