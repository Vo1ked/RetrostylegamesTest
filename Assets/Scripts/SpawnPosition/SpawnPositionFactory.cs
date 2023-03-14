using UnityEngine;
using System.Collections.Generic;
using Zenject;
using RetroStyleGamesTest.SpawnPosition.Implementation;

namespace RetroStyleGamesTest.SpawnPosition
{
    public class SpawnPositionFactory
    {
        private readonly List<ISpawnType> _spawnTypes = new List<ISpawnType>()
    {
        new FarthestSpawnPositionFromEnemies(),
        new SpawnByRandomRadius()
    };
        private bool _inited;

        private DiContainer _diContainer;
        [Inject]
        private void Construct(DiContainer diContainer)
        {
            _diContainer = diContainer;
        }

        public Vector3 GetSpawnPosition(SpawnType type)
        {
            TryInit();
            if (_spawnTypes.Exists(x => x.type == type))
            {
                return _spawnTypes.Find(x => x.type == type).GetSpawnPosition();
            }
            else
            {
                throw new System.ArgumentException($"Type {type} not injected!");
            }
        }

        private void TryInit()
        {
            if (_inited)
                return;

            _spawnTypes.ForEach(x => _diContainer.Inject(x));
            _inited = true;
        }
    }

    public enum SpawnType
    {
        random,
        farthestSpawnFromEnemies
    }
}