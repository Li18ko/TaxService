using System;
using System.Collections.Generic;

namespace TaxService.Models;

public class User {
    public int Userid { get; set; }

    public string Fullname { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual ICollection<Taxpayer> Taxpayers { get; set; } = new List<Taxpayer>();
}
