using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveCharts;
using LiveCharts.Wpf;


namespace StudentMarkPredictionWPF
{
    /// <summary>
    /// Interaction logic for Graphs.xaml
    /// </summary>
    public partial class Graphs : Page
    {
        public Graphs(List<float> predictions)
        {
            InitializeComponent();

            SeriesCollection seriesCollection = new SeriesCollection
        {
            new LineSeries
            {
                Title = "Predictions",
                Values = new ChartValues<float>(predictions)
            }
        };

            string[] labels = new string[predictions.Count];
            for (int i = 0; i < predictions.Count; i++)
            {
                labels[i] = (i + 1).ToString();
            }



            Func<double, string> formatter = value => value.ToString("N2");

            SeriesCollection = seriesCollection;
            Labels = labels;
            Formatter = formatter;

            DataContext = this;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> Formatter { get; set; }
    }

}
