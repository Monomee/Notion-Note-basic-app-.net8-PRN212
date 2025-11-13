using Microsoft.EntityFrameworkCore;
using NotionNote.Models;
using System.Collections.Generic;
using System.Linq;

namespace NotionNote.Services
{
    public class PageService : IPageService
    {
        private readonly NoteHubDbContext _context;

        public PageService(NoteHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Page CreatePage(Page page)
        {
            // Ensure new pages are active
            page.IsActive = true;
            _context.Pages.Add(page);
            _context.SaveChanges();
            return page;
        }

        public Page? GetPageById(int pageId)
        {
            return _context.Pages
                .Include(p => p.Workspace)
                .Include(p => p.Tags)
                .FirstOrDefault(p => p.PageId == pageId && p.IsActive);
        }

        public IEnumerable<Page> GetPagesByWorkspaceId(int workspaceId)
        {
            return _context.Pages
                .Include(p => p.Tags)
                .Where(p => p.WorkspaceId == workspaceId && p.IsActive)
                .OrderByDescending(p => p.IsPinned)  // Pinned lên đầu
                .ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .ToList();
        }

        public Page UpdatePage(Page page)
        {
            _context.Pages.Update(page);
            _context.SaveChanges();
            return page;
        }

        public void DeletePage(int pageId)
        {
            // Soft delete: set IsActive = false
            var page = _context.Pages.Find(pageId);
            if (page != null)
            {
                page.IsActive = false;
                _context.SaveChanges();
            }
        }

        public void HardDeletePage(int pageId)
        {
            // Permanent delete from database
            var page = _context.Pages.Find(pageId);
            if (page != null)
            {
                _context.Pages.Remove(page);
                _context.SaveChanges();
            }
        }

        public void RestorePage(int pageId)
        {
            // Restore deleted page
            var page = _context.Pages.Find(pageId);
            if (page != null)
            {
                page.IsActive = true;
                _context.SaveChanges();
            }
        }

        public IEnumerable<Page> GetDeletedPages(int userId)
        {
            // Get all deleted pages for a user (through workspaces)
            return _context.Pages
                .Include(p => p.Workspace)
                .Include(p => p.Tags)
                .Where(p => !p.IsActive && p.Workspace.UserId == userId)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .ToList();
        }

        public IEnumerable<Page> SearchPages(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Page>();

            return _context.Pages
                .Include(p => p.Tags)
                .Where(p => p.IsActive && 
                           (p.Title.Contains(searchTerm) || 
                           (p.Content != null && p.Content.Contains(searchTerm))))
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .ToList();
        }
    }
}
