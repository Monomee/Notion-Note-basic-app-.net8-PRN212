using NotionNote.Models;
using System.Collections.Generic;

namespace NotionNote.Services
{
    public interface ITagService
    {
        Tag CreateTag(string name);
        Tag? GetTagById(int tagId);
        Tag? GetTagByName(string name);
        IEnumerable<Tag> GetAllTags();
        Tag UpdateTag(Tag tag);
        void DeleteTag(int tagId);
        void AddTagToPage(int pageId, int tagId);
        void RemoveTagFromPage(int pageId, int tagId);
        IEnumerable<Tag> GetTagsByPageId(int pageId);
        IEnumerable<Page> GetPagesByTagId(int tagId);
    }
}

