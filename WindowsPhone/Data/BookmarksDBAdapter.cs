using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using QuranPhone.Common;
using QuranPhone.SQLite;
using QuranPhone.Utils;

namespace QuranPhone.Data
{
    public enum BoomarkSortOrder
    {
        DateAdded = 0,
        Location = 1,
        Alphabetical = 2
    }

    public class BookmarksDBAdapter : BaseDatabaseHandler
    {
        public const string DbName = "bookmarks.db";

        public BookmarksDBAdapter()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null)
            {
                return;
            }
            string path = Path.Combine(basePath, DbName);

            if (!QuranFileUtils.FileExists(path))
            {
                MDatabase = CreateDatabase(path);
            }
            else
            {
                MDatabase = new SQLiteDatabase(path);
            }
        }

        private SQLiteDatabase CreateDatabase(string path)
        {
            var newDb = new SQLiteDatabase(path);
            newDb.CreateTable<Bookmarks>();
            newDb.CreateTable<Tags>();
            newDb.CreateTable<BookmarkTags>();
            return newDb;
        }

        public List<Bookmarks> GetBookmarks(bool loadTags, BoomarkSortOrder sortOrder)
        {
            TableQuery<Bookmarks> bookmarks = MDatabase.Query<Bookmarks>();
            switch (sortOrder)
            {
                case BoomarkSortOrder.Location:
                    bookmarks = bookmarks.OrderBy(b => b.Page).OrderBy(b => b.Sura).OrderBy(b => b.Ayah);
                    break;
                default:
                    bookmarks = bookmarks.OrderByDescending(b => b.AddedDate);
                    break;
            }
            List<Bookmarks> result = bookmarks.ToList();
            if (loadTags)
            {
                Dictionary<int, Tags> tags = GetTags().ToDictionary(t => t.Id);
                List<BookmarkTags> bookmarkTags = GetBookmarkTags();
                foreach (BookmarkTags bt in bookmarkTags)
                {
                    Bookmarks bookmark = result.FirstOrDefault(b => b.Id == bt.BookmarkId);
                    Tags tag = tags[bt.TagId];
                    if (bookmark != null)
                    {
                        if (bookmark.Tags == null)
                        {
                            bookmark.Tags = new List<Tags>();
                        }
                        bookmark.Tags.Add(new Tags {Id = bt.TagId, Name = tag.Name});
                    }
                }
            }
            return result;
        }

        public List<int> GetBookmarkTagIds(int bookmarkId)
        {
            return
                MDatabase.Query<BookmarkTags>()
                    .Where(bt => bt.BookmarkId == bookmarkId)
                    .OrderBy(bt => bt.TagId)
                    .Select(bt => bt.TagId)
                    .ToList();
        }

        public List<BookmarkTags> GetBookmarkTags(int? bookmarkId = null)
        {
            TableQuery<BookmarkTags> query = MDatabase.Query<BookmarkTags>();
            if (bookmarkId != null)
            {
                query = query.Where(bt => bt.BookmarkId == bookmarkId);
            }
            return query.ToList();
        }

        public bool TogglePageBookmark(int page)
        {
            int bookmarkId = GetBookmarkId(null, null, page);
            if (bookmarkId < 0)
            {
                AddBookmark(page);
                return true;
            }
            RemoveBookmark(bookmarkId);
            return false;
        }

        public bool IsPageBookmarked(int page)
        {
            return GetBookmarkId(null, null, page) >= 0;
        }

        public int GetBookmarkId(int? sura, int? ayah, int page)
        {
            TableQuery<Bookmarks> bookmarks = MDatabase.Query<Bookmarks>().Where(b => b.Page == page);
            if (sura != null)
            {
                bookmarks = bookmarks.Where(b => b.Sura == (int) sura);
            }
            if (ayah != null)
            {
                bookmarks = bookmarks.Where(b => b.Ayah == (int) ayah);
            }
            List<Bookmarks> results = bookmarks.ToList();
            if (results.Count > 0)
            {
                return results[0].Id;
            }
            return -1;
        }

        public bool IsTagged(int bookmarkId)
        {
            return MDatabase.Query<BookmarkTags>().Where(bt => bt.BookmarkId == bookmarkId).Count() > 0;
        }

        public int AddBookmark(int page)
        {
            return AddBookmark(null, null, page);
        }

        public int AddBookmarkIfNotExists(int? sura, int? ayah, int page)
        {
            int bookmarkId = GetBookmarkId(sura, ayah, page);
            if (bookmarkId < 0)
            {
                bookmarkId = AddBookmark(sura, ayah, page);
            }
            return bookmarkId;
        }

        public int AddBookmark(int? sura, int? ayah, int page)
        {
            var bookmark = new Bookmarks {Ayah = ayah, Sura = sura, Page = page};
            MDatabase.Insert(bookmark);
            return bookmark.Id;
        }

        public bool RemoveBookmark(int bookmarkId)
        {
            ClearBookmarkTags(bookmarkId);
            return MDatabase.Delete(new Bookmarks {Id = bookmarkId}) == 1;
        }

        public List<Tags> GetTags()
        {
            return GetTags(BoomarkSortOrder.Alphabetical);
        }

        public List<Tags> GetTags(BoomarkSortOrder sortOrder)
        {
            TableQuery<Tags> tags = MDatabase.Query<Tags>();
            switch (sortOrder)
            {
                case BoomarkSortOrder.Location:
                    tags = tags.OrderBy(t => t.Name);
                    break;
                default:
                    tags = tags.OrderByDescending(t => t.AddedDate);
                    break;
            }
            return tags.ToList();
        }

        public int AddTag(string name)
        {
            Tags existingTag = MDatabase.Query<Tags>().FirstOrDefault(t => t.Name.ToLower() == name.ToLower());

            if (existingTag == null)
            {
                var tag = new Tags {Name = name};
                return MDatabase.Insert(tag);
            }
            return existingTag.Id;
        }

        public bool UpdateTag(int id, string newName)
        {
            var tag = new Tags {Id = id, Name = newName};

            return MDatabase.Update(tag) == 1;
        }

        public bool RemoveTag(int tagId)
        {
            bool removed = MDatabase.Delete(new Tags {Id = tagId}) == 1;
            if (removed)
            {
                MDatabase.Execute("delete from \"bookmark_tag\" where \"tag_id\" = ?", tagId);
            }

            return removed;
        }

        public int GetBookmarkTagId(int bookmarkId, int tagId)
        {
            TableQuery<int> result =
                MDatabase.Query<BookmarkTags>()
                    .Where(bt => bt.BookmarkId == bookmarkId && bt.TagId == tagId)
                    .Select(bt => bt.Id);

            if (result.Count() == 0)
            {
                return -1;
            }
            return result.First();
        }

        public void TagBookmarks(int[] bookmarkIds, List<Tags> tags)
        {
            MDatabase.BeginTransaction();
            try
            {
                foreach (Tags t in tags)
                {
                    if (t.Id < 0)
                    {
                        continue;
                    }

                    if (t.Checked)
                    {
                        for (int i = 0; i < bookmarkIds.Length; i++)
                        {
                            int btId = GetBookmarkTagId(bookmarkIds[i], t.Id);
                            if (btId == -1)
                            {
                                MDatabase.Insert(new BookmarkTags {BookmarkId = bookmarkIds[i], TagId = t.Id});
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bookmarkIds.Length; i++)
                        {
                            int btId = GetBookmarkTagId(bookmarkIds[i], t.Id);
                            if (btId != -1)
                            {
                                MDatabase.Delete(new BookmarkTags {Id = btId});
                            }
                        }
                    }
                }
                MDatabase.CommitTransaction();
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception in tagBookmark: " + e.Message);
                MDatabase.RollbackTransaction();
            }
        }

        public void TagBookmark(int bookmarkId, List<Tags> tags)
        {
            TagBookmarks(new[] {bookmarkId}, tags);
        }

        public void TagBookmark(int bookmarkId, int tagId)
        {
            Tags tag = MDatabase.Query<Tags>().FirstOrDefault(t => t.Id == tagId);
            if (tag != null)
            {
                tag.Checked = true;
                TagBookmarks(new[] {bookmarkId}, new List<Tags> {tag});
            }
        }

        public void UntagBookmark(int bookmarkId, int tagId)
        {
            MDatabase.Execute("delete \"bookmark_tag\" where \"bookmark_id\" = ? and \"tag_id\" = ?", bookmarkId, tagId);
        }

        public void ClearBookmarkTags(int bookmarkId)
        {
            MDatabase.Execute("delete from \"bookmark_tag\" where \"bookmark_id\" = ?", bookmarkId);
        }
    }
}