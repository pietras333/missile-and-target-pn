using UnityEngine;

[System.Serializable]
public class Missile
{
    [Header("Missile Properties")]
    public string name = "Missile";
    public Vector3 position;
    public Vector3 velocity;
    public float speed = 50f;
    public float navigationGain = 3f; // PN navigation constant (typically 3-5)
    public Color gizmoColor = Color.red;
    
    [Header("State")]
    public bool isActive = true;
    public Target lockedTarget;
    
    [HideInInspector]
    public LineRenderer pathRenderer;
    
    private Vector3 lastLineOfSight;
    private bool hasLastLOS;

    public void Initialize(Vector3 startPos, Vector3 startVel)
    {
        position = startPos;
        velocity = startVel.normalized * speed;
        isActive = true;
        hasLastLOS = false;
    }

    public void LockOntoTarget(Target target)
    {
        lockedTarget = target;
        hasLastLOS = false;
    }

    public void UpdateMissile(float deltaTime)
    {
        if (!isActive || lockedTarget == null || !lockedTarget.isActive)
            return;

        // Calculate line of sight (LOS) vector
        Vector3 los = (lockedTarget.position - position).normalized;
        
        // Calculate LOS rate (angular velocity of LOS)
        Vector3 losRate = Vector3.zero;
        if (hasLastLOS)
        {
            Vector3 losChange = los - lastLineOfSight;
            losRate = losChange / deltaTime;
        }
        
        lastLineOfSight = los;
        hasLastLOS = true;

        // Calculate closing velocity
        Vector3 relativeVelocity = velocity - lockedTarget.velocity;
        float closingSpeed = Vector3.Dot(relativeVelocity, los);

        // Pure Navigation guidance law
        // Lateral acceleration = N * Vc * λ_dot
        // where N = navigation gain, Vc = closing velocity, λ_dot = LOS rate
        Vector3 lateralAcceleration = navigationGain * closingSpeed * losRate;

        // Update velocity with guidance command
        velocity += lateralAcceleration * deltaTime;
        
        // Maintain constant speed
        velocity = velocity.normalized * speed;

        // Update position
        position += velocity * deltaTime;

        // Check if hit target (within 2 units)
        float distanceToTarget = Vector3.Distance(position, lockedTarget.position);
        if (distanceToTarget < 2f)
        {
            isActive = false;
            lockedTarget.isActive = false;
        }
    }

    public void DrawGizmos()
    {
        if (!isActive)
            return;

        // Draw missile body
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(position, 0.5f);

        // Draw velocity vector
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(position, velocity.normalized * 3f);

        // Draw line to locked target
        if (lockedTarget != null && lockedTarget.isActive)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(position, lockedTarget.position);
        }

        // Draw missile name
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(position + Vector3.up * 2f, name);
        #endif
    }
}