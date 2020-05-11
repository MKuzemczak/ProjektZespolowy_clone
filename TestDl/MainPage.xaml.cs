using Autofac;
using TestDl.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestDl
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly ICalculator calculator;
        private readonly IVatCalculator vatCalculator;

        public MainPage()
        {
            this.InitializeComponent();
            calculator = App.Container.Resolve<ICalculator>();
            vatCalculator = App.Container.Resolve<IVatCalculator>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Result.Text = calculator.Add(200, 500).ToString();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var calculationResult = int.Parse(Result.Text);

            ResultWithVat.Text = vatCalculator.AddVat(calculationResult).ToString();
        }
    }
}
