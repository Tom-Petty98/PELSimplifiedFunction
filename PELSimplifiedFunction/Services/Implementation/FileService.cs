using ExcelDataReader;
using Microsoft.Extensions.Logging;
using PELSimplifiedFunction.Models;
using PnP.Core.Model.SharePoint;
using PnP.Core.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace PELSimplifiedFunction.Services.Implementation;
public class FileService : IFileService
{
    private const string FilePath = "Shared Documents/Dev/PelTestSpreadsheet.xlsx";
    private const string ReportFilePath = "Shared Documents/Dev/DataExtractRunReports";
    private const string ErrorReportFilePath = "Shared Documents/Dev/DataExtractRunReports/ERROR";

    private readonly IPnPContextFactory _contextFactory;
    private readonly ILogger<FileService> _logger;

    public FileService(IPnPContextFactory contextFactory, ILogger<FileService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<IEnumerable<LooseProduct>> RetrieveExcelFileDataAsync()
    {
        var dataModel = new List<LooseProduct>();
        try
        {
            _logger.LogInformation($"Opening file {FilePath}");
            using (var context = await _contextFactory.CreateAsync("Default"))
            {
                IFile sharepointDoc = await context.Web.GetFileByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/{FilePath}");
                using (var stream = await sharepointDoc.GetContentAsync())
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        DataTable dataTable = reader.AsDataSet().Tables[0];

                        for (int i = 1; i < dataTable.Rows.Count; i++)
                        {
                            var row = dataTable.Rows[i];
                            dataModel.Add(new LooseProduct
                            {
                                CertNumber = row.ItemArray[0]?.ToString(),
                                TestName = row.ItemArray[1]?.ToString(),
                                TechType = row.ItemArray[2]?.ToString(),
                                CertifiedFrom = row.ItemArray[3]?.ToString(),
                                CertifiedTo = row.ItemArray[4]?.ToString(),
                                EmissionsCert = row.ItemArray[5]?.ToString()
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error retrieving excel file: ", ex);
            throw;
        }
        _logger.LogInformation($"Finished retrieving {dataModel.Count} rows");
        return dataModel;
    }

    public async Task WriteReportAsync(IEnumerable<ProductValidationResult> models)
    {
        var allRecordsCount = models.Count();
        var validRecordsCount = models.Count(x => x.IsValid);
        var invalidRecordsCount = allRecordsCount - validRecordsCount;
        var invalidRecords = models.Where(x => !x.IsValid).Select(r => r.Source).ToList();
        var reportFileName = string.Concat(ReportFilePath, $"/SUCESS {DateTime.Now:yyyy-MM-dd}.txt");

        var reportModel = new ReportSummary
        {
            Records = allRecordsCount,
            ValidRecords = validRecordsCount,
            InvalidRecords = new InvalidRecords
            {
                Count = invalidRecordsCount,
                Records = invalidRecords
            }
        };

        using (var context = await _contextFactory.CreateAsync("Default"))
        {
            var siteAssestsFolder = await context.Web.GetFolderByServerRelativeUrlAsync($"{context.Uri.PathAndQuery}/{ReportFilePath}");

            using (var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(reportModel)))
            {
                await siteAssestsFolder.Files.AddAsync(reportFileName, stream);
            }
        }
    }
}

