using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public partial class Module
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public int SubjectId { get; set; }

    public int DurationMin { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual Subject Subject { get; set; } = null!;
}
