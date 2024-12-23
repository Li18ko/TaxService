using System;
using System.Collections.Generic;

namespace TaxService.Models;

public partial class Log {
    public int Logid { get; set; }

    public int Userid { get; set; }

    public string Action { get; set; } = null!;

    public DateTime? Actiondate { get; set; }

    public virtual User User { get; set; } = null!;
}
