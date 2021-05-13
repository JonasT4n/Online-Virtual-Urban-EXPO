using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public interface ISpawner<T>
    {
        T Spawn(T prefab);
    }

}