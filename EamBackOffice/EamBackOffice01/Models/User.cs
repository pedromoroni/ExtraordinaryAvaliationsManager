using System;
using System.Collections.Generic;

namespace EamBackOffice01.Models;

public partial class User
{
    public int Id { get; set; }

    public string Identification { get; set; } = null!;

    public string Nif { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public byte[] ProfilePic { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public int? ClassId { get; set; }

    public DateOnly BirthDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Class? Class { get; set; }

    public virtual ICollection<RequestHistory> RequestHistories { get; set; } = new List<RequestHistory>();

    public virtual ICollection<Request> RequestStudents { get; set; } = new List<Request>();

    public virtual ICollection<Request> RequestTeachers { get; set; } = new List<Request>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
