using System;
using System.Collections.Generic;

namespace EamBackOffice01.Models;

public partial class Course
{
    public int Id { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
