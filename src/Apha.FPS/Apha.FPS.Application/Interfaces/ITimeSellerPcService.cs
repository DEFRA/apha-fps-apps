using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface ITimeSellerPcService
    {
        /// <summary>
        /// Returns the grid rows for the Income/Contribution from Time Sales form,
        /// filtered to the given selling profit centre and current FPS year.
        /// </summary>
        Task<List<TimeSellerPcRowDto>> GetRowsAsync(string sellingPc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the footer totals for the Income/Contribution from Time Sales form.
        /// Includes animal cost adjustment when sellingPc equals "ASU".
        /// </summary>
        Task<TimeSellerPcTotalsDto> GetTotalsAsync(string sellingPc, CancellationToken cancellationToken = default);
    }
}
