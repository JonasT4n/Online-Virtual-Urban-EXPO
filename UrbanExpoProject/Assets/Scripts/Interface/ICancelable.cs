using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public interface ICancelable
    {
        bool IsCancelled { set; get; }
    }

}