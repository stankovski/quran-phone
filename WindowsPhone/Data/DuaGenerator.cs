using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuranPhone.Resources;

namespace QuranPhone.Data
{
    public class DuaGenerator
    {
        private static int[][] quran_dua = new int[][]
            {
                new int[] {2, 201}, 
                new int[] {2, 250},
                new int[] {2, 286},
                new int[] {3, 8},
                new int[] {3, 147},
                new int[] {3, 192},
                new int[] {3, 193},
                new int[] {3, 194},
                new int[] {7, 23},
                new int[] {7, 47},
                new int[] {7, 89},
                new int[] {7, 126},
                new int[] {10, 85},
                new int[] {14, 38},
                new int[] {18, 10},
                new int[] {25, 74},
                new int[] {40, 7},
                new int[] {40, 8},
                new int[] {44, 12},
                new int[] {59, 10},
                new int[] {60, 4},
                new int[] {66, 8},
            };

        public static void Generate()
        {
            using (var adapter = new BookmarksDBAdapter())
            {
                var tagId = adapter.AddTag(AppResources.dua);
                foreach (int[] dua in quran_dua)
                {
                    var p = QuranInfo.GetPageFromSuraAyah(dua[0], dua[1]);
                    var id = adapter.AddBookmarkIfNotExists(dua[0], dua[1], p);
                    if (!adapter.IsTagged(id))
                        adapter.TagBookmark(id, tagId);
                }
            }
        }
    }
}
