using System;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace PerfToolkit
{
    public static class Shell
    {
        private static FunctionEvaluator m_FunctionEvaluator;
        private static UdpHost m_UdpHost;

        public static int MTU
        {
            get => m_UdpHost.MTU;
            set => m_UdpHost.MTU = value;
        }

        public static void Init(int port = 9999)
        {
            m_FunctionEvaluator = new FunctionEvaluator();
            m_UdpHost = new UdpHost(port);
            m_UdpHost.MessageReceived += OnMessageReceived;
            m_UdpHost.Start();
        }

        public static string TestSelf()
        {
            try
            {
                object result;
                float num;
                
                result = Execute("UnityEngine.Application.targetFrameRate;f");
                Assert.IsTrue(float.TryParse(result.ToString(), out _));

                result = Execute("Time.time;");
                Assert.IsTrue(float.TryParse(result.ToString(), out _));

                result = Execute("PerfToolkit.TestEvaluator.StaticAdd(1.0, 2.9)");
                num = float.Parse(result.ToString());
                Assert.IsTrue(num == 3.9f);

                result = Execute("TestEvaluator.GetInstance().Add(1.0, 2.9);");
                num = float.Parse(result.ToString());
                Assert.IsTrue(num == 3.9f);

                Execute("PerfToolkit.TestEvaluator.StaticPrivateValue = 1");
                Execute("PerfToolkit.TestEvaluator.StaticPublicValue = 1");
                Execute("PerfToolkit.TestEvaluator.StaticGetSetValue = 1; ");

                Execute("PerfToolkit.TestEvaluator.GetInstance().PublicValue = 1");
                Execute("PerfToolkit.TestEvaluator.GetInstance().m_PrivateValue = 1");
                Execute("PerfToolkit.TestEvaluator.GetInstance().GetSetValue = 1");

                result = Execute("PerfToolkit.TestEvaluator.StaticPrivateValue;");
                Assert.IsTrue(result.ToString() == "1");

                result = Execute("PerfToolkit.TestEvaluator.StaticPublicValue");
                Assert.IsTrue(result.ToString() == "1");
                
                result = Execute("PerfToolkit.TestEvaluator.StaticGetSetValue");
                Assert.IsTrue(result.ToString() == "1");

                result = Execute("PerfToolkit.TestEvaluator.GetInstance().PublicValue");
                Assert.IsTrue(result.ToString() == "1");
                
                result = Execute("PerfToolkit.TestEvaluator.GetInstance().m_PrivateValue");
                Assert.IsTrue(result.ToString() == "1");
                
                result = Execute("PerfToolkit.TestEvaluator.GetInstance().GetSetValue");
                Assert.IsTrue(result.ToString() == "1");
                
                Execute("TestEvaluator.GetInstance()");
                
                return "Test Complete!";
            }
            catch (Exception e)
            {
                return $"{e.Message}\n{e.StackTrace}";
            }
        }

        public static string TestLargeStr(int count = 1000)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                sb.AppendLine($"TestLargeStr: {i}");
            }

            return sb.ToString();
        }

        public static void DeInit()
        {
            m_UdpHost.Stop();
        }

        private static void OnMessageReceived(string code)
        {
            if(string.IsNullOrEmpty(code)) return;
            
            m_FunctionEvaluator.Execute(code, out var returnObj);
            var msg = returnObj == null ? "ok" : returnObj.ToString();
            Debug.Log($"Shell Execute: {code}\n{returnObj}");
            m_UdpHost.Send(msg);
        }

        public static object Execute(string code)
        {
            if (m_FunctionEvaluator.Execute(code, out var returnObj))
                return returnObj;
            throw new Exception($"execute failed \n{returnObj}");
        }
    }
}
