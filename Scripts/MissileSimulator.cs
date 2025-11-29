using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MissileSimulatorManager : MonoBehaviour
{
    [Header("Simulation Data")]
    [SerializeField] private List<Missile> missiles = new List<Missile>();
    [SerializeField] private List<Target> targets = new List<Target>();
    
    [Header("Simulation Settings")]
    [SerializeField] private bool autoAssignTargets = true;
    [SerializeField] private bool pauseSimulation = false;
    [SerializeField] private float timeScale = 1f;
    [SerializeField] private bool spawnOnHit = true;
    
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnAreaMin = new Vector3(-40, 5, -40);
    [SerializeField] private Vector3 spawnAreaMax = new Vector3(40, 5, 40);
    [SerializeField] private float minSpawnDistance = 20f;
    
    [Header("Path Visualization")]
    [SerializeField] private bool showPaths = true;
    [SerializeField] private Material pathMaterial;
    [SerializeField] private float missilePathWidth = 0.1f;
    [SerializeField] private float targetPathWidth = 0.05f;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool showStats = true;
    
    private int totalHits = 0;
    private Transform pathContainer;

    private void Start()
    {
        InitializeSimulation();
        CreatePathContainer();
    }

    private void CreatePathContainer()
    {
        GameObject containerObj = new GameObject("PathContainer");
        containerObj.transform.SetParent(transform);
        pathContainer = containerObj.transform;
    }

    private void InitializeSimulation()
    {
        // Initialize all missiles
        foreach (var missile in missiles)
        {
            missile.Initialize(missile.position, missile.velocity);
            CreatePathRendererForMissile(missile);
        }

        // Initialize all targets
        foreach (var target in targets)
        {
            target.Initialize(target.position, target.centerPoint);
            CreatePathRendererForTarget(target);
        }

        // Auto-assign targets if enabled
        if (autoAssignTargets)
        {
            AssignClosestTargets();
        }
    }

    private void Update()
    {
        if (pauseSimulation)
            return;

        float dt = Time.deltaTime * timeScale;

        // Update all targets
        foreach (var target in targets)
        {
            target.UpdateTarget(dt);
        }

        // Track missiles that hit targets this frame
        List<Missile> hitMissiles = new List<Missile>();

        // Update all missiles and check for hits
        foreach (var missile in missiles)
        {
            bool wasActive = missile.isActive;
            missile.UpdateMissile(dt);
            
            // Check if missile just became inactive (hit target)
            if (wasActive && !missile.isActive)
            {
                hitMissiles.Add(missile);
            }
        }

        // Spawn new pairs for each hit
        if (spawnOnHit)
        {
            foreach (var hitMissile in hitMissiles)
            {
                SpawnNewPair();
                totalHits++;
            }
        }
    }

    private void AssignClosestTargets()
    {
        foreach (var missile in missiles)
        {
            if (!missile.isActive)
                continue;

            Target closestTarget = FindClosestTarget(missile.position);
            if (closestTarget != null)
            {
                missile.LockOntoTarget(closestTarget);
                Debug.Log($"{missile.name} locked onto {closestTarget.name}");
            }
        }
    }

    private Target FindClosestTarget(Vector3 position)
    {
        Target closest = null;
        float minDistance = float.MaxValue;

        foreach (var target in targets)
        {
            if (!target.isActive)
                continue;

            float distance = Vector3.Distance(position, target.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = target;
            }
        }

        return closest;
    }

    private void SpawnNewPair()
    {
        // Generate random positions ensuring minimum distance
        Vector3 missilePos = GetRandomSpawnPosition();
        Vector3 targetPos = GetRandomSpawnPosition();
        
        // Ensure missile and target are far enough apart
        while (Vector3.Distance(missilePos, targetPos) < minSpawnDistance)
        {
            targetPos = GetRandomSpawnPosition();
        }

        // Create new missile
        Missile newMissile = new Missile
        {
            name = $"Missile {missiles.Count + 1}",
            position = missilePos,
            velocity = (targetPos - missilePos).normalized,
            speed = Random.Range(40f, 60f),
            navigationGain = Random.Range(2.5f, 4f),
            gizmoColor = new Color(Random.Range(0.5f, 1f), Random.Range(0f, 0.5f), Random.Range(0f, 0.5f))
        };
        newMissile.Initialize(missilePos, newMissile.velocity);
        CreatePathRendererForMissile(newMissile);

        // Create new target
        Target.MovementType[] movementTypes = { 
            Target.MovementType.Circle, 
            Target.MovementType.Figure8, 
            Target.MovementType.Straight 
        };
        
        Target newTarget = new Target
        {
            name = $"Target {targets.Count + 1}",
            position = targetPos,
            centerPoint = targetPos,
            radius = Random.Range(10f, 20f),
            orbitSpeed = Random.Range(8f, 15f),
            movementType = movementTypes[Random.Range(0, movementTypes.Length)],
            straightDirection = Random.onUnitSphere,
            gizmoColor = new Color(Random.Range(0f, 0.5f), Random.Range(0.5f, 1f), Random.Range(0f, 0.5f))
        };
        newTarget.Initialize(targetPos, targetPos);
        CreatePathRendererForTarget(newTarget);

        // Add to lists
        missiles.Add(newMissile);
        targets.Add(newTarget);

        // Lock missile onto target
        newMissile.LockOntoTarget(newTarget);

        Debug.Log($"Spawned new pair: {newMissile.name} -> {newTarget.name}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        return new Vector3(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y),
            Random.Range(spawnAreaMin.z, spawnAreaMax.z)
        );
    }

    private void CreatePathRendererForMissile(Missile missile)
    {
        if (!showPaths || pathContainer == null)
            return;

        GameObject pathObj = new GameObject($"{missile.name}_Path");
        pathObj.transform.SetParent(pathContainer);
        
        LineRenderer lr = pathObj.AddComponent<LineRenderer>();
        lr.material = pathMaterial != null ? pathMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = missile.gizmoColor;
        lr.endColor = new Color(missile.gizmoColor.r, missile.gizmoColor.g, missile.gizmoColor.b, 0.3f);
        lr.startWidth = missilePathWidth;
        lr.endWidth = missilePathWidth * 0.5f;
        lr.positionCount = 1;
        lr.SetPosition(0, missile.position);
        lr.useWorldSpace = true;
        lr.numCapVertices = 2;
        
        missile.pathRenderer = lr;
    }

    private void CreatePathRendererForTarget(Target target)
    {
        if (!showPaths || pathContainer == null)
            return;

        GameObject pathObj = new GameObject($"{target.name}_Path");
        pathObj.transform.SetParent(pathContainer);
        
        LineRenderer lr = pathObj.AddComponent<LineRenderer>();
        lr.material = pathMaterial != null ? pathMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = target.gizmoColor;
        lr.endColor = new Color(target.gizmoColor.r, target.gizmoColor.g, target.gizmoColor.b, 1f);
        lr.startWidth = targetPathWidth;
        lr.endWidth = targetPathWidth * 0.5f;
        lr.positionCount = 1;
        lr.SetPosition(0, target.position);
        lr.useWorldSpace = true;
        lr.numCapVertices = 2;
        
        target.pathRenderer = lr;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        // Draw all targets
        if (targets != null)
        {
            foreach (var target in targets)
            {
                target.DrawGizmos();
            }
        }

        // Draw all missiles
        if (missiles != null)
        {
            foreach (var missile in missiles)
            {
                missile.DrawGizmos();
            }
        }

        // Draw simulation bounds
        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(100f, 50f, 100f));
        
        // Draw spawn area
        if (spawnOnHit)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.1f);
            Vector3 center = (spawnAreaMin + spawnAreaMax) * 0.5f;
            Vector3 size = spawnAreaMax - spawnAreaMin;
            Gizmos.DrawWireCube(center, size);
        }
    }

    private void OnGUI()
    {
        if (!showStats)
            return;

        int activeMissiles = missiles.Count(m => m.isActive);
        int activeTargets = targets.Count(t => t.isActive);

        GUILayout.BeginArea(new Rect(10, 10, 300, 280));
        GUILayout.Box("Missile Simulation Stats");
        GUILayout.Label($"Active Missiles: {activeMissiles}/{missiles.Count}");
        GUILayout.Label($"Active Targets: {activeTargets}/{targets.Count}");
        GUILayout.Label($"Total Hits: {totalHits}");
        GUILayout.Label($"Time Scale: {timeScale:F2}x");
        GUILayout.Label($"Paused: {pauseSimulation}");
        GUILayout.Label($"Spawn on Hit: {spawnOnHit}");
        GUILayout.Label($"Show Paths: {showPaths}");
        
        if (GUILayout.Button(pauseSimulation ? "Resume" : "Pause"))
        {
            pauseSimulation = !pauseSimulation;
        }
        
        if (GUILayout.Button("Reset Simulation"))
        {
            ClearAllPaths();
            InitializeSimulation();
            totalHits = 0;
        }
        
        if (GUILayout.Button("Spawn New Pair"))
        {
            SpawnNewPair();
        }
        
        GUILayout.EndArea();
    }

    // Editor helpers
    [ContextMenu("Add Sample Missile")]
    private void AddSampleMissile()
    {
        missiles.Add(new Missile
        {
            name = $"Missile {missiles.Count + 1}",
            position = new Vector3(0, 5, -30),
            velocity = new Vector3(0, 0, 1),
            speed = 50f,
            navigationGain = 3f,
            gizmoColor = Color.red
        });
    }

    [ContextMenu("Add Sample Target")]
    private void AddSampleTarget()
    {
        targets.Add(new Target
        {
            name = $"Target {targets.Count + 1}",
            position = new Vector3(20, 5, 20),
            centerPoint = new Vector3(20, 5, 20),
            radius = 15f,
            orbitSpeed = 10f,
            movementType = Target.MovementType.Circle,
            gizmoColor = Color.green
        });
    }

    [ContextMenu("Clear All")]
    private void ClearAll()
    {
        ClearAllPaths();
        missiles.Clear();
        targets.Clear();
    }

    private void ClearAllPaths()
    {
        if (pathContainer != null)
        {
            foreach (Transform child in pathContainer)
            {
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}