using QuranPhone.Common;
using QuranPhone.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuranPhone.Data
{
    public class AyahInfoDatabaseHandler
    {
        private SQLiteDatabase database = null;

        public AyahInfoDatabaseHandler(string databaseName)
        {
            string b = QuranFileUtils.GetQuranDatabaseDirectory();
            if (b == null) return;
            string path = b + Path.PathSeparator + databaseName;
            database = new SQLiteDatabase(path);
        }

        public bool validDatabase()
        {
            return (database == null) ? false : database.isOpen();
        }

        public IList<AyahBounds> getVerseBounds(int sura, int ayah)
        {
            if (!validDatabase()) return null;
            return database.query<AyahBounds>().Where(a=>a.SurahNumber == sura && a.AyahNumber == ayah).ToList();
        }

        public Rect? getPageBounds(int page)
        {
            if (!validDatabase()) return null;
            AyahLimits limits = database.query<AyahLimits>().Where(a => a.PageNumber == page).FirstOrDefault();
            if (limits == null) return null;
            Rect r = new Rect(limits.MinX, limits.MinY, limits.MaxX, limits.MaxY);
            return r;
        }

        //// TODO Improve efficiency -AF
        //public QuranAyah getVerseAtPoint(int page, float x, float y)
        //{
        //    if (!validDatabase())
        //        return null;
        //    Cursor cursor = database.query(GLYPHS_TABLE,
        //            new string[]{ COL_PAGE, COL_LINE, COL_SURA, COL_AYAH, COL_POSITION,
        //                        MIN_X, MIN_Y, MAX_X, MAX_Y },
        //            COL_PAGE + "=" + page,
        //            null, null, null, COL_LINE);
        //    if (!cursor.moveToFirst())
        //        return null;
        //    Dictionary<int, int[]> lines = new Dictionary<int, int[]>();
        //    do
        //    {
        //        int lineNumber = cursor.getInt(1);
        //        int minY = cursor.getInt(6);
        //        int maxY = cursor.getInt(8);
        //        int[] lineBounds = lines[lineNumber];
        //        if (lineBounds == null)
        //        {
        //            int[] bounds = { minY, maxY };
        //            lines[lineNumber] = bounds;
        //        }
        //        else
        //        {
        //            if (minY < lineBounds[0])
        //                lineBounds[0] = minY;
        //            if (maxY > lineBounds[1])
        //                lineBounds[1] = maxY;
        //        }
        //    } while (cursor.moveToNext());
        //    cursor.close();

        //    float? delta = null;
        //    int? closestNeighbor = null;
        //    foreach (var line in lines.Keys)
        //    {
        //        int[] bounds = lines[line];
        //        if (y >= bounds[0] && y <= bounds[1])
        //        {
        //            return getVerseAtPoint(page, line, x);
        //        }
        //        else if (y >= bounds[1])
        //        {
        //            // past this line
        //            if (delta == null)
        //            {
        //                delta = y - bounds[1];
        //                closestNeighbor = line;
        //            }
        //            else if ((y - bounds[1]) < delta)
        //            {
        //                delta = y - bounds[1];
        //                closestNeighbor = line;
        //            }
        //        }
        //        else if (bounds[0] >= y)
        //        {
        //            // before this line
        //            if (delta == null)
        //            {
        //                delta = bounds[0] - y;
        //                closestNeighbor = line;
        //            }
        //            else if ((bounds[0] - y) < delta)
        //            {
        //                delta = bounds[0] - y;
        //                closestNeighbor = line;
        //            }
        //        }
        //    }

        //    if (delta != null && closestNeighbor != null)
        //    {
        //        return getVerseAtPoint(page, (int)closestNeighbor, x);
        //    }
        //    return null;
        //}

        //public QuranAyah getVerseAtPoint(int page, int line, float x)
        //{
        //    if (!validDatabase() || line < 1 || line > 15)
        //        return null;
        //    Cursor cursor = database.query(GLYPHS_TABLE,
        //            new string[]{ COL_PAGE, COL_LINE, COL_SURA, COL_AYAH, COL_POSITION,
        //                        MIN_X, MIN_Y, MAX_X, MAX_Y },
        //            COL_PAGE + "=" + page + " and " + COL_LINE + "=" + line,
        //            null, null, null, COL_AYAH);
        //    if (!cursor.moveToFirst())
        //        return null;
        //    int suraNumber = cursor.getInt(2);
        //    Dictionary<int, int[]> ayahs = new Dictionary<int, int[]>();
        //    do
        //    {
        //        int ayahNumber = cursor.getInt(3);
        //        int minX = cursor.getInt(5);
        //        int maxX = cursor.getInt(7);
        //        int[] ayahBounds = ayahs[ayahNumber];
        //        if (ayahBounds == null)
        //        {
        //            int[] bounds = { minX, maxX };
        //            ayahs[ayahNumber] = bounds;
        //        }
        //        else
        //        {
        //            if (minX < ayahBounds[0])
        //                ayahBounds[0] = minX;
        //            if (maxX > ayahBounds[1])
        //                ayahBounds[1] = maxX;
        //        }
        //    } while (cursor.moveToNext());
        //    cursor.close();
        //    foreach (int ayah in ayahs.Keys)
        //    {
        //        int[] bounds = ayahs[ayah];
        //        if (x >= bounds[0] && x <= bounds[1])
        //            return new QuranAyah(suraNumber, ayah);
        //    }
        //    return null;
        //}

        public void closeDatabase()
        {
            if (database != null)
                database.close();
        }
    }
}
