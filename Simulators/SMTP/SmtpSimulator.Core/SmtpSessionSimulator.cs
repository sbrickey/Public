using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SmtpSimulator.Core
{
    /// <summary>
    /// Simulates a single SMTP Session (client connection)
    /// </summary>
    public class SmtpSessionSimulator
    {
        private readonly Action<MailMessage> _messageQueueDelegate;
        private readonly TcpClient _client;
        private readonly NetworkStream stream;
        private readonly System.IO.StreamReader reader;
        private readonly System.IO.StreamWriter writer;
        private bool _ExternallyClosed = false;

        public SmtpSessionSimulator(TcpClient client, Action<MailMessage> MessageQueueDelegate)
        {
            _messageQueueDelegate = MessageQueueDelegate;

            _client = client;
            _client.ReceiveTimeout = int.MaxValue; //5000;

            stream = _client.GetStream();
            reader = new System.IO.StreamReader(stream);
            writer = new System.IO.StreamWriter(stream)
            {
                NewLine = "\r\n",
                AutoFlush = true
            };
        } // CTor(...)

        public void Execute()
        {
            WriteOut("220 localhost -- Mock SMTP server");

            try
            {
                string line = null;
                var currentMessage = new MailMessage();

                var RequestedQuit = false;
                while (reader != null && !RequestedQuit)
                {
                    line = reader.ReadLine();
                    //Console.Error.WriteLine("< {0}", line);
                    var line_lower = line.ToLower();


                    if (line_lower == "")
                    {
                        // nothing
                        writer.Write("");
                    }
                    else if (line_lower.StartsWith("mail from:"))
                    {
                        // parse input
                        var data = line.Remove(0, "mail from:".Length);

                        // persist
                        currentMessage.From = data.TrimStart(new char[] { '<' }).TrimEnd(new char[] { '>' });

                        // response
                        WriteOut("250 2.1.0 {0}....Sender OK", data);
                    }

                    else if (line_lower.StartsWith("rcpt to:"))
                    {
                        // parse input
                        var data = line.Remove(0, "rcpt to:".Length)
                                       .TrimStart(new char[] { '<' })
                                       .TrimEnd(new char[] { '>' });

                        // persist
                        currentMessage.Recipients.Add(data);

                        // response
                        WriteOut("250 2.1.5 {0}", data);
                    }

                    else if (line_lower.StartsWith("data"))
                    {
                        WriteOut("354 Start input, end data with <CRLF>.<CRLF>");

                        StringBuilder data = new StringBuilder();

                        var dataline = reader.ReadLine();
                        while (dataline != null && dataline != ".")
                        {
                            // log and append line
                            //Console.Error.WriteLine("< {0}", dataline);
                            data.AppendLine(dataline);

                            // read next
                            dataline = reader.ReadLine();
                        }

                        currentMessage.Message = data.ToString();

                        // end of message is initiation of delivery
                        _messageQueueDelegate(currentMessage);

                        WriteOut("250 Mail queued for delivery.");

                        // reset message
                        currentMessage = new MailMessage();
                    }

                    else if (line_lower.StartsWith("rset"))
                    {
                        currentMessage = new MailMessage();

                        WriteOut("250 2.0.0 Resetting");
                    }

                    else if (line_lower.StartsWith("quit"))
                    {
                        RequestedQuit = true;
                        WriteOut("250 OK");
                    }

                    else
                    {
                        writer.WriteLine("250 OK");
                        //Console.Error.WriteLine(">>> 250 OK (default)");
                    } // else (unhandled command)

                } // while reader != null && !RequestedQuit
            } // try
            catch (IOException)
            {
                if (_ExternallyClosed)
                    ;//Console.WriteLine("Connection closed");
                else
                    ;//Console.WriteLine("Connection lost.");
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
            }
        } // void Execute()

        internal void Close()
        {
            _ExternallyClosed = true;
            _client.Close();
        }

        private void WriteOut(string msg, params string[] objs)
        {
            var outmsg = String.Format(msg, objs);

            writer.WriteLine(outmsg);
            //Console.Error.WriteLine(">>> " + outmsg);
        }

    } // class SmtpSessionSimulator
} // namespace
