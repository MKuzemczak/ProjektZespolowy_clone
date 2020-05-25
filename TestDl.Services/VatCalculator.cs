namespace TestDl.Services
{
    public class VatCalculator : IVatCalculator
    {
        private const int VatValue = 23;

        private readonly ICalculator calculator;
        public VatCalculator(ICalculator calculator)
        {
            this.calculator = calculator;
        }

        public int AddVat(int value)
        {
            var calculatedVatAmount = value * VatValue / 100;
            return calculator.Add(calculatedVatAmount, value);
        }
    }
}
