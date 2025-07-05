using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Venti.Experience
{
    public static class Utils
    {
        // Fetch all child of given type of a parent object
        public static T[] FetchChildFields<T>(GameObject parent, bool searchForInactive = false)
        {
            List<T> directChildFields = new List<T>();

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                if (!searchForInactive && !child.gameObject.activeSelf)
                    continue;

                T field = child.GetComponent<T>();
                if (field == null) continue;

                directChildFields.Add(field);

                BaseField baseField = field as BaseField;
                if (baseField != null)
                    baseField.FetchChildFields(searchForInactive);
            }

            return directChildFields.ToArray();
        }

        public static void ClearChildFields(BaseField[] fields)
        {
            if (fields == null) return;

            foreach (var field in fields)
            {
                ////BaseField baseField = field as BaseField;
                //if (baseField != null)
                //    baseField.Clear();
                if (field != null)
                    field.Clear();
            }
        }

        public static BasePage[] FetchChildPages(GameObject parent, bool searchForInactive = false)
        {
            List<BasePage> directChildPages = new List<BasePage>();

            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                if (!searchForInactive && !child.gameObject.activeSelf)
                    continue;

                BasePage page = child.GetComponent<BasePage>();
                if (page == null) continue;

                directChildPages.Add(page);

                page.FetchChildren(searchForInactive);
            }

            return directChildPages.ToArray();
        }

        public static void ClearChildPages(BasePage[] pages)
        {
            if (pages == null) return;

            foreach (var page in pages)
            {
                if (page != null)
                    page.Clear();
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

        public static T[] JoinArrays<T>(T[] array1, T[] array2)
        {
            if (array1 == null || array2 == null)
                return null;

            T[] joinedArray = new T[array1.Length + array2.Length];
            Array.Copy(array1, 0, joinedArray, 0, array1.Length);
            Array.Copy(array2, 0, joinedArray, array1.Length, array2.Length);

            return joinedArray;
        }
    }
}