using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using Shift25.Managers;
using System.Collections.Generic;

namespace Shift25.Gameplay
{
    public class NPCSpawner : MonoBehaviour
    {
        [SerializeField] private NPCController npcPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform exitPoint;
        [SerializeField] private List<Transform> browsingPoints;

        private IObjectPool<NPCController> _npcPool;
        private int _activeNPCCount = 0; // [Logic] นับคนในร้าน

        private void Awake()
        {
            _npcPool = new ObjectPool<NPCController>(
                createFunc: () => Instantiate(npcPrefab),
                actionOnGet: (npc) => { npc.gameObject.SetActive(true); _activeNPCCount++; },
                actionOnRelease: (npc) => { npc.gameObject.SetActive(false); _activeNPCCount--; },
                actionOnDestroy: (npc) => Destroy(npc.gameObject)
            );
        }

        private void Start() => SpawningLoop().Forget();

        private async UniTaskVoid SpawningLoop()
        {
            await UniTask.Yield();
            while (true)
            {
                var phase = GamePhaseManager.Instance.CurrentPhase;
                // [Logic] ถ้าในร้านเต็มตามกะปัจจุบัน ให้หยุดเกิดลูกค้าใหม่
                if (_activeNPCCount >= phase.maxNPCInStore)
                {
                    await UniTask.Delay(1000);
                    continue;
                }

                float delay = Random.Range(phase.minSpawnInterval, phase.maxSpawnInterval);
                await UniTask.Delay((int)(delay * 1000));

                NPCController npc = _npcPool.Get();
                npc.transform.position = spawnPoint.position;
                npc.Initialize(exitPoint, _npcPool, browsingPoints);
            }
        }
    }
}