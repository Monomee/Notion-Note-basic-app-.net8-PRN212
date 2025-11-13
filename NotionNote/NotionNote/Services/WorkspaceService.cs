using Microsoft.EntityFrameworkCore;
using NotionNote.Models;
using System.Collections.Generic;
using System.Linq;

namespace NotionNote.Services
{
    public class WorkspaceService : IWorkspaceService
    {
        private readonly NoteHubDbContext _context;

        public WorkspaceService(NoteHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Workspace CreateWorkspace(Workspace workspace)
        {
            // Ensure new workspaces are active
            workspace.IsActive = true;
            _context.Workspaces.Add(workspace);
            _context.SaveChanges();
            return workspace;
        }

        public Workspace? GetWorkspaceById(int workspaceId)
        {
            var workspace = _context.Workspaces
                .Include(w => w.Pages.Where(p => p.IsActive))
                .Include(w => w.User)
                .FirstOrDefault(w => w.WorkspaceId == workspaceId && w.IsActive);
            return workspace;
        }

        public IEnumerable<Workspace> GetWorkspacesByUserId(int userId)
        {
            return _context.Workspaces
                .Include(w => w.Pages.Where(p => p.IsActive))
                .Where(w => w.UserId == userId && w.IsActive)
                .OrderByDescending(w => w.CreatedAt)
                .ToList();
        }

        public Workspace UpdateWorkspace(Workspace workspace)
        {
            _context.Workspaces.Update(workspace);
            _context.SaveChanges();
            return workspace;
        }

        public void DeleteWorkspace(int workspaceId)
        {
            var workspace = _context.Workspaces.Find(workspaceId);
            if (workspace != null)
            {
                _context.Workspaces.Remove(workspace);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Workspace> SearchWorkspaces(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Workspace>();

            return _context.Workspaces
                .Include(w => w.Pages.Where(p => p.IsActive))
                .Where(w => w.IsActive && w.Name.Contains(searchTerm))
                .OrderByDescending(w => w.CreatedAt)
                .ToList();
        }
    }
}

