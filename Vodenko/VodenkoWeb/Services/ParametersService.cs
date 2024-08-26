using DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace VodenkoWeb.Services
{
    public class ParametersService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ParametersService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(float MinValue, float MaxValue)?> GetParameterLimitsAsync(string parameter)
        {
            var param = await _unitOfWork.AuxValuesRepository.GetFirstOrDefaultAsync(p => p.Parameter == parameter);

            if (param != null)
            {
                return (param.MinVaue, param.MaxValue);
            }

            return null;
        }
    }
}
