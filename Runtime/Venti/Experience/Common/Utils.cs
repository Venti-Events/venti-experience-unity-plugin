using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SimpleJSON;

namespace Venti.Experience
{
    public static class Utils
    {
        // Fetch all child fields of a parent object
        public static T[] FetchChildFields<T>(GameObject parent, bool searchForInactive = false)
        {
            List<T> directChildFields = new List<T>();

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                T field = child.GetComponent<T>();
                if (field == null) continue;

                directChildFields.Add(field);

                BaseField baseField = field as BaseField;
                if (baseField != null)
                    baseField.FetchChildFields(searchForInactive);
            }

            return directChildFields.ToArray();
        }

        public static void ClearChildFields<T>(T[] fields)
        {
            if (fields == null) return;

            foreach (var field in fields)
            {
                BaseField baseField = field as BaseField;
                if (baseField != null)
                    baseField.ClearFields();
            }
        }

        public static string GenerateRandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            System.Random random = new System.Random();
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }
}