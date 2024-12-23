using System;
using System.Collections.Generic;

namespace TaxService.Models;

public partial class Document {
    public int Documentid { get; set; }

    public int Reportid { get; set; }

    public string Filepath { get; set; } = null!;

    public DateTime? Uploadedat { get; set; }

    public virtual Report Report { get; set; } = null!;
}
