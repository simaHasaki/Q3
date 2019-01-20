﻿using GameFramework;
using GameFramework.DataTable;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GamePlay
{
    public class SangongGame : GameBase
    {
        private float m_ElapseSeconds = 0f;

        public override GameMode GameMode
        {
            get
            {
                return GameMode.Sangong;
            }
        }

        public override void Initialize(GameMode fromMode)
        {
            base.Initialize(fromMode);

        }

        public override void Shutdown()
        {
            base.Shutdown();
            NetWorkManager.Instance.CloseChanel();
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            base.Update(elapseSeconds, realElapseSeconds);

            m_ElapseSeconds += elapseSeconds;
         /*   if (m_ElapseSeconds >= 1f)
            {
                m_ElapseSeconds = 0f;
                IDataTable<DRAsteroid> dtAsteroid = GameEntry.DataTable.GetDataTable<DRAsteroid>();
                float randomPositionX = SceneBackground.EnemySpawnBoundary.bounds.min.x + SceneBackground.EnemySpawnBoundary.bounds.size.x * (float)Utility.Random.GetRandomDouble();
                float randomPositionZ = SceneBackground.EnemySpawnBoundary.bounds.min.z + SceneBackground.EnemySpawnBoundary.bounds.size.z * (float)Utility.Random.GetRandomDouble();
                GameEntry.Entity.ShowAsteroid(new AsteroidData(GameEntry.Entity.GenerateSerialId(), 60000 + Utility.Random.GetRandom(dtAsteroid.Count))
                {
                    Position = new Vector3(randomPositionX, 0f, randomPositionZ),
                });
            }*/
        }
    }
}
