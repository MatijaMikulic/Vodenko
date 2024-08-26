using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ModelProvider.Interfaces;
using Newtonsoft.Json;
using System.Text;
using VodenkoWeb.Model;
using VodenkoWeb.Pages.LQR;
using VodenkoWeb.Pages.RLocus;

namespace VodenkoWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ControlSystemAPI : ControllerBase
    {
        private readonly IModelProvider _modelProvider;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ExternalUrls _externalUrls;

        public ControlSystemAPI(IModelProvider modelProvider, IUnitOfWork unitOfWork, IHttpClientFactory httpClientFactory, IOptions<ExternalUrls> externalUrls)
        {
            _modelProvider = modelProvider;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
            _externalUrls = externalUrls.Value;
        }


        [HttpPost("RootLocusPoints")]
        public async Task<IActionResult> RootLocusPoints([FromBody] InputModel input)
        {
            if (input == null || input.InputStrings == null || input.InputStrings.Length == 0)
            {
                return BadRequest("Input strings are empty or null");
            }

            var processedDoubles = new List<double>();
            foreach (var str in input.InputStrings)
            {
                if (double.TryParse(str, out var parsedDouble))
                {
                    processedDoubles.Add(parsedDouble);
                }
                else
                {
                    return BadRequest($"Invalid number format: {str}");
                }
            }

            var httpClient = _httpClientFactory.CreateClient();
            var data = new
            {
                A = _modelProvider.MathematicalModel.Model.A,
                B = _modelProvider.MathematicalModel.Model.B,
                C = _modelProvider.MathematicalModel.Model.C,
                D = _modelProvider.MathematicalModel.Model.D,
                Kp = processedDoubles[0],
                Ki = processedDoubles[1],
                Kd = processedDoubles[2]
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_externalUrls.RootLocus, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var rlocusPlotData = JsonConvert.DeserializeObject<RootLocusData>(responseBody);
                return Ok(rlocusPlotData);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error: Unable to get root locus data");
            }
        }

        [HttpPost("StepResponsePoints")]
        public async Task<IActionResult> StepResponsePoints([FromBody] InputModel input)
        {
            if (input == null || input.InputStrings == null || input.InputStrings.Length == 0)
            {
                return BadRequest("Input strings are empty or null");
            }

            var processedDoubles = new List<double>();
            foreach (var str in input.InputStrings)
            {
                if (double.TryParse(str, out var parsedDouble))
                {
                    processedDoubles.Add(parsedDouble);
                }
                else
                {
                    return BadRequest($"Invalid number format: {str}");
                }
            }

            var httpClient = _httpClientFactory.CreateClient();
            var data = new
            {
                A = _modelProvider.MathematicalModel.Model.A,
                B = _modelProvider.MathematicalModel.Model.B,
                C = _modelProvider.MathematicalModel.Model.C,
                D = _modelProvider.MathematicalModel.Model.D,
                Kp = processedDoubles[0],
                Ki = processedDoubles[1],
                Kd = processedDoubles[2]
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_externalUrls.StepResponse, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var stepResponse = JsonConvert.DeserializeObject<StepRespnseData>(responseBody);
                return Ok(stepResponse);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error: Unable to get step response data");
            }
        }

        [HttpPost("BodePoints")]
        public async Task<IActionResult> BodePoints([FromBody] InputModel input)
        {
            if (input == null || input.InputStrings == null || input.InputStrings.Length == 0)
            {
                return BadRequest("Input strings are empty or null");
            }

            var processedDoubles = new List<double>();
            foreach (var str in input.InputStrings)
            {
                if (double.TryParse(str, out var parsedDouble))
                {
                    processedDoubles.Add(parsedDouble);
                }
                else
                {
                    return BadRequest($"Invalid number format: {str}");
                }
            }

            var httpClient = _httpClientFactory.CreateClient();
            var data = new
            {
                A = _modelProvider.MathematicalModel.Model.A,
                B = _modelProvider.MathematicalModel.Model.B,
                C = _modelProvider.MathematicalModel.Model.C,
                D = _modelProvider.MathematicalModel.Model.D,
                Kp = processedDoubles[0],
                Ki = processedDoubles[1],
                Kd = processedDoubles[2]
            };

            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_externalUrls.BodePlot, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var bodePlotData = JsonConvert.DeserializeObject<BodeData>(responseBody);
                return Ok(bodePlotData);
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error: Unable to get bode plot data");
            }
        }

        [HttpPost("CalculateLQR")]
        public async Task<IActionResult> OnPostCalculateAsync([FromBody] MatrixData input)
        {
            if (input == null || input.Q == null || input.R == null)
            {
                return BadRequest(new { error = "Invalid input matrices" });
            }

            try
            {
                var result = await CalculateLQRAsync(input.Q, input.R);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private async Task<LQRData> CalculateLQRAsync(double[][] Q, double[][] R)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var data = new
                {
                    A = _modelProvider.MathematicalModel.Model.A,
                    B = _modelProvider.MathematicalModel.Model.B,
                    C = _modelProvider.MathematicalModel.Model.C,
                    D = _modelProvider.MathematicalModel.Model.D,
                    Q = Q,
                    R = R
                };

                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(_externalUrls.CalculateLQR, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var body = JsonConvert.DeserializeObject<LQRData>(responseBody);
                    return body;
                }
                else
                {
                    throw new Exception("Error: Unable to get LQR gain");
                }
            }
        }
    }

    public class PIDModel
    {
        public double Kp { get; set; }
        public double Ki { get; set; }
        public double Kd { get; set; }
    }

    public class InputModel
    {
        public string[] InputStrings { get; set; }
    }

    public class RootLocusData
    {
        public Dictionary<int, PoleData> grouped_locus { get; set; }

        public double[] poles_real { get; set; }
        public double[] poles_imag { get; set; }

        public double[] zeros_real { get; set; }
        public double[] zeros_imag { get; set; }

        public double[] angles { get; set; }
        public double centroid { get; set; }
    }
    public class PoleData
    {
        public List<double> real { get; set; }
        public List<double> imag { get; set; }
        public List<double> gain { get; set; }
    }

    public class BodeData
    {
        public double[] frequency { get; set; }
        public double[] magnitude { get; set; }
        public double[] phase { get; set; }

        public double gain_cross_freq { get; set; }
        public double phase_cross_freq { get; set; }
        public double gain_margin { get; set; }
        public double phase_margin { get; set; }

    }

    public class StepRespnseData
    {
        public double[] time { get; set; }
        public double[] response { get; set; }

        public double overshoot { get; set; }
        public double rise_time { get; set; }
        public double settling_time { get; set; }

        public double error { get; set; }
    }

    public class MatrixData
    {
        public double[][] Q { get; set; }
        public double[][] R { get; set; }
    }

    public class LQRData
    {
        public double Kx1 { get; set; }
        public double Kx2 { get; set; }
        public double Kx3 { get; set; }
        public double Ki { get; set; }
        public double[] time { get; set; }
        public double[] y { get; set; }
        public double[] u { get; set; }

        public double overshoot { get; set; }
        public double rise_time { get; set; }
        public double settling_time { get; set; }
        public double error { get; set; }

    }


}
