using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VodenkoWeb.Model;
using VodenkoWeb.Services;

namespace VodenkoWeb.Pages.ControllerAnalysis
{
    public class IndexModel : PageModel
    {
        private readonly RecordService _recordService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private IUnitOfWork _unitOfWork;
        private readonly ParametersService _parameterService;

        public IndexModel(RecordService recordService, ILogger<IndexModel> logger, IConfiguration configuration, IUnitOfWork unitOfWork, ParametersService parametersService)
        {
            _recordService = recordService;
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _parameterService = parametersService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSendH2Setpoint([FromBody] SetpointModel setpoint)
        {
            var limits = await _parameterService.GetParameterLimitsAsync("Tank 2 Water level");

            if (limits.HasValue)
            {
                float min = limits.Value.MinValue;
                float max = limits.Value.MaxValue;

                if (setpoint.H2Setpoint < min || setpoint.H2Setpoint > max)
                {
                    ModelState.AddModelError(string.Empty, $"Setpoint must be between {min} and {max}.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Parameter limits not found.");
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return new JsonResult(new { success = false, errors });
            }

            Message message = new Message
            {
                MessageId = 1,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
                {
                    { "TargetH2Level", setpoint.H2Setpoint.ToString() },
                    { "pvInitialValue","-1" },
                    { "pvFinalValue","-1" },
                    { "Mode","1" }
                },
                RetryCount = 0,
                ErrorLog = string.Empty
            };

            int success = await _unitOfWork.MessageRepository.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return new JsonResult(new { success = true });
        }
    }

    public class SetpointModel
    {
        public float H2Setpoint { get; set; }
    }

}
