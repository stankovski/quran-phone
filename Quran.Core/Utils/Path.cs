using System;
using System.Globalization;
using System.Text;

namespace Quran.Core.Utils
{
    public static class PathHelper
    {
        private static readonly char[] InvalidPathChars = { '\x22', '\x3C', '\x3E', '\x7C', '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07',
					'\x08', '\x09', '\x0A', '\x0B', '\x0C', '\x0D', '\x0E', '\x0F', '\x10', '\x11', '\x12', 
					'\x13', '\x14', '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D', 
					'\x1E', '\x1F' };

        private static readonly char DirectorySeparatorChar = '/';
        private static readonly string DirectorySeparatorStr = DirectorySeparatorChar.ToString();
        private static readonly char AltDirectorySeparatorChar = '\\';
        private static readonly char[] PathSeparatorChars = { DirectorySeparatorChar, AltDirectorySeparatorChar };

        public static string Combine(params string[] paths)
        {
            if (paths == null)
                throw new ArgumentNullException("paths");

            bool need_sep;
            var ret = new StringBuilder();
            int pathsLen = paths.Length;
            int slen;
            foreach (var s in paths)
            {
                need_sep = false;
                if (s == null)
                    throw new ArgumentNullException("One of the paths contains a null value", "paths");
                if (s.IndexOfAny(InvalidPathChars) != -1)
                    throw new ArgumentException("Illegal characters in path.");

                pathsLen--;
                if (IsPathRooted(s))
                    ret.Length = 0;

                ret.Append(s);
                slen = s.Length;
                if (slen > 0 && pathsLen > 0)
                {
                    char p1end = s[slen - 1];
                    if (p1end != DirectorySeparatorChar && p1end != AltDirectorySeparatorChar)
                        need_sep = true;
                }

                if (need_sep)
                    ret.Append(DirectorySeparatorStr);
            }

            return ret.ToString();
        }

        public static bool IsPathRooted(string path)
        {
            if (path == null || path.Length == 0)
                return false;

            if (path.IndexOfAny(InvalidPathChars) != -1)
                throw new ArgumentException("Illegal characters in path.");

            char c = path[0];
            return (c == DirectorySeparatorChar ||
                c == AltDirectorySeparatorChar);
        }

        public static string GetDirectoryName(string path)
        {
            // LAMESPEC: For empty string MS docs say both
            // return null AND throw exception.  Seems .NET throws.
            if (path == String.Empty)
                throw new ArgumentException("Invalid path");

            if (path == null || GetPathRoot(path) == path)
                return null;

            if (path.Trim().Length == 0)
                throw new ArgumentException("Argument string consists of whitespace characters only.");

            if (path.IndexOfAny(PathHelper.InvalidPathChars) > -1)
                throw new ArgumentException("Path contains invalid characters");

            int nLast = path.LastIndexOfAny(PathSeparatorChars);
            if (nLast == 0)
                nLast++;

            if (nLast > 0)
            {
                string ret = path.Substring(0, nLast);
                int l = ret.Length;

                if (l >= 2 && DirectorySeparatorChar == '\\' && ret[l - 1] == DirectorySeparatorChar)
                    return ret + DirectorySeparatorChar;
                else if (l == 1 && DirectorySeparatorChar == '\\' && path.Length >= 2 && path[nLast] == DirectorySeparatorChar)
                    return ret + DirectorySeparatorChar;
                else
                {
                    //
                    // Important: do not use CanonicalizePath here, use
                    // the custom CleanPath here, as this should not
                    // return absolute paths
                    //
                    return CleanPath(ret);
                }
            }

            return String.Empty;
        }

        internal static string CleanPath(string s)
        {
            int l = s.Length;
            int sub = 0;
            int start = 0;

            // Host prefix?
            char s0 = s[0];
            if (l > 2 && s0 == '\\' && s[1] == '\\')
            {
                start = 2;
            }

            // We are only left with root
            if (l == 1 && (s0 == DirectorySeparatorChar || s0 == AltDirectorySeparatorChar))
                return s;

            // Cleanup
            for (int i = start; i < l; i++)
            {
                char c = s[i];

                if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar)
                    continue;
                if (i + 1 == l)
                    sub++;
                else
                {
                    c = s[i + 1];
                    if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar)
                        sub++;
                }
            }

            if (sub == 0)
                return s;

            char[] copy = new char[l - sub];
            if (start != 0)
            {
                copy[0] = '\\';
                copy[1] = '\\';
            }
            for (int i = start, j = start; i < l && j < copy.Length; i++)
            {
                char c = s[i];

                if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar)
                {
                    copy[j++] = c;
                    continue;
                }

                // For non-trailing cases.
                if (j + 1 != copy.Length)
                {
                    copy[j++] = DirectorySeparatorChar;
                    for (; i < l - 1; i++)
                    {
                        c = s[i + 1];
                        if (c != DirectorySeparatorChar && c != AltDirectorySeparatorChar)
                            break;
                    }
                }
            }
            return new String(copy);
        }

        public static string GetPathRoot(string path)
        {
            if (path == null)
                return null;

            if (path.Trim().Length == 0)
                throw new ArgumentException("The specified path is not of a legal form.");

            if (!IsPathRooted(path))
                return String.Empty;

            if (DirectorySeparatorChar == '/')
            {
                // UNIX
                return IsDsc(path[0]) ? DirectorySeparatorStr : String.Empty;
            }
            else
            {
                // Windows
                int len = 2;

                if (path.Length == 1 && IsDsc(path[0]))
                    return DirectorySeparatorStr;
                else if (path.Length < 2)
                    return String.Empty;

                if (IsDsc(path[0]) && IsDsc(path[1]))
                {
                    // UNC: \\server or \\server\share
                    // Get server
                    while (len < path.Length && !IsDsc(path[len])) len++;

                    // Get share
                    if (len < path.Length)
                    {
                        len++;
                        while (len < path.Length && !IsDsc(path[len])) len++;
                    }

                    return DirectorySeparatorStr +
                        DirectorySeparatorStr +
                        path.Substring(2, len - 2).Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
                }
                else if (IsDsc(path[0]))
                {
                    // path starts with '\' or '/'
                    return DirectorySeparatorStr;
                }
                else if (path[1] == DirectorySeparatorChar)
                {
                    // C:\folder
                    if (path.Length >= 3 && (IsDsc(path[2]))) len++;
                }
                else
                    return "C:";
                return path.Substring(0, len);
            }
        }

        static bool IsDsc(char c)
        {
            return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
        }

        public static string GetFileName(string path)
        {
            if (path == null || path.Length == 0)
                return path;

            if (path.IndexOfAny(InvalidPathChars) != -1)
                throw new ArgumentException("Illegal characters in path.");

            int nLast = path.LastIndexOfAny(PathSeparatorChars);
            if (nLast >= 0)
                return path.Substring(nLast + 1);

            return path;
        }
    }
}

