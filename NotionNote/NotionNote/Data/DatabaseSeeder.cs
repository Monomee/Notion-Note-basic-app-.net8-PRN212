using NotionNote.Models;
using System;
using System.Linq;

namespace NotionNote.Data
{
    public static class DatabaseSeeder
    {
        public static void Seed(NoteHubDbContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            var user = new User
            {
                Username = "demo",
                PasswordHash = "demo123",
                CreatedAt = DateTime.Now
            };
            context.Users.Add(user);
            context.SaveChanges();

            // Create sample workspaces
            var personalWorkspace = new Workspace
            {
                Name = "Personal",
                CreatedAt = DateTime.Now,
                UserId = user.UserId
            };

            var workWorkspace = new Workspace
            {
                Name = "Work",
                CreatedAt = DateTime.Now,
                UserId = user.UserId
            };

            context.Workspaces.AddRange(personalWorkspace, workWorkspace);
            context.SaveChanges();

            var welcomePage = new Page
            {
                Title = "Welcome to NotionNote",
                Content = "This is your first note! Start writing something amazing.",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsPinned = true,
                WorkspaceId = personalWorkspace.WorkspaceId
            };

            var meetingPage = new Page
            {
                Title = "Meeting Notes",
                Content = "Q1 2025 Planning Meeting\n\n- Review goals\n- Budget planning\n- Team updates",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsPinned = false,
                WorkspaceId = workWorkspace.WorkspaceId
            };

            var todoPage = new Page
            {
                Title = "TODO List",
                Content = "Things to do:\n- Buy groceries\n- Finish project\n- Call mom",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsPinned = false,
                WorkspaceId = personalWorkspace.WorkspaceId
            };

            context.Pages.AddRange(welcomePage, meetingPage, todoPage);
            context.SaveChanges();
        }
    }
}