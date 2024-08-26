using OfficeOpenXml;
using System.Collections.Concurrent;
using System.Globalization;
using VodenkoWeb.Model;

namespace VodenkoWeb.Services
{
    public class RecordService
    {
        private bool _isRecording;
        private readonly ConcurrentQueue<DataPoints> _recordedData;
        private readonly ILogger<RecordService> _logger;

        public RecordService(ILogger<RecordService> logger)
        {
            _isRecording = false;
            _recordedData = new ConcurrentQueue<DataPoints>();
            _logger = logger;
        }

        public void StartRecording()
        {
            _isRecording = true;
            _recordedData.Clear(); // Clear previously recorded data
            _logger.LogInformation("Started recording data.");
        }

        public void StopRecording()
        {
            _isRecording = false;
            _logger.LogInformation("Stopped recording data.");
        }

        public bool IsRecording => _isRecording;

        public void RecordData(DataPoints data)
        {
            if (_isRecording)
            {
                _recordedData.Enqueue(data);
                _logger.LogInformation($"Recorded data: {data}");
            }
        }

        public async Task SaveRecordedDataToFileAsync(string filePath)
        {
            try
            {
                _logger.LogInformation("Saving recorded data to file.");

                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"Created directory: {directory}");
                }

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("RecordedData");
                    var row = 1;
                    // Adding headers
                    worksheet.Cells[row, 1].Value = "ValvePositionFeedback";
                    worksheet.Cells[row, 2].Value = "InletFlow";
                    worksheet.Cells[row, 3].Value = "WaterLevelTank1";
                    worksheet.Cells[row, 4].Value = "WaterLevelTank2";
                    worksheet.Cells[row, 5].Value = "InletFlowNonLinModel";
                    worksheet.Cells[row, 6].Value = "WaterLevelTank1NonLinModel";
                    worksheet.Cells[row, 7].Value = "WaterLevelTank2NonLinModel";
                    worksheet.Cells[row, 8].Value = "OutletFlow";
                    worksheet.Cells[row, 9].Value = "DateTime";
                    worksheet.Cells[row, 10].Value = "Flag";
                    worksheet.Cells[row, 11].Value = "Sample";
                    worksheet.Cells[row, 12].Value = "Target";
                    worksheet.Cells[row, 13].Value = "InletFlowLinModel";
                    worksheet.Cells[row, 14].Value = "WaterLevelTank1LinModel";
                    worksheet.Cells[row, 15].Value = "WaterLevelTank2LinModel";

                    foreach (var data in _recordedData)
                    {
                        row++;
                        worksheet.Cells[row, 1].Value = data.ValvePositionFeedback;
                        worksheet.Cells[row, 2].Value = data.InletFlow;
                        worksheet.Cells[row, 3].Value = data.WaterLevelTank1;
                        worksheet.Cells[row, 4].Value = data.WaterLevelTank2;
                        worksheet.Cells[row, 5].Value = data.InletFlowNonLinModel;
                        worksheet.Cells[row, 6].Value = data.WaterLevelTank1NonLinModel;
                        worksheet.Cells[row, 7].Value = data.WaterLevelTank2NonLinModel;
                        worksheet.Cells[row, 8].Value = data.OutletFlow;
                        worksheet.Cells[row, 9].Value = data.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture); //DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture);//data.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                        worksheet.Cells[row, 10].Value = data.IsPumpActive;
                        worksheet.Cells[row, 11].Value = data.Sample;
                        worksheet.Cells[row, 12].Value = data.TargetWaterLevelTank2Model;
                        worksheet.Cells[row, 13].Value = data.InletFlowLinModel;
                        worksheet.Cells[row, 14].Value = data.WaterLevelTank1LinModel;
                        worksheet.Cells[row, 15].Value = data.WaterLevelTank2LinModel;
                    }

                    var file = new FileInfo(filePath);
                    await package.SaveAsAsync(file);

                    _logger.LogInformation($"Successfully saved data to {filePath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save recorded data to file.");
                throw;
            }
        }
    }
}
