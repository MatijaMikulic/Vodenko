using SharedLibrary.Entities;
using DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VodenkoWeb.Pages.Alarms
{
    public class IndexModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTimeFromFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTimeToFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? AlarmCodeIdFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SeverityFilter { get; set; }

        public IEnumerable<Alarm> Alarms { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool FilterEnabled { get; set; }

        public IndexModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task OnGet()
        {
            var allAlarms = await _unitOfWork.AlarmRepository.GetAllAsync();

            if (FilterEnabled)
            {
                if (DateTimeFromFilter.HasValue)
                {
                    allAlarms = allAlarms.Where(a => a.DateTime >= DateTimeFromFilter.Value).ToList();
                }

                if (DateTimeToFilter.HasValue)
                {
                    allAlarms = allAlarms.Where(a => a.DateTime <= DateTimeToFilter.Value).ToList();
                }

                if (AlarmCodeIdFilter.HasValue)
                {
                    allAlarms = allAlarms.Where(a => a.AlarmCodeId == AlarmCodeIdFilter.Value);
                }

                if (!string.IsNullOrEmpty(SeverityFilter))
                {
                    allAlarms = allAlarms.Where(a => a.AlarmCode.Severity == SeverityFilter).ToList();
                }
            }

            Alarms = allAlarms;
        }

        public async Task<IActionResult> OnPostUpdateAlarmAsync(int id, string comment, DateTime dateTime)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }

            var alarm = await _unitOfWork.AlarmRepository.GetFirstOrDefaultAsync(x => x.Id == id);
            if (alarm == null)
            {
                return NotFound("Alarm not found");
            }

            alarm.Comment = comment;
            alarm.DateTime = dateTime;

            await _unitOfWork.AlarmRepository.UpdateAsync(alarm);
            await _unitOfWork.SaveAsync();

            return new JsonResult(new { success = true });
        }
    }
}