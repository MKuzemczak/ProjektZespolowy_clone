namespace TestDl.Services
{
    public class Calculator : ICalculator
    {
        public int Add(int a, int b)
        {
            checked
            {
                return a + b;
            }
        }

        public int Sub(int a, int b)
        {
            //TODO: Add checked to prevent int overflow
            return a - b;
        }
    }
}
