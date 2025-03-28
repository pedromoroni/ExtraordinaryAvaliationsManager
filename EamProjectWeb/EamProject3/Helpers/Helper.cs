using EamProject3.Models;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;

public static class Helper
{
    public static bool VerifyUser(User? user, int roleId)
    {
        if (user == null)
        {
            return false;
        }
        if (user.Id == -1) // caso tenha sido logout
        {
            return false;
        }
        if (user.IsDeleted)
        {
            return false;
        }
        if (user.RoleId == 1 && (user.ClassId == null || user.Class!.IsDeleted)) 
        {
            return false;
        }
        if (user.RoleId != roleId)
        {
            return false;
        }

        return true;
    }
}