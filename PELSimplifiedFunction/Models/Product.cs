using System;
using System.ComponentModel.DataAnnotations;

namespace PELSimplifiedFunction;

public class Product
{
    [StringLength(70, MinimumLength = 3)]
    public string CertNumber { get; set; }
    [StringLength(150, MinimumLength = 2)]
    public string TestName { get; set; }
    [StringLength(150, MinimumLength = 2)]
    public string TechType { get; set; }
    public DateTime CertifiedFrom { get; set; }
    public DateTime CertifiedTo { get; set; }
    public bool? EmissionsCert { get; set; }
}
