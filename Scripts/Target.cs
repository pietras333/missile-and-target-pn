using UnityEngine;

[System.Serializable]
public class Target
{
    [Header("Target Properties")]
    public string name = "Target";
    public Vector3 position;
    public Vector3 velocity;
    public Color gizmoColor = Color.green;
    
    [Header("Movement Pattern")]
    public MovementType movementType = MovementType.Circle;
    public Vector3 centerPoint;
    public float radius = 20f;
    public float orbitSpeed = 10f;
    public Vector3 straightDirection = Vector3.forward;
    
    [Header("State")]
    public bool isActive = true;
    
    [HideInInspector]
    public LineRenderer pathRenderer;
    
    private float currentAngle;
    private Vector3 figure8Center;

    public enum MovementType
    {
        Circle,
        Figure8,
        Straight,
        Stationary
    }

    public void Initialize(Vector3 startPos, Vector3 center)
    {
        position = startPos;
        centerPoint = center;
        figure8Center = center;
        isActive = true;
        
        // Calculate initial angle for circular motion
        Vector3 offset = position - centerPoint;
        currentAngle = Mathf.Atan2(offset.z, offset.x);
    }

    public void UpdateTarget(float deltaTime)
    {
        if (!isActive)
            return;

        switch (movementType)
        {
            case MovementType.Circle:
                UpdateCircularMotion(deltaTime);
                break;
            case MovementType.Figure8:
                UpdateFigure8Motion(deltaTime);
                break;
            case MovementType.Straight:
                UpdateStraightMotion(deltaTime);
                break;
            case MovementType.Stationary:
                velocity = Vector3.zero;
                break;
        }

        // Update path renderer
        UpdatePathRenderer();
    }

    private void UpdatePathRenderer()
    {
        if (pathRenderer == null)
            return;

        // Add current position to the line renderer
        int currentPositions = pathRenderer.positionCount;
        pathRenderer.positionCount = currentPositions + 1;
        pathRenderer.SetPosition(currentPositions, position);
    }

    private void UpdateCircularMotion(float deltaTime)
    {
        currentAngle += (orbitSpeed / radius) * deltaTime;
        
        Vector3 newPos = centerPoint + new Vector3(
            Mathf.Cos(currentAngle) * radius,
            0f,
            Mathf.Sin(currentAngle) * radius
        );

        velocity = (newPos - position) / deltaTime;
        position = newPos;
    }

    private void UpdateFigure8Motion(float deltaTime)
    {
        currentAngle += (orbitSpeed / radius) * deltaTime;
        
        // Lissajous curve for figure-8
        Vector3 newPos = figure8Center + new Vector3(
            Mathf.Sin(currentAngle) * radius,
            0f,
            Mathf.Sin(currentAngle * 2f) * radius * 0.5f
        );

        velocity = (newPos - position) / deltaTime;
        position = newPos;
    }

    private void UpdateStraightMotion(float deltaTime)
    {
        velocity = straightDirection.normalized * orbitSpeed;
        position += velocity * deltaTime;
    }

    public void DrawGizmos()
    {
        if (!isActive)
            return;

        // Draw target body
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(position, 1f);
        Gizmos.DrawSphere(position, 0.3f);

        // Draw velocity vector
        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(position, velocity.normalized * 2f);

        // Draw movement path preview
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
        DrawPathPreview();

        // Draw target name
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(position + Vector3.up * 3f, name);
        #endif
    }

    private void DrawPathPreview()
    {
        int segments = 50;
        Vector3 lastPoint = position;

        for (int i = 1; i <= segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 nextPoint = Vector3.zero;

            switch (movementType)
            {
                case MovementType.Circle:
                    nextPoint = centerPoint + new Vector3(
                        Mathf.Cos(currentAngle + t) * radius,
                        0f,
                        Mathf.Sin(currentAngle + t) * radius
                    );
                    break;
                case MovementType.Figure8:
                    nextPoint = centerPoint + new Vector3(
                        Mathf.Sin(currentAngle + t) * radius,
                        0f,
                        Mathf.Sin((currentAngle + t) * 2f) * radius * 0.5f
                    );
                    break;
                case MovementType.Straight:
                    nextPoint = position + straightDirection.normalized * orbitSpeed * (i * 0.5f);
                    if (i > 10) return; // Only draw short preview for straight
                    break;
            }

            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}