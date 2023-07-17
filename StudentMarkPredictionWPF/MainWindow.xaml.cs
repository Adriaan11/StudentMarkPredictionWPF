using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Threading;

namespace StudentMarkPredictionWPF
{
    public class StudentData
    {
        [LoadColumn(0)]
        public float StudentID; // Column attribute indicating the index of the column in the data file
        [LoadColumn(1)]
        public float HoursStudied; // Column attribute indicating the index of the column in the data file
        [LoadColumn(2)]
        public float Attendance; // Column attribute indicating the index of the column in the data file
        [LoadColumn(3)]
        public float Marks; // Column attribute indicating the index of the column in the data file
    }
    public class MarksPrediction
    {
        [ColumnName("Score")]
        public float Marks; // The predicted marks value
    }
    public partial class MainWindow : Window
    {
        // Variables
        private string _dataPath; // Path to the data file
        private CancellationTokenSource _cancellationTokenSource; // Used for cancelling the prediction task
        private string modelPath = @"C:\Users\Adriaan\Downloads\studentMarkModel.zip"; // Path of the trained model file
        private readonly MLContext _mlContext = new MLContext();
        private ITransformer _model;
        private IDataView _dataView;
        private List<StudentData> _dataCache;
        private List<float> predictions = new List<float>(); // List to store predicted marks
        public MainWindow()
        {
            InitializeComponent();

            // Initialize data path and update averages on UI
            _dataPath = @"C:\Users\Adriaan\Downloads\data2.csv";
            UpdateAverages();
        }
        // Train and save the model asynchronously
        private async Task TrainAndSaveModelAsync()
        {
            await LoadDataAsync();
            if (_dataCache == null || !_dataCache.Any()) return;

            await Task.Run(() =>
            {
                _dataView = _mlContext.Data.LoadFromEnumerable(_dataCache);
                _model = TrainAndSaveModel(_mlContext, _dataView);
            });
        }
        // Load data from a file using DataView and DataLoader
        private IDataView LoadData()
        {
            if (string.IsNullOrEmpty(_dataPath))
            {
                MessageBox.Show("Please select a file first.");
                return null;
            }
            try
            {
                var dataLoader = _mlContext.Data.CreateTextLoader<StudentData>(separatorChar: ',',
                    hasHeader: true,
                    allowQuoting: true,
                    allowSparse: false);

                return dataLoader.Load(_dataPath);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to load data from {_dataPath}. Please make sure the file exists and is accessible.");
            }
            return null;
        }
        // Load data asynchronously
        private async Task LoadDataAsync()
        {
            if (_dataCache != null) return;
            var dataView = LoadData();
            if (dataView == null) return;
            _dataCache = _mlContext.Data.CreateEnumerable<StudentData>(dataView, reuseRowObject: true).ToList();
        }
        // Update the average values on the UI
        private void UpdateAverages()
        {
            var dataView = LoadData();
            if (dataView == null || dataView.GetRowCount() == 0) return;
            var data = _mlContext.Data.CreateEnumerable<StudentData>(dataView, reuseRowObject: true);
            AverageHoursStudiedTextBlock.Text = "Average Hours Studied: " + Math.Round(data.Average(d => d.HoursStudied), 2);
            AverageAttendanceTextBlock.Text = "Average Attendance: " + Math.Round(data.Average(d => d.Attendance), 2);
            AverageMarksTextBlock.Text = "Average Marks: " + Math.Round(data.Average(d => d.Marks), 2);
        }
        // Train and save the model
        private ITransformer TrainAndSaveModel(MLContext mlContext, IDataView trainingDataView)
        {
            var pipeline = mlContext.Transforms.Concatenate("Features", "HoursStudied", "Attendance")
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Marks", maximumNumberOfIterations: 100));
            var model = pipeline.Fit(trainingDataView);
            mlContext.Model.Save(model, trainingDataView.Schema, modelPath); // Save the trained model to a file
            return model;
        }
        // Load the model from a file
        private ITransformer LoadModel(MLContext mlContext, out DataViewSchema modelSchema)
        {
            return mlContext.Model.Load(modelPath, out modelSchema);
        }
        // Event handler for "Train and Save Model" button click
        private async void TrainAndSaveModelButton_Click(object sender, RoutedEventArgs e)
        {
            await TrainAndSaveModelAsync();
        }
        // Event handler for "Load Model" button click
        private void LoadModelButton_Click(object sender, RoutedEventArgs e)
        {
            var mlContext = new MLContext();
            LoadModel(mlContext, out _); // Call this when you want to load the model
        }
        // Event handler for "Open File" button click
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                _dataPath = openFileDialog.FileName;
                UpdateAverages();
            }
        }
        // Predict marks using the trained model
        private (float PredictedMarks, float ActualMarks, RegressionMetrics Metrics) PredictMarks(MLContext mlContext, ITransformer model, IDataView dataView, StudentData sample)
        {
            var prediction = model.Transform(dataView);
            var metrics = mlContext.Regression.Evaluate(prediction, labelColumnName: "Marks");
            var predictor = mlContext.Model.CreatePredictionEngine<StudentData, MarksPrediction>(model);
            float predictedMarks = (float)Math.Round(predictor.Predict(sample).Marks, 2);
            float actualMarks = sample.Marks;
            return (predictedMarks, actualMarks, metrics);
        }
        // Train the model
        private ITransformer TrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            var pipeline = mlContext.Transforms.Concatenate("Features", "HoursStudied", "Attendance")
                .Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Marks", maximumNumberOfIterations: 100));

            return pipeline.Fit(trainingDataView);
        }
        // Event handler for "Predict" button click
        private async void PredictButton_Click(object sender, RoutedEventArgs e)
        {
            float hoursStudied = float.Parse(HoursStudiedTextBox.Text);
            float attendance = float.Parse(AttendanceTextBox.Text);
            int loopCount = int.Parse(NumberOfTestsTextBox.Text);
            await Task.Run(() =>
            {
                var mlContext = new MLContext();
                var trainingDataView = mlContext.Data.LoadFromTextFile<StudentData>(_dataPath, separatorChar: ',');
                for (int i = 0; i < loopCount; i++)
                {
                    var model = TrainModel(mlContext, trainingDataView);
                    var newSample = new StudentData { StudentID = 6, HoursStudied = hoursStudied, Attendance = attendance };
                    var (predictedMarks, actualMarks, metrics) = PredictMarks(mlContext, model, trainingDataView, newSample);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RunDetailsListBox.Items.Add($"Custom Prediction for Hours Studied = {hoursStudied}, Attendance = {attendance}");
                        RunDetailsListBox.Items.Add($"R^2 Score (coefficient of determination): {Math.Round(metrics.RSquared, 2)}");

                        RunDetailsListBox.Items.Add($"Predicted Marks: {predictedMarks}");
                        RunDetailsListBox.Items.Add("----------------------------------------------------");
                    });
                }              
            });
        }
        // Event handler for "Reset" button click
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource?.Cancel();
            // Reset UI elements
            ProgressBar1.Value = 0;
            RunDetailsListBox.Items.Clear();
            AveragePredictionTextBlock.Text = "Average Prediction:";
            // Reset data
            predictions.Clear();
        }
    }
}
