using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Entities;

namespace VodenkoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControlRequestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ControlRequestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("request-control")]
        public async Task<IActionResult> RequestControl([FromBody] bool mode)
        {
            string modeValue = mode ? "1" : "0";

            Message message = new Message
            {
                MessageId = 3,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
                {
                    { "RequestAutoMode", modeValue }
                },
                RetryCount = 0,
                ErrorLog = string.Empty
            };

            int success = await _unitOfWork.MessageRepository.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return new JsonResult(new { success = true });
        }

        [HttpPost("request-stop")]
        public async Task<IActionResult> RequestStop()
        {
            Message message = new Message
            {
                MessageId = 3,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
                {
                    { "RequestAutoMode", "2" }
                },
                RetryCount = 0,
                ErrorLog = string.Empty
            };

            int success = await _unitOfWork.MessageRepository.AddAsync(message);
            await _unitOfWork.SaveAsync();
            return new JsonResult(new { success = true });
        }
    }
}
