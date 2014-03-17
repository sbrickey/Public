using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;

namespace SmtpSimulator.Core
{
    /// <summary>
    /// Simulates a multi threaded, multi connection SMTP Server
    /// </summary>
    public class SmtpSimulator
    {
        public static readonly List<MailMessage> MailQueue = new List<MailMessage>();

        private readonly AutoResetEvent connectionWaitHandle = new AutoResetEvent(false);
        private readonly IPAddress listener_address;
        private readonly int listener_port;
        private bool listening = false;
        private readonly TcpListener listener;
        private readonly List<SmtpSessionSimulator> sessions = new List<SmtpSessionSimulator>();

        #region Events

        public delegate void StartedEventHandler();
        public event StartedEventHandler Started;
        protected virtual void OnStart() { if (Started != null) { Started(); } }

        public delegate void StoppedEventHandler();
        public event StoppedEventHandler Stopped;
        protected virtual void OnStop() { if (Stopped != null) { Stopped(); } }

        #endregion

        public SmtpSimulator(IPAddress address, int port)
        {
            listener_address = address;
            listener_port = port;

            listener = new TcpListener(listener_address, listener_port);
        }


        public void Start()
        {
            listener.Start();
            listening = true;

            var listenerThread = new System.Threading.Thread(new ThreadStart(Listen));
            listenerThread.Start();

            // raise event that simulator has started / is listening
            OnStart();
        } // void Start()

        public void Stop()
        {
            // identify internally that we are done
            listening = false;

            // stop listening for new connections
            listener.Stop();

            // kill current connections, reset list
            sessions.ForEach(c => c.Close());
            sessions.Clear();

            // signal external code that simulator has stopped
            OnStop();
        } // void Stop()



        private void Listen()
        {
            while (listening)
            {
                IAsyncResult result = listener.BeginAcceptTcpClient(Respond, listener);
                connectionWaitHandle.WaitOne();
                connectionWaitHandle.Reset();
            }
        } // void Listen()

        private void Respond(IAsyncResult result)
        {
            if (!listening)
            {
                connectionWaitHandle.Set(); // clear the thread block
                return;
            }

            var client = (result.AsyncState as TcpListener).EndAcceptTcpClient(result);
            connectionWaitHandle.Set(); // signal to main thread that connection is handled

            var session = new SmtpSessionSimulator(
                client: client,
                MessageQueueDelegate: new Action<MailMessage>((m) => { MailQueue.Add(m); })
                );

            // add to list of active connections
            sessions.Add(session);
            
            // session simulator main execution
            session.Execute();

            // remove from list of active sessions
            sessions.Remove(session);

            // close the connection if necessary
            if (client.Connected)
                client.Close();
        } // void Respond(...)

    } // class SmtpSimulator
} // namespace
