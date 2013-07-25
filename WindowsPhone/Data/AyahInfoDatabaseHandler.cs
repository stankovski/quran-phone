using System.Windows.Input;
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
    public class AyahInfoDatabaseHandler : BaseDatabaseHandler
    {
        public AyahInfoDatabaseHandler(string databaseName)
        {
            string b = QuranFileUtils.GetQuranDatabaseDirectory(false, true);
            if (b == null) return;
            string path = Path.Combine(b, databaseName);
            mDatabase = new SQLiteDatabase(path);
        }

        public IList<AyahBounds> GetVerseBounds(int sura, int ayah)
        {
            return mDatabase.Query<AyahBounds>().Where(a => a.SurahNumber == sura && a.AyahNumber == ayah).ToList();
        }

        public IList<AyahBounds> GetVerseBoundsCombined(int sura, int ayah)
        {
            var bounds = GetVerseBounds(sura, ayah);
            var lineCoords = new Dictionary<int, AyahBounds>();
            AyahBounds first = null, last = null, current = null;
            foreach(var bound in bounds)
            {
                current = bound;
                if (first == null) 
                    first = current;

                if (!lineCoords.ContainsKey(current.Line))
                {
                    lineCoords[current.Line] = current;
                }
                else
                {
                    lineCoords[current.Line].Engulf(current);
                }
            }

            if ((first != null) && (current != null) &&
                (first.Position != current.Position))
                last = current;

            return doHighlightAyah(first, last, lineCoords);
        }

        private IList<AyahBounds> doHighlightAyah(AyahBounds first, AyahBounds last, Dictionary<int, AyahBounds> lineCoordinates)
        {
            if (first == null) 
                return null;

            var rangesToDraw = new List<AyahBounds>();
            if (last == null)
            {
                rangesToDraw.Add(first);
            }
            else
            {
                if (first.Line == last.Line)
                {
                    first.Engulf(last);
                    rangesToDraw.Add(first);
                }
                else
                {
                    AyahBounds b = lineCoordinates[first.Line];
                    rangesToDraw.Add(b);

                    int currentLine = first.Line + 1;
                    int diff = last.Line - first.Line - 1;
                    for (int i = 0; i < diff; i++)
                    {
                        b = lineCoordinates[currentLine + i];
                        rangesToDraw.Add(b);
                    }

                    b = lineCoordinates[last.Line];
                    rangesToDraw.Add(b);
                }
            }
            return rangesToDraw;
        }

        public Rect? GetPageBounds(int page)
        {
            AyahLimits limits = mDatabase.Query<AyahLimits>().FirstOrDefault(a => a.PageNumber == page);
            if (limits == null) return null;
            var r = new Rect(limits.MinX, limits.MinY, limits.MaxX, limits.MaxY);
            return r;
        }

        public QuranAyah GetVerseAtPoint(int page, double x, double y)
        {
            var ayahBounds = mDatabase.Query<AyahBounds>().Where(a => a.PageNumber == page);
            if (!ayahBounds.Any()) return null;

            var lines = new Dictionary<int, int[]>();
            foreach (var bound in ayahBounds)
            {
                int lineNumber = bound.Line;
                int minY = bound.MinY;
                int maxY = bound.MaxY;
                if (!lines.ContainsKey(lineNumber))
                {
                    int[] bounds = { minY, maxY };
                    lines[lineNumber] = bounds;
                }
                else
                {
                    int[] lineBounds = lines[lineNumber];
                    if (minY < lineBounds[0])
                        lineBounds[0] = minY;
                    if (maxY > lineBounds[1])
                        lineBounds[1] = maxY;
                }
            }

            double? delta = null;
            int? closestNeighbor = null;
            foreach (var line in lines.Keys)
            {
                int[] bounds = lines[line];
                if (y >= bounds[0] && y <= bounds[1])
                {
                    return GetVerseAtLine(page, line, x);
                }
                else if (y >= bounds[1])
                {
                    // past this line
                    if (delta == null)
                    {
                        delta = y - bounds[1];
                        closestNeighbor = line;
                    }
                    else if ((y - bounds[1]) < delta)
                    {
                        delta = y - bounds[1];
                        closestNeighbor = line;
                    }
                }
                else if (bounds[0] >= y)
                {
                    // before this line
                    if (delta == null)
                    {
                        delta = bounds[0] - y;
                        closestNeighbor = line;
                    }
                    else if ((bounds[0] - y) < delta)
                    {
                        delta = bounds[0] - y;
                        closestNeighbor = line;
                    }
                }
            }

            if (delta != null && closestNeighbor != null)
            {
                return GetVerseAtLine(page, (int)closestNeighbor, x);
            }
            return null;
        }

        public QuranAyah GetVerseAtLine(int page, int line, double x)
        {
            var allAyahBounds = mDatabase.Query<AyahBounds>().Where(a => a.PageNumber == page && a.Line == line);
            if (!allAyahBounds.Any()) return null;

            int suraNumber = allAyahBounds.First().SurahNumber;
            var ayahs = new Dictionary<int, int[]>();
		    foreach(var bound in allAyahBounds)
		    {
		        int ayahNumber = bound.AyahNumber;
			    int minX = bound.MinX;
			    int maxX = bound.MaxX;
                if (!ayahs.ContainsKey(ayahNumber)) 
                {
				    var bounds = new[]{minX, maxX};
				    ayahs[ayahNumber] = bounds;
			    } 
                else 
                {
                    int[] ayahBounds = ayahs[ayahNumber];
				    if (minX < ayahBounds[0])
					    ayahBounds[0] = minX;
				    if (maxX > ayahBounds[1])
					    ayahBounds[1] = maxX;
			    }
		    }

		    foreach (var ayahNumber in ayahs.Keys) 
            {
			    int[] bounds = ayahs[ayahNumber];
			    if (x >= bounds[0] && x <= bounds[1])
				    return new QuranAyah(suraNumber, ayahNumber);
		    }
		    return null;
        }
    }
}
