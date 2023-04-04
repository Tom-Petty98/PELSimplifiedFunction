using FluentValidation;
using Microsoft.Extensions.Logging;
using PELSimplifiedFunction.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PELSimplifiedFunction.Services.Implementation;
public class ProductValidator : AbstractValidator<LooseProduct>
{
    public ProductValidator()
    {
        RuleFor(x => x).NotEmpty();
        RuleFor(x => x.CertNumber).NotEmpty().WithMessage("No CertNumber");
        RuleFor(x => x.TestName).NotEmpty().WithMessage("No TestName");
        RuleFor(x => x.TechType).NotEmpty().WithMessage("No TechType");
        RuleFor(x => x.CertifiedFrom).NotEmpty().WithMessage("No CertifiedFrom");
        RuleFor(x => x.CertifiedTo).NotEmpty().WithMessage("No CertifiedTo");
        RuleFor(x => x.CertifiedFrom).Must(d => BeAValidDate(d, "CertifiedFrom")).WithMessage("Invalid date/time");
        RuleFor(x => x.CertifiedTo).Must(d => BeAValidDate(d, "CertifiedTo")).WithMessage("Invalid date/time");
        RuleFor(x => x.CertNumber).Length(3, 75);
    }

    private bool BeAValidDate(string value, string fieldName)
    {
        DateTime date;
        if (!DateTime.TryParse(value, out date)) return false;
        if (date >= DateTime.UtcNow && fieldName == "CertifiedFrom") return false;
        if (date <= DateTime.UtcNow && fieldName == "CertifiedTo") return false;

        return true;
    }
}

public class ValidationService : IValidationService
{
    private readonly IValidator<LooseProduct> _validator;
    private readonly ILogger<ValidationService> _logger;

    public ValidationService(IValidator<LooseProduct> validator, ILogger<ValidationService> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductValidationResult>> ValidateAsync(IEnumerable<LooseProduct> models)
    {
        List<ProductValidationResult> validationResults = new();

        foreach (var model in models)
        {
            var validationResult = await _validator.ValidateAsync(model);
            validationResults.Add(new ProductValidationResult { Source = model, ValidationResult = validationResult });
        }

        _logger.LogInformation($"Found {validationResults!.Count}");
        return validationResults;
    }

    public IEnumerable<Product> BuildValidResults(IEnumerable<ProductValidationResult> validationResults)
    {
        return validationResults.Where(x => x.IsValid).Select(x => new Product
        {
            CertNumber = x.Source.CertNumber,
            TestName = x.Source.TestName,
            TechType = x.Source.TechType,
            CertifiedFrom = DateTime.Parse(x.Source.CertifiedFrom),
            CertifiedTo = DateTime.Parse(x.Source.CertifiedTo),
            EmissionsCert = ConvertToBool(x.Source.EmissionsCert),
        }).ToList();
    }

    private bool? ConvertToBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        bool result;
        if (!Boolean.TryParse(value, out result)) return null;
        return result;
    }
}
