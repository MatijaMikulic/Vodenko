using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModelProvider.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using VodenkoWeb.Model;
using static VodenkoWeb.Pages.RLocus.IndexModel;

namespace VodenkoWeb.Pages.LQR
{
    public class IndexModel : PageModel
    {
        public IModelProvider _modelProvider;
        private IUnitOfWork _unitOfWork;

        [BindProperty]
        public double[][] Q { get; set; }

        [BindProperty]
        public double[][] R { get; set; }

        [BindProperty]
        public double Kx1 { get; set; }
        [BindProperty]
        public double Kx2 { get; set; }
        [BindProperty]
        public double Kx3 { get; set; }

        [BindProperty]
        public double Ki { get; set; }

        public IndexModel(IModelProvider modelProvider,IUnitOfWork unitOfWork)
        {
            _modelProvider = modelProvider;
            _unitOfWork = unitOfWork;

            InitializeMatrices();

        }

        private void InitializeMatrices()
        {
            //Q = new double[3][];
            //R = new double[3][];

            //for (int i = 0; i < 3; i++)
            //{
            //    Q[i] = new double[3]; // Assuming a 3x3 matrix, adjust the size as needed
            //    R[i] = new double[3]; // Assuming a 3x3 matrix, adjust the size as needed

            //    for (int j = 0; j < 3; j++)
            //    {
            //        Q[i][j] = 0.0; // Initialize with default value, can be customized
            //        R[i][j] = 0.0; // Initialize with default value, can be customized
            //    }
            //}

            Kx1 = 0.0;
            Kx2 = 0.0;
            Kx3 = 0.0;
            Ki = 0.0;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostSendLQR()
        {
            
            Message message = new Message
            {
                MessageId = 2,
                Status = 0,
                EnqueueDT = DateTime.Now,
                PayloadDictionary = new Dictionary<string, string>
            {
                { "Method","2" },
                { "Proportional", "-1" },
                { "Integral", "-1" },
                { "Derivative", "-1" },
                { "K1", Kx1.ToString() },
                { "K2", Kx2.ToString() },
                { "K3", Kx3.ToString() },
                { "K4", Ki.ToString() }
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
                { "RequestAutoMode", "0" }
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