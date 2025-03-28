using System;
using System.Collections.Generic;

namespace EamBackOffice01.Models;

public partial class TeacherSubject
{
    public int TeacherId { get; set; }

    public int SubjectId { get; set; }

    public virtual Subject Subject { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
