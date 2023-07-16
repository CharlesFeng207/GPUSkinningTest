using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace PerfToolkit
{
    public static class Shell
    {
        [System.Serializable]
        public class Message
        {
            public int ErrorCode;
            public string Info;
        }

        private static FunctionEvaluator m_FunctionEvaluator;
        private static UdpHost m_UdpHost;

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
                
                return "ok";
            }
            catch (Exception e)
            {
                return $"{e.Message}\n{e.StackTrace}";
            }
        }

        public static void DeInit()
        {
            m_UdpHost.Stop();
        }

        private static void OnMessageReceived(string code)
        {
            var b = m_FunctionEvaluator.Execute(code, out var returnObj);
            var message = new Message()
            {
                ErrorCode = b ? 0 : 1,
                Info = Dumper.Do(returnObj)
            };

            var text = JsonUtility.ToJson(message);
            Debug.Log($"Shell Execute: {code}\n{text}");

            m_UdpHost.Send(text);
        }

        public static object Execute(string code)
        {
            if (m_FunctionEvaluator.Execute(code, out var returnObj))
                return returnObj;
            throw new Exception($"execute failed \n{returnObj}");
        }
    }
}