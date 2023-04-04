using FluentValidation.Results;

namespace PELSimplifiedFunction.Models
{
    public class ProductValidationResult
    {
        public LooseProduct Source { get; set; }
        public ValidationResult ValidationResult { get; set; }
        public bool IsValid => ValidationResult.IsValid;
    }
}
