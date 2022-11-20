task-sendmail
=============

Runs specified process and sends email notification with program's output if exit code was not zero (Windows only)

Usage: `task_sendmail.exe [--verbose] --config PATH_TO_CONFIG.ini PROGRAM_TO_RUN [arguments]`

Config file should be in the format of .ini file, example:

```
[smtp]
host = smtp.gmail.com
port = 465
user = me@gmail.com
password = some-pass
StartTLS = true
from = me@gmail.com
to = me@gmail.com
```

NOTE: task_sendmail supports only unencrypted SMTP connections or SMTP with StartTLS.
See https://sendgrid.com/blog/what-is-starttls for explanation
