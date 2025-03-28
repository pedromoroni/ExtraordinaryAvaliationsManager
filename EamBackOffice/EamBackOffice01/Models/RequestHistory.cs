using System;
using System.Collections.Generic;

namespace EamBackOffice01.Models;

public partial class RequestHistory
{
    public int RequestId { get; set; }

    public int StatusId { get; set; }

    public int UserId { get; set; }

    public DateTime Datetime { get; set; }

    public virtual Request Request { get; set; } = null!;

    public virtual Status Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
