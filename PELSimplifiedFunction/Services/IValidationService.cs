using PELSimplifiedFunction.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PELSimplifiedFunction.Services;

public interface IValidationService
{
    Task<IEnumerable<ProductValidationResult>> ValidateAsync(IEnumerable<LooseProduct> models);

    IEnumerable<Product> BuildValidResults(IEnumerable<ProductValidationResult> models);
}
