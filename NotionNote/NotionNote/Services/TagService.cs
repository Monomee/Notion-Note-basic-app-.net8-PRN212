using Microsoft.EntityFrameworkCore;
using NotionNote.Models;
using System.Collections.Generic;
using System.Linq;

namespace NotionNote.Services
{
    public class TagService : ITagService
    {
        private readonly NoteHubDbContext _context;

        public TagService(NoteHubDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Tag CreateTag(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tag name cannot be empty", nameof(name));

            // Check if tag already exists
            var existingTag = GetTagByName(name);
            if (existingTag != null)
                return existingTag;

            var tag = new Tag
            {
                Name = name.Trim()
            };

            _context.Tags.Add(tag);
            _context.SaveChanges();
            return tag;
        }

        public Tag? GetTagById(int tagId)
        {
            return _context.Tags
                .Include(t => t.Pages.Where(p => p.IsActive))
                .FirstOrDefault(t => t.TagId == tagId);
        }

        public Tag? GetTagByName(string name)
        {
            return _context.Tags
                .FirstOrDefault(t => t.Name.ToLower() == name.ToLower().Trim());
        }

        public IEnumerable<Tag> GetAllTags()
        {
            return _context.Tags
                .Include(t => t.Pages.Where(p => p.IsActive))
                .OrderBy(t => t.Name)
                .ToList();
        }

        public Tag UpdateTag(Tag tag)
        {
            _context.Tags.Update(tag);
            _context.SaveChanges();
            return tag;
        }

        public void DeleteTag(int tagId)
        {
            var tag = _context.Tags
                .Include(t => t.Pages.Where(p => p.IsActive))
                .FirstOrDefault(t => t.TagId == tagId);
            
            if (tag != null)
            {
                // Remove all associations with active pages only
                tag.Pages.Clear();
                _context.Tags.Remove(tag);
                _context.SaveChanges();
            }
        }

        public void AddTagToPage(int pageId, int tagId)
        {
            // Only allow adding tags to active pages
            var page = _context.Pages
                .Include(p => p.Tags)
                .FirstOrDefault(p => p.PageId == pageId && p.IsActive);
            
            var tag = _context.Tags.Find(tagId);
            
            if (page != null && tag != null && !page.Tags.Contains(tag))
            {
                page.Tags.Add(tag);
                _context.SaveChanges();
            }
        }

        public void RemoveTagFromPage(int pageId, int tagId)
        {
            // Only allow removing tags from active pages
            var page = _context.Pages
                .Include(p => p.Tags)
                .FirstOrDefault(p => p.PageId == pageId && p.IsActive);
            
            var tag = _context.Tags.Find(tagId);
            
            if (page != null && tag != null && page.Tags.Contains(tag))
            {
                page.Tags.Remove(tag);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Tag> GetTagsByPageId(int pageId)
        {
            // Only get tags for active pages
            var page = _context.Pages
                .Include(p => p.Tags)
                .FirstOrDefault(p => p.PageId == pageId && p.IsActive);
            
            return page?.Tags ?? new List<Tag>();
        }

        public IEnumerable<Page> GetPagesByTagId(int tagId)
        {
            var tag = _context.Tags
                .Include(t => t.Pages.Where(p => p.IsActive))
                .FirstOrDefault(t => t.TagId == tagId);
            
            return tag?.Pages ?? new List<Page>();
        }
    }
}

