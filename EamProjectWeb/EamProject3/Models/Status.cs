using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public partial class Status
{
    public int Id { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<RequestHistory> RequestHistories { get; set; } = new List<RequestHistory>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
