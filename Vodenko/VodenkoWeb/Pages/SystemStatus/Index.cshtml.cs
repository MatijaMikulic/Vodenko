using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModelProvider.Interfaces;
using VodenkoWeb.Model;

namespace VodenkoWeb.Pages.SystemStatus
{
    public class IndexModel : PageModel
    {
        public IModelProvider _modelProvider { get; }

        public IndexModel(IModelProvider modelProvider)
        {
            _modelProvider = modelProvider;
        }
        public void OnGet()
        {
        }
    }
}
