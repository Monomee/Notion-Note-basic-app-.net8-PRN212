using NotionNote.Models;
using System.Collections.Generic;

namespace NotionNote.Services
{
    public interface IWorkspaceService
    {
        Workspace CreateWorkspace(Workspace workspace);
        Workspace? GetWorkspaceById(int workspaceId);
        IEnumerable<Workspace> GetWorkspacesByUserId(int userId);
        Workspace UpdateWorkspace(Workspace workspace);
        void DeleteWorkspace(int workspaceId); // Soft delete (set IsActive = false, and all pages)
        IEnumerable<Workspace> SearchWorkspaces(string searchTerm);
        IEnumerable<Workspace> GetDeletedWorkspaces(int userId);
        void HardDeleteWorkspace(int workspaceId); // Permanent delete from database
        void RestoreWorkspace(int workspaceId); // Restore deleted workspace and its pages
    }
}

