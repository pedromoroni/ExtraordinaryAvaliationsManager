using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public partial class TeacherSubject
{
    public int TeacherId { get; set; }

    public int SubjectId { get; set; }

    public virtual Class Subject { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
