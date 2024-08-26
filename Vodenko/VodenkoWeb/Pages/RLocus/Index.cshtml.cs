using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModelProvider.Interfaces;
using Newtonsoft.Json;
using System.Text;
using VodenkoWeb.Model;

namespace VodenkoWeb.Pages.RLocus
{
    public class IndexModel : PageModel
    {
        public IModelProvider _modelProvider;
        private IUnitOfWork _unitOfWork;

        public IndexModel(IModelProvider modelProvider,IUnitOfWork unitOfWork)
        {
            _modelProvider = modelProvider;
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public double Kp { get; set; }
        [BindProperty]
        public double Ki { get; set; }
        [BindProperty]
        public double Kd { get; set; }

        public async Task<IActionResult> OnPostSendPIDAsync()
        {
            if(Kp == 0)
            {
                return new JsonResult(new { error = true });
            }
            Message message = new Message
            {
                MessageId = 2,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
            {
                { "Method","1" },
                { "Proportional", Kp.ToString() },
                { "Integral", Ki.ToString() },
                { "Derivative", Kd.ToString() },
                { "K1", "-1" },
                { "K2", "-1" },
                { "K3", "-1" },
                { "K4", "-1" }
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
}
