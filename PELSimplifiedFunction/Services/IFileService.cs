using PELSimplifiedFunction.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PELSimplifiedFunction.Services
{
    public interface IFileService
    {
        Task<IEnumerable<LooseProduct>> RetrieveExcelFileDataAsync();
        Task WriteReportAsync(IEnumerable<ProductValidationResult> models);
    }
}
