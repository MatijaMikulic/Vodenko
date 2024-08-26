using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VodenkoWeb.Services;

namespace VodenkoWeb.Pages.SResponse
{
    public class IndexModel : PageModel
    {
        private readonly RecordService _recordService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ParametersService _parametersService;


        public IndexModel(RecordService recordService, ILogger<IndexModel> logger, IConfiguration configuration, IUnitOfWork unitOfWork, ParametersService parametersService)
        {
            _recordService = recordService;
            _logger = logger;
            _configuration = configuration;
            _unitOfWork = unitOfWork;
            _parametersService = parametersService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSendValveSetpoint([FromBody] ValveSetpointModel setpoint)
        {
            var errors = new List<string>();
            var limits = await _parametersService.GetParameterLimitsAsync("Valve position");

            if (limits.HasValue)
            {
                float min = limits.Value.MinValue;
                float max = limits.Value.MaxValue;

                if (setpoint.Initial < min || setpoint.Initial > max)
                {
                    errors.Add($"Initial valve openness must be between {min} and {max}.");
                }
                if (setpoint.Final < min || setpoint.Final > max)
                {
                    errors.Add($"Final valve openness must be between {min} and {max}.");
                }
            }
            else
            {
                errors.Add("Parameter limits not found.");
            }

            if (errors.Count > 0)
            {
                return new JsonResult(new { success = false, errors });
            }

            Message message = new Message
            {
                MessageId = 1,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
                {
                    { "TargetH2Level", "-1" },
                    { "pvInitialValue", setpoint.Initial.ToString() },
                    { "pvFinalValue", setpoint.Final.ToString() },
                    { "Mode", "0" }
                },
                RetryCount = 0,
                ErrorLog = string.Empty
            };
            int success = await _unitOfWork.MessageRepository.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostRequestControl()
        {
            Message message = new Message
            {
                MessageId = 3,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
                {
                    { "RequestAutoMode", "1" }
                },
                RetryCount = 0,
                ErrorLog = string.Empty
            };
            int success = await _unitOfWork.MessageRepository.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return new JsonResult(new { success = true });

        }
    }

    public class ValveSetpointModel
    {
        public float Initial { get; set; }
        public float Final { get; set; }
    }

}
