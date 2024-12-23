using System;
using System.Collections.Generic;

namespace TaxService.Models;

public partial class Report {
    public int Reportid { get; set; }

    public int Taxpayerid { get; set; }

    public string Reporttype { get; set; } = null!;

    public DateTime? Submissiondate { get; set; }

    public string? Status { get; set; }

    public string? Errordescription { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Taxpayer Taxpayer { get; set; } = null!;
    
}

