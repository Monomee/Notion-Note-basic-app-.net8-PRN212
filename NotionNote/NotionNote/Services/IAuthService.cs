using NotionNote.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotionNote.Services
{
    public interface IAuthService
    {
        User? Login(string username, string password);
        User? Register(string username, string password);
        bool UsernameExists(string username);
    }
}
