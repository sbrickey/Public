using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace SmtpSimulator.Core
{
    public class MailMessage
    {
        public string From { get; set; }

        private readonly List<string> _recipients = new List<string>();
        public List<string> Recipients { get { return _recipients; } }

        //public string Subject { get; set; }

        public string Message { get; set; }



        //const string SUBJECT = "Subject: ";
        //const string FROM = "From: ";
        //const string TO = "To: ";
        //const string MIME_VERSION = "MIME-Version: ";
        //const string DATE = "Date: ";
        //const string CONTENT_TYPE = "Content-Type: ";
        //const string CONTENT_TRANSFER_ENCODING = "Content-Transfer-Encoding: ";


        //private string DecodeQuotedPrintable(string input)
        //{
        //    var occurences = new Regex(@"(=[0-9A-Z][0-9A-Z])+", RegexOptions.Multiline);
        //    var matches = occurences.Matches(input);
        //    foreach (Match m in matches)
        //    {
        //        byte[] bytes = new byte[m.Value.Length / 3];
        //        for (int i = 0; i < bytes.Length; i++)
        //        {
        //            string hex = m.Value.Substring(i * 3 + 1, 2);
        //            int iHex = Convert.ToInt32(hex, 16);
        //            bytes[i] = Convert.ToByte(iHex);
        //        }
        //        input = input.Replace(m.Value, Encoding.Default.GetString(bytes));
        //    }
        //    return input.Replace("=\r\n", "");
        //}

    } // class MailMessage
} // namespace
