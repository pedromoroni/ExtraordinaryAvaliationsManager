using System;
using System.Collections.Generic;

namespace EamBackOffice01.Models;

public partial class Request
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public int StudentId { get; set; }

    public int StatusId { get; set; }

    public DateTime ExamDatetime { get; set; }

    public int DurationMin { get; set; }

    public int CourseId { get; set; }

    public int ModuleId { get; set; }

    public int TeacherId { get; set; }

    public int SituationId { get; set; }

    public decimal? Grade { get; set; }

    public string? PaymentMethod { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<RequestHistory> RequestHistories { get; set; } = new List<RequestHistory>();

    public virtual Situation Situation { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual User Student { get; set; } = null!;

    public virtual User Teacher { get; set; } = null!;
}
