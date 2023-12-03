// <copyright file="HistoricalStartSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace HistoricalStart
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Game;
    using Game.Prefabs;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// The historical start mod system.
    /// </summary>
    internal sealed partial class HistoricalStartSystem : GameSystemBase
    {
        // References.
        private ILog _log;
        private EntityQuery _lockedQuery;
        private PrefabSystem _prefabSystem;

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
            foreach (Entity entity in _lockedQuery.ToEntityArray(Allocator.Temp))
            {
                // Train depot.
                if (EntityManager.TryGetComponent(entity, out TransportDepotData transportDepotData))
                {
                    if (transportDepotData.m_TransportType == TransportType.Train)
                    {
                        _log.Debug("unlocking train depot");
                        Unlock(entity);
                    }
                }

                // Train tracks.
                else if (EntityManager.TryGetComponent(entity, out TrackData trackData))
                {
                    // Only train tracks.
                    if (trackData.m_TrackType == Game.Net.TrackTypes.Train)
                    {
                        _log.Debug("unlocking train track");
                        Unlock(entity);
                    }
                }

                // Ship paths.
                else if (EntityManager.HasComponent<WaterwayData>(entity))
                {
                    _log.Debug("unlocking waterway");
                    Unlock(entity);
                }

                // Cargo transport stations.
                else if (EntityManager.HasComponent<CargoTransportStationData>(entity) && EntityManager.TryGetComponent(entity, out TransportStationData transportStationData))
                {
                    // Exclude airports.
                    if (transportStationData.m_AircraftRefuelTypes == Game.Vehicles.EnergyTypes.None)
                    {
                        _log.Debug("unlocking cargo transport station");
                        Unlock(entity);
                    }
                }

                // Specialized industry.
                else if (EntityManager.TryGetComponent(entity, out PlaceholderBuildingData placeholderData) && placeholderData.m_Type == BuildingType.ExtractorBuilding)
                {
                    _log.Debug("unlocking extractor");
                    if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && _prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab)
                    {
                        _log.Debug("extractor prefab name is " + prefab.name);
                        Unlock(entity);
                    }
                }

                // Specifically named prefabs.
                else if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && _prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab)
                {
                    if (prefab.name.Equals("TransportationTrain") || prefab.name.Equals("TrainStation01") || prefab.name.Equals("ZonesExtractors") || prefab.name.Equals("TransportationWater") || prefab.name.Equals("WaterTransportationGroup")
                        || prefab.name.Equals("TransportationGroup") || prefab.name.Equals("Harbor01"))
                    {
                        _log.Debug("unlocking named prefab " + prefab.name);
                        Unlock(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            // Set log.
            _log = Mod.Instance.Log;
            _log.Info("OnCreate");

            // Get system references.
            _prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();

            // Set up query.
            _lockedQuery = GetEntityQuery(ComponentType.ReadWrite<Locked>());
            RequireForUpdate(_lockedQuery);
        }

        /// <summary>
        /// Unlocks the given entity.
        /// </summary>
        /// <param name="entity">Entity to unlock.</param>
        private void Unlock(Entity entity)
        {
            // Unlock entity and remove unlock requirements.
            EntityManager.RemoveComponent<UnlockRequirement>(entity);
            EntityManager.RemoveComponent<Locked>(entity);

            // Reduce XP gain.
            if (EntityManager.TryGetComponent(entity, out PlaceableObjectData placeableData))
            {
                _log.Debug("Reducing XP from PlaceableObjectData");
                placeableData.m_XPReward /= 10;
                EntityManager.SetComponentData(entity, placeableData);
            }

            if (EntityManager.TryGetComponent(entity, out ServiceUpgradeData serviceData))
            {
                _log.Debug("Reducing XP from ServiceUpgradeData");
                placeableData.m_XPReward /= 10;
                EntityManager.SetComponentData(entity, serviceData);
            }
        }
    }
}
