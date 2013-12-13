using System.Text;

namespace Quran.Core.Utils
{
    /**
    * @author Rehab Mohammed
    * Idea of the ARABIC_GLYPHS matrix by Ahmed Essam
    */

    public class ArabicReshaper
    {

        private const int NON_ARABIC = 0, ISOLATED = 1, BEGINING = 2, MIDDLE = 3, END = 4,
                ARABIC2FORM = 5, LAMALEF = 6;
        private int preState = NON_ARABIC, loc = 0, lamState = 0, alef = 0;
        char curntChar, nxtChar;
        bool lam = true, append = false;

        private int[][] ARABIC_GLYPHS = {
			new [] { 1570, 65153, 65153, 65154, 65154, 2 },
			new [] { 1571, 65155, 65155, 65156, 65156, 2 },
			new [] { 1572, 65157, 65157, 65158, 65158, 2 },
			new [] { 1573, 65159, 65159, 65160, 65160, 2 },
			new [] { 1574, 65161, 65163, 65164, 65162, 4 },
			new [] { 1575, 65165, 65165, 65166, 65166, 2 },
			new [] { 1576, 65167, 65169, 65170, 65168, 4 },
			new [] { 1577, 65171, 65171, 65172, 65172, 2 },
			new [] { 1578, 65173, 65175, 65176, 65174, 4 },
			new [] { 1579, 65177, 65179, 65180, 65178, 4 },
			new [] { 1580, 65181, 65183, 65184, 65182, 4 },
			new [] { 1581, 65185, 65187, 65188, 65186, 4 },
			new [] { 1582, 65189, 65191, 65192, 65190, 4 },
			new [] { 1583, 65193, 65193, 65194, 65194, 2 },
			new [] { 1584, 65195, 65195, 65196, 65196, 2 },
			new [] { 1585, 65197, 65197, 65198, 65198, 2 },
			new [] { 1586, 65199, 65199, 65200, 65200, 2 },
			new [] { 1587, 65201, 65203, 65204, 65202, 4 },
			new [] { 1588, 65205, 65207, 65208, 65206, 4 },
			new [] { 1589, 65209, 65211, 65212, 65210, 4 },
			new [] { 1590, 65213, 65215, 65216, 65214, 4 },
			new [] { 1591, 65217, 65219, 65218, 65220, 4 },
			new [] { 1592, 65221, 65223, 65222, 65222, 4 },
			new [] { 1593, 65225, 65227, 65228, 65226, 4 },
			new [] { 1594, 65229, 65231, 65232, 65230, 4 },
			new [] { 1601, 65233, 65235, 65236, 65234, 4 },
			new [] { 1602, 65237, 65239, 65240, 65238, 4 },
			new [] { 1603, 65241, 65243, 65244, 65242, 4 },
			new [] { 1604, 65245, 65247, 65248, 65246, 4 },
			new [] { 1605, 65249, 65251, 65252, 65250, 4 },
			new [] { 1606, 65253, 65255, 65256, 65254, 4 },
			new [] { 1607, 65257, 65259, 65260, 65258, 4 },
			new [] { 1608, 65261, 65261, 65262, 65262, 2 },
			new [] { 1609, 65263, 65265, 65266, 65264, 4 },
			new [] { 1610, 65265, 65267, 65268, 65266, 4 },
			new [] { 1600, 1600, 1600, 1600, 1600, 4 } };

        private int[][] LAM_ALEF = {
			new [] { 65269, 65270 },
			new [] { 65271, 65272 },
			new [] { 65273, 65274 },
			new [] { 65275, 65276 } };

        public string Reshape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }
            text += "  ";
            int state = 0, tashkeelCount = 0;
            int count = text.Length;
            char[] states = new char[count];
            bool[] appends = new bool[count];
            bool[] tashkeel = new bool[count];
            char[] reshapedTxt = new char[text.Length];
            reshapedTxt = text.ToCharArray();
            for (int i = 0; i < count; i++)
            {
                if ((reshapedTxt[i] > 1610 && reshapedTxt[i] < 1619)
                        || reshapedTxt[i] == 1648
                        || (reshapedTxt[i] > 1749 && reshapedTxt[i] < 1755)
                        || (reshapedTxt[i] > 1759 && reshapedTxt[i] < 1763))
                {
                    // the char is tashkeel char
                    tashkeel[i] = true;
                    tashkeelCount++;
                }
                else
                {
                    // the char is not tashkeel char
                    tashkeel[i] = false;
                }
            }
            int curntIndex = 0, nxtIndex = 0;
            for (int i = 0; i < count - tashkeelCount - 1; i++)
            {
                if (!tashkeel[curntIndex])
                {
                    curntChar = text[curntIndex];
                }
                else
                {
                    states[curntIndex] = (char)state;
                    appends[curntIndex] = true;
                    curntIndex++;
                    nxtIndex++;
                    i--;
                    continue;
                }
                if (!tashkeel[curntIndex + 1])
                {
                    nxtChar = reshapedTxt[curntIndex + 1];
                }
                else
                {
                    while (tashkeel[nxtIndex + 1])
                    {
                        nxtIndex++;
                    }
                    nxtChar = reshapedTxt[nxtIndex + 1];
                    nxtIndex = curntIndex;
                }
                state = getCurrentState(curntChar, nxtChar);
                states[curntIndex] = (char)state;

                if (!append)
                {
                    appends[curntIndex] = true;
                    reshapedTxt[curntIndex] = getReshapedChar(state);

                }
                else
                {
                    appends[curntIndex] = false;
                    append = false;
                }
                curntIndex++;
                nxtIndex++;
            }
            if ((count) > 0)
            {
                curntChar = reshapedTxt[count - 1];
                state = getCurrentState(curntChar, ' ');
                if (!append)
                {
                    appends[curntIndex] = true;
                    reshapedTxt[curntIndex] = getReshapedChar(state);
                    curntIndex++;
                }
            }

            var finalText = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                if (appends[i])
                {

                    finalText.Append(reshapedTxt[i]);
                }
            }
            return finalText.ToString().Trim();

        }

        private int getCurrentState(char current, char next)
        {
            int state = 0, curnt = 0, nxt = 0;
            curnt = isArabicChar(current);
            nxt = isArabicChar(next);
            loc = curnt;
            switch (preState)
            {
                case NON_ARABIC:
                    if (curnt == -1)
                        state = NON_ARABIC;
                    else if (curnt == 28
                            && (nxt == 0 || nxt == 1 || nxt == 3 || nxt == 5))
                        state = LAMALEF;
                    else if (curnt != -1 && nxt == -1)
                        state = ISOLATED;
                    else if (curnt != -1 && nxt != -1)
                    {
                        if (ARABIC_GLYPHS[curnt][5] == 2)
                            state = ARABIC2FORM;
                        else
                            state = BEGINING;
                    }
                    lamState = BEGINING;
                    break;
                case ISOLATED:
                    if (curnt == -1)
                        state = NON_ARABIC;
                    break;
                case BEGINING:
                    if (curnt == 28 && (nxt == 0 || nxt == 1 || nxt == 3))
                        state = LAMALEF;
                    else if (curnt != -1)
                    {
                        if (ARABIC_GLYPHS[curnt][5] == 2)
                            state = END;
                        else
                        {
                            if (nxt != -1)
                                state = MIDDLE;
                            else if (nxt == -1)
                                state = END;
                        }
                    }
                    lamState = END;
                    break;
                case MIDDLE:
                    if (curnt == 28 && (nxt == 0 || nxt == 1 || nxt == 3 || nxt == 5))
                        state = LAMALEF;
                    else if (curnt != -1)
                    {
                        if (ARABIC_GLYPHS[curnt][5] == 2)
                            state = END;
                        else
                        {
                            if (nxt != -1)
                                state = MIDDLE;
                            else if (nxt == -1)
                                state = END;
                        }
                    }
                    lamState = END;
                    break;
                case END:
                    if (curnt == -1)
                        state = NON_ARABIC;
                    else if (curnt == 28
                            && (nxt == 0 || nxt == 1 || nxt == 3 || nxt == 5))
                        state = LAMALEF;
                    else if (curnt != -1 && nxt == -1)
                        state = ISOLATED;
                    else if (curnt != -1 && nxt != -1)
                    {
                        if (ARABIC_GLYPHS[curnt][5] == 2)
                            state = ARABIC2FORM;
                        else
                            state = BEGINING;
                    }
                    lamState = BEGINING;
                    break;
                case ARABIC2FORM:
                    if (curnt == -1)
                        state = NON_ARABIC;
                    else if (curnt == 28
                            && (nxt == 0 || nxt == 1 || nxt == 3 || nxt == 5))
                        state = LAMALEF;
                    else if (curnt != -1)
                    {
                        if (ARABIC_GLYPHS[curnt][5] == 2)
                            state = ARABIC2FORM;
                        else
                        {
                            if (nxt == -1)
                                state = ISOLATED;
                            else
                                state = BEGINING;
                        }
                    }
                    lamState = BEGINING;
                    break;
                case LAMALEF:
                    if (lam)
                    {
                        lam = false;
                    }
                    else
                    {
                        if (curnt == -1)
                            state = NON_ARABIC;
                        else if (curnt == 28
                                && (nxt == 0 || nxt == 1 || nxt == 3 || nxt == 5))
                            state = LAMALEF;
                        else if (curnt != -1 && nxt == -1)
                            state = ISOLATED;
                        else if (curnt != -1 && nxt != -1)
                        {
                            if (ARABIC_GLYPHS[curnt][5] == 2)
                                state = ARABIC2FORM;
                            else
                                state = BEGINING;
                        }
                        lam = true;
                    }
                    lamState = BEGINING;
                    break;

            }
            preState = state;
            alef = nxt;
            return state;

        }

        private int isArabicChar(char target)
        {
            for (int i = 0; i < 36; i++)
            {
                if (target == ARABIC_GLYPHS[i][0])
                {
                    return i;
                }
            }
            return -1;
        }

        private char getReshapedChar(int state)
        {
            int reshaped = 0;
            switch (state)
            {
                case NON_ARABIC:
                    reshaped = curntChar;
                    break;
                case ISOLATED:
                    reshaped = ARABIC_GLYPHS[loc][1];
                    break;
                case BEGINING:
                    reshaped = ARABIC_GLYPHS[loc][2];
                    break;
                case MIDDLE:
                    reshaped = ARABIC_GLYPHS[loc][3];
                    break;
                case END:
                    reshaped = ARABIC_GLYPHS[loc][4];
                    break;
                case ARABIC2FORM:
                    reshaped = ARABIC_GLYPHS[loc][1];
                    break;
                case LAMALEF:
                    append = true;
                    if (lamState == BEGINING)
                    {
                        if (alef == 0)
                            reshaped = LAM_ALEF[0][0];
                        else if (alef == 1)
                            reshaped = LAM_ALEF[1][0];
                        else if (alef == 3)
                            reshaped = LAM_ALEF[2][0];
                        else if (alef == 5)
                            reshaped = LAM_ALEF[3][0];
                    }
                    else
                    {
                        if (alef == 0)
                            reshaped = LAM_ALEF[0][1];
                        else if (alef == 1)
                            reshaped = LAM_ALEF[1][1];
                        else if (alef == 3)
                            reshaped = LAM_ALEF[2][1];
                        else if (alef == 5)
                            reshaped = LAM_ALEF[3][1];
                    }
                    break;
            }
            return (char)reshaped;
        }
    }
}
