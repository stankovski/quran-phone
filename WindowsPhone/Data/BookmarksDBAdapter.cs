using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public static string DB_NAME = "bookmarks.db";
        public static int SORT_DATE_ADDED = 0;
        public static int SORT_LOCATION = 1;
        public static int SORT_ALPHABETICAL = 2;

        public BookmarksDBAdapter()
        {
            string basePath = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (basePath == null) return;
            string path = basePath + QuranFileUtils.PATH_SEPARATOR + DB_NAME;

            if (!QuranFileUtils.FileExists(path))
                mDatabase = CreateDatabase(path);
            else
                mDatabase = new SQLiteDatabase(path);
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
            var bookmarks = mDatabase.Query<Bookmarks>();
            switch (sortOrder)
            {
                case BoomarkSortOrder.Location:
                    bookmarks = bookmarks.OrderBy(b => b.Page).ThenBy(b => b.Sura).ThenBy(b => b.Ayah);
                    break;
                default:
                    bookmarks = bookmarks.OrderByDescending(b => b.AddedDate);
                    break;
            }
            var result = bookmarks.ToList();
            if (loadTags)
            {
                foreach (var b in result)
                {
                    b.Tags = GetBookmarkTags(b.Id);
                }
            }
            return result;
        }

        public List<int> GetBookmarkTagIds(int bookmarkId)
        {
            return mDatabase.Query<BookmarkTags>().Where(bt => bt.BookmarkId == bookmarkId).
                OrderBy(bt => bt.TagId).Select(bt => bt.TagId).ToList();
        }

        public List<Tags> GetBookmarkTags(int bookmarkId)
        {
            var result = from tags in mDatabase.Query<Tags>()
                         join bookmarkTags in mDatabase.Query<BookmarkTags>()
                         on tags.Id equals bookmarkTags.TagId
                         orderby tags.Id
                         select tags;
            return result.ToList();
        }

        public bool TogglePageBookmark(int page)
        {
            int bookmarkId = GetBookmarkId(null, null, page);
            if (bookmarkId < 0)
            {
                AddBookmark(page);
                return true;
            }
            else
            {
                RemoveBookmark(bookmarkId);
                return false;
            }
        }

        public bool IsPageBookmarked(int page)
        {
            return GetBookmarkId(null, null, page) >= 0;
        }

        public int GetBookmarkId(int? sura, int? ayah, int page)
        {
            var bookmarks = mDatabase.Query<Bookmarks>().Where(b => b.Page == page);
            if (sura != null)
            {
                bookmarks = bookmarks.Where(b => b.Sura == (int)sura);
            }
            if (ayah != null)
            {
                bookmarks = bookmarks.Where(b => b.Ayah == (int)ayah);
            }
            var results = bookmarks.ToList();
            if (results.Count > 0)
            {
                return results[0].Id;
            }
            return -1;
        }

        public bool IsTagged(int bookmarkId)
        {
            return mDatabase.Query<BookmarkTags>().Where(bt => bt.BookmarkId == bookmarkId).Count() > 0;
        }

        public int AddBookmark(int page)
        {
            return AddBookmark(null, null, page);
        }

        public int AddBookmarkIfNotExists(int? sura, int? ayah, int page)
        {
            int bookmarkId = GetBookmarkId(sura, ayah, page);
            if (bookmarkId < 0)
                bookmarkId = AddBookmark(sura, ayah, page);
            return bookmarkId;
        }

        public int AddBookmark(int? sura, int? ayah, int page)
        {
            var bookmark = new Bookmarks { Ayah = ayah, Sura = sura, Page = page };
            return mDatabase.Insert(bookmark);
        }

        public bool RemoveBookmark(int bookmarkId)
        {
            ClearBookmarkTags(bookmarkId);
            return mDatabase.Delete(new Bookmarks { Id = bookmarkId }) == 1;
        }

        public List<Tags> GetTags()
        {
            return GetTags(BoomarkSortOrder.Alphabetical);
        }

        public List<Tags> GetTags(BoomarkSortOrder sortOrder)
        {
            var tags = mDatabase.Query<Tags>();
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
            Tags tag = new Tags { Name = name };
            return mDatabase.Insert(tag);
        }

        public bool UpdateTag(int id, string newName)
        {
            Tags tag = new Tags { Id = id, Name = newName };

            return mDatabase.Update(tag) == 1;
        }

        public bool RemoveTag(int tagId)
        {
            bool removed = mDatabase.Delete(new Tags { Id = tagId }) == 1;
            if (removed)
            {
                mDatabase.Execute("delete from \"bookmark_tag\" where \"tag_id\" = ?", tagId);
            }

            return removed;
        }

        public int GetBookmarkTagId(int bookmarkId, int tagId)
        {
            var result = mDatabase.Query<BookmarkTags>().Where(bt => bt.BookmarkId == bookmarkId && bt.TagId == tagId).Select(bt => bt.Id);

            if (result.Count() == 0)
                return -1;
            else
                return result.First();
        }

        public void TagBookmarks(int[] bookmarkIds, List<Tags> tags)
        {
            mDatabase.BeginTransaction();
            try
            {
                foreach (var t in tags)
                {
                    if (t.Id < 0)
                        continue;

                    if (t.Checked)
                    {
                        for (int i = 0; i < bookmarkIds.Length; i++)
                        {
                            var btId = GetBookmarkTagId(bookmarkIds[i], t.Id);
                            if (btId == -1)
                            {
                                mDatabase.Insert(new BookmarkTags { BookmarkId = bookmarkIds[i], TagId = t.Id });
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bookmarkIds.Length; i++)
                        {
                            var btId = GetBookmarkTagId(bookmarkIds[i], t.Id);
                            if (btId != -1)
                            {
                                mDatabase.Delete(new BookmarkTags { Id = btId });
                            }
                        }
                    }
                }
                mDatabase.CommitTransaction();
            }
            catch (Exception e)
            {
                Debug.WriteLine("exception in tagBookmark: " + e.Message);
                mDatabase.RollbackTransaction();
            }
        }

        public void TagBookmark(int bookmarkId, List<Tags> tags)
        {
            TagBookmarks(new int[] { bookmarkId }, tags);
        }

        public void TagBookmark(int bookmarkId, int tagId)
        {
            var tag = mDatabase.Query<Tags>().Where(t => t.Id == tagId).FirstOrDefault();
            if (tag != null)
            {
                var list = new List<Tags>();
                list.Add(tag);
                TagBookmarks(new int[] { bookmarkId }, list);
            }
        }

        public void UntagBookmark(int bookmarkId, int tagId)
        {
            mDatabase.Execute("delete \"bookmark_tag\" where \"bookmark_id\" = ? and \"tag_id\" = ?", bookmarkId, tagId);
        }

        public void ClearBookmarkTags(int bookmarkId)
        {
            mDatabase.Execute("delete \"bookmark_tag\" where \"bookmark_id\" = ?", bookmarkId);
        }
    }
}
