using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using PELSimplifiedFunction.Services;

namespace PELSimplifiedFunction;
public class ProcessSpreadsheet
{
    private readonly IFileService _fileService;
    private readonly IValidationService _validationService;
    private readonly ILogger<ProcessSpreadsheet> _logger;

    public ProcessSpreadsheet(IFileService fileService, IValidationService validationService, ILogger<ProcessSpreadsheet> logger)
    {
        _fileService = fileService;
        _validationService = validationService;
        _logger = logger;
    }

    [Function("ProcessSpreadsheet")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        _logger.LogInformation($"Process spreadsheet function triggered.");

        try
        {
            _logger.LogInformation($"Retriving file data");
            var looseModel = await _fileService.RetrieveExcelFileDataAsync();

            _logger.LogInformation($"Validating file data");
            var validationResults = await _validationService.ValidateAsync(looseModel);

            _logger.LogInformation($"Mapping valid records to model");
            var products = _validationService.BuildValidResults(validationResults);

            _logger.LogInformation($"Generating summary report");
            await _fileService.WriteReportAsync(validationResults);

            _logger.LogInformation("Finished");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new OkObjectResult(ex.Message);
        }

        return new OkObjectResult("Spreasheet processed successfully");
    }
}