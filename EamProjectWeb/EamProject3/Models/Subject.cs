using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public partial class Subject
{
    public int Id { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string Name { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<User> Teachers { get; set; } = new List<User>();
}
