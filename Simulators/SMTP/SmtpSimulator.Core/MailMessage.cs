using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmtpSimulator.Core
{
    public class MailMessage
    {
        public string From { get; set; }

        private readonly List<string> _recipients = new List<string>();
        public List<string> Recipients { get { return _recipients; } }

        public string Message { get; set; }


    } // class MailMessage
} // namespace
