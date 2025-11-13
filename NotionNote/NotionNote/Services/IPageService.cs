using NotionNote.Models;
using System.Collections.Generic;

namespace NotionNote.Services
{
    public interface IPageService
    {
        Page CreatePage(Page page);
        Page? GetPageById(int pageId);
        IEnumerable<Page> GetPagesByWorkspaceId(int workspaceId);
        Page UpdatePage(Page page);
        void DeletePage(int pageId); // Soft delete (set IsActive = false)
        IEnumerable<Page> SearchPages(string searchTerm);
        IEnumerable<Page> GetDeletedPages(int userId);
        void HardDeletePage(int pageId); // Permanent delete from database
        void RestorePage(int pageId); // Restore deleted page
    }
}
