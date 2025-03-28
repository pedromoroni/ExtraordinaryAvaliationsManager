using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public partial class Situation
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public DateOnly? StartAt { get; set; }

    public DateOnly? EndAt { get; set; }

    public decimal Tax { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
