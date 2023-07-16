using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace PerfToolkit
{
    public class UdpHost
    {
        public event Action<string> MessageReceived;
        private readonly UdpClient m_UdpClient;
        
        private IPEndPoint m_ReceiveRemoteEndPoint;
        private IPEndPoint m_SendRemoteEndPoint;
        private string m_RemoteIP; 
        private int m_RemotePort;
        private SynchronizationContext m_MainContext;
        private Thread m_Thread;

        public UdpHost(int localPort)
        {
            m_UdpClient = new UdpClient(localPort);
        }
        
        public void Send(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            lock (this)
            {
                m_UdpClient.Send(buffer, buffer.Length, m_SendRemoteEndPoint);    
            }
        }

        public void Start()
        {
            m_MainContext = SynchronizationContext.Current;
            m_Thread = new Thread(Run) { Name = "UdpShell" };
            m_Thread.Start();
        }

        public void Stop()
        {
            m_UdpClient?.Close();
            m_Thread?.Abort();
        }
        
        private void Run()
        {
            m_ReceiveRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    var buffer = m_UdpClient.Receive(ref m_ReceiveRemoteEndPoint);
                    lock (this)
                    {
                        m_SendRemoteEndPoint = m_ReceiveRemoteEndPoint;
                    }
                    
                    OnReceiveMessage(buffer);
                }
                catch (Exception e)
                {
                    if(e is ThreadAbortException)
                        continue;
                    Debug.LogError(e.Message);
                }

                Thread.Sleep(200);
            }
        }
        
        private void OnReceiveMessage(byte[] data)
        {
            if (data == null)
            {
                return;
            }
            
            var text = Encoding.UTF8.GetString(data, 0, data.Length);
            if (text == "hi")
            {
                Send("welcome");
            }
            else
            {
                m_MainContext.Post((_) =>
                {
                    MessageReceived?.Invoke(text);
                }, null);    
            }
        }
    }
}