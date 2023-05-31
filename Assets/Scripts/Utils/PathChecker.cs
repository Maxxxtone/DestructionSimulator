using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathChecker : MonoBehaviour
{
    private void Start()
    {
        Test1();
        Test2();
    }
    private void Test1()
    {
        string fileName = "test.txt";
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        string result;
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            result = www.text;
            print("TEST 1 GOOD " + filePath);
            print("TEST TEXT " + result);
        }
        else
        {
            print("TEST FAILED " + filePath);
        }
    }
    private void Test2()
    {
        string fileName = "monu1.vox";
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        string result;
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            result = www.text;
            print("TEST 2 GOOD " + filePath);
        }
        else
        {
            print("TEST FAILED " + filePath);
        }
    }
}
