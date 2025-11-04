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
        void DeleteWorkspace(int workspaceId);
        IEnumerable<Workspace> SearchWorkspaces(string searchTerm);
    }
}

