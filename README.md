# Student Mark Prediction Application

This is a Windows Presentation Foundation (WPF) application using machine learning (ML.NET) to predict student marks based on the amount of hours studied and the attendance of the student.

The application consists of three primary components:
1. A machine learning model.
2. A graphical user interface for training the model, loading data, and making predictions.
3. Data classes for the student data and the mark predictions.

## Dependencies

The application requires the following NuGet packages:
- `Microsoft.ML`
- `Microsoft.Win32`

## File Structure

- `StudentData.cs`: Class representing the student data with features to predict marks.
- `MarksPrediction.cs`: Class representing a prediction of student marks.
- `MainWindow.xaml`: The main GUI for the application.
- `MainWindow.xaml.cs`: The logic behind the GUI, including the machine learning pipeline and model operations.

## Features

- **Train the Model**: This functionality is used to train the model with a selected CSV file containing the student data.
- **Load the Model**: Once a model has been trained and saved, this button can be used to load the saved model for use in making predictions.
- **Predict Marks**: Given the number of hours studied and the attendance, the application will predict the marks of a student.
- **File Selection**: A CSV file with the student data can be selected for training the model.
- **Displaying Averages**: Displays the average values of the hours studied, attendance, and marks from the loaded data.

## How to Use

1. Clone this repository to your local machine.
2. Open the solution in Visual Studio.
3. Run the application by pressing `F5` or clicking the `Start Debugging` button.
4. Click the `Open File` button and select a CSV file containing your student data.
5. Click the `Train and Save Model` button to train the machine learning model on your data and save it to your local machine.
6. Click the `Load Model` button to load the trained model.
7. Enter the values for the number of hours studied and attendance, then click the `Predict for Custom Values` button to make a prediction.

## Dataset

The dataset used to train the model should be in CSV format and contain four columns: `StudentID`, `HoursStudied`, `Attendance`, and `Marks`. The first row of the CSV file should be the column headers.

## Note

Before making predictions, make sure to load a pre-trained model or train a new one. Also, ensure to load valid student data from a CSV file.

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License

This project is licensed under the MIT License.
