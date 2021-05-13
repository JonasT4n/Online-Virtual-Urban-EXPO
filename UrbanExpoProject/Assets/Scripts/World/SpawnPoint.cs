using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UrbanExpo
{
    public class SpawnPoint : Building, IEntitySpawner
    {
        public LivingEntity Spawn(LivingEntity entityPrefab)
        {
            Vector3 pos = transform.position;
            pos.z = 0f;
            LivingEntity entity = Instantiate(entityPrefab, pos, Quaternion.identity);
            return entity;
        }
    }

}