using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class Text
{
    public static string FirstLine(string text)
    {
        using (StringReader reader = new StringReader(text))
        {
            string firstLine = reader.ReadLine();
            return firstLine;
        }
    }
}
