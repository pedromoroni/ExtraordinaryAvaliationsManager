using System;
using System.Collections.Generic;

namespace EamProject3.Models;

public class UserDashboardViewModel
{
    public User User { get; set; }
    public List<Request> Requests { get; set; }
    public string ProfilePictureBase64 { get; set; }
}
