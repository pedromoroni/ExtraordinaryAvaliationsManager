using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public partial class Class
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int MaxStudents { get; set; }

    public int CourseId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
