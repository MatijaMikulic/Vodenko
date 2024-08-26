using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SharedLibrary.Entities;
using VodenkoWeb.Model;
using VodenkoWeb.Services;

namespace VodenkoWeb.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public readonly DynamicDataBuffer _buffer;
        private readonly RecordService _recordService;
        private readonly IConfiguration _configuration;

        private readonly IUnitOfWork _unitOfWork;

        public IndexModel(ILogger<IndexModel> logger,DynamicDataBuffer buffer, RecordService recordService, IConfiguration configuration, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _buffer = buffer;
            _recordService = recordService;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPostStartRecording()
        {
            try
            {
                _recordService.StartRecording();
                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start recording.");
                return new JsonResult(new { success = false });
            }
        }

        public async Task<IActionResult> OnPostStopRecordingAsync()
        {
            try
            {
                var directoryPath = _configuration["Recording:DirectoryPath"] ?? "RecordedData";
                string filePath = Path.Combine(directoryPath, $"Data_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                await _recordService.SaveRecordedDataToFileAsync(filePath);
                _recordService.StopRecording();
                return new JsonResult(new { success = true, filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop recording.");
                return new JsonResult(new { success = false });
            }
        }

    }
}