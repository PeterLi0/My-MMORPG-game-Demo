using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace MyCustom
{
    public class CSharpTest
    {
        public static void ShowMessage()
        {
            Debug.Log("Hello");
        }

        public static void ShowMessage(int a)
        {
            Debug.Log(a);
        }

        public void Show()
        {
            Debug.Log("aaaaaaaa");
        }
    }
}