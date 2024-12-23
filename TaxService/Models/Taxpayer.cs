using System;
using System.Collections.Generic;

namespace TaxService.Models;

public partial class Taxpayer {
    public int Taxpayerid { get; set; }

    public int Userid { get; set; }

    public string Inn { get; set; } = null!;

    public string? Companyname { get; set; }

    public string? Address { get; set; }

    public virtual ICollection<Report> Reports { get; set; } = new List<Report>();

    public User User { get; set; }
}
