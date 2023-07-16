namespace PerfToolkit
{
    public class TestEvaluator
    {
        private static int StaticPrivateValue;
        public static int StaticPublicValue;
        
        public static int StaticGetSetValue
        {
            get => StaticPrivateValue;
            set => StaticPrivateValue = value;
        }

        private static TestEvaluator m_Evaluator; 
        public static TestEvaluator GetInstance()
        {
            return m_Evaluator ??= new TestEvaluator();
        }
        
        public static float StaticAdd(float a, float b)
        {
            return a + b;
        }
        
        public int PublicValue;
        public float PublicValue2;
        private int m_PrivateValue;
        
        public int GetSetValue
        {
            get => m_PrivateValue;
            set => m_PrivateValue = value;
        }
        
        public float Add(float a, float b)
        {
            return a + b;
        }
    }
}