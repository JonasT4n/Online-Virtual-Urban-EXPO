using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace UrbanExpo
{
    public static class BinaryDataSaver
    {
        public static LocalData LoadMainData()
        {
            return new LocalData();
        }

        public static void SaveMainData(LocalData data)
        {

        }
    }

    [Serializable]
    public class LocalData
    {
        public LocalData()
        {

        }
    }

}