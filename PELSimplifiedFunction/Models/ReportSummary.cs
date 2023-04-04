using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PELSimplifiedFunction.Models;
public class ReportSummary
{
    public int Records { get; set; }
    public int ValidRecords { get; set; }
    public InvalidRecords InvalidRecords { get; set; }
}

public class InvalidRecords
{
    public int Count { get; set; }
    public List<LooseProduct> Records { get; set; }
}

