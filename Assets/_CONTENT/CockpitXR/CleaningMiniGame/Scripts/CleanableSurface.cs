using System;
using System.Collections.Generic;
using UnityEngine;

public enum CellState
{
    Dirty,
    Wet,
    Clean
}

[Serializable]
public class CleaningCell
{
    public CellState State;
    public float WetAmount;      // 0-1, gradual wetting
    public float CleanAmount;    // 0-1, gradual cleaning
    public float WetTimer;       // Time remaining before drying out
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CleanableSurface : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 16;
    [SerializeField] private int gridHeight = 16;
    [SerializeField] private Vector2 surfaceSize = new Vector2(0.4f, 0.4f);
    
    [Header("State Colors")]
    [SerializeField] private Color dirtyColor = new Color(0.35f, 0.25f, 0.15f, 0f);
    [SerializeField] private Color wetColor = new Color(0.25f, 0.3f, 0.4f, 0f);
    [SerializeField] private Color cleanColor = new Color(1f, 1f, 1f, 1f);
    
    [Header("Wet State Transition")]
    [SerializeField] private float wetThreshold = 0.3f;        // WetAmount needed to transition to Wet state
    [SerializeField] private float cleanableThreshold = 0.5f;  // WetAmount needed to allow cleaning
    
    [Header("Clean State Transition")]
    [SerializeField] private float cleanThreshold = 1.0f;      // CleanAmount needed to transition to Clean state
    [SerializeField] [Range(0f, 0.5f)] private float cleanMissProbability = 0.2f;
    
    [Header("Dry Out Settings")]
    [SerializeField] private bool enableDryOut = true;
    [SerializeField] private float dryOutTime = 8f;            // Seconds before wet becomes dirty
    [SerializeField] private float dryOutRate = 0.15f;         // How fast wetness decreases per second
    
    [Header("Visual Smoothing")]
    [SerializeField] private bool smoothColorTransitions = true;
    [SerializeField] private float colorTransitionSpeed = 5f;
    
    [Header("Audio Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip cleanSound;
    [SerializeField] private AudioClip completionSound;
    [SerializeField] [Range(0f, 1f)] private float audioVolume = 0.5f;

    public event Action<float> OnProgressChanged;
    public event Action OnFullyCleaned;
    public event Action<Vector2Int, CellState> OnCellStateChanged;
    
    private CleaningCell[,] cells;
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    
    private Vector3[] vertices;
    private Color[] vertexColors;
    private Color[] targetColors;  // For smooth transitions
    private Vector2[] uvs;
    private int[] triangles;
    
    private Vector2 cellSize;
    private int totalCells;
    private int cleanedCells;
    private float completionPercentage;
    
    private bool meshNeedsUpdate;
    private bool isInitialized;
    
    // Object pooling for GetCellsInRadius
    private List<Vector2Int> cellResultPool = new List<Vector2Int>(64);
    
    public int GridWidth => gridWidth;
    public int GridHeight => gridHeight;
    public Vector2 SurfaceSize => surfaceSize;
    public Vector2 CellSize => cellSize;
    public float CompletionPercentage => completionPercentage;
    public bool IsFullyCleaned => cleanedCells >= totalCells;
    public int TotalCells => totalCells;
    public int CleanedCells => cleanedCells;
    public int WetCells { get; private set; }
    public int DirtyCells => totalCells - cleanedCells - WetCells;
    
    private void Awake()
    {
        Initialize();
    }
    
    private void Update()
    {
        if (!isInitialized) return;
        
        if (enableDryOut)
        {
            ProcessDryOut();
        }
        
        if (smoothColorTransitions)
        {
            UpdateColorTransitions();
        }
        
        if (meshNeedsUpdate)
        {
            ApplyMeshColors();
            meshNeedsUpdate = false;
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying && isInitialized)
        {
            Initialize();
        }
    }
    
    public void Initialize()
    {
        // Calculate cell size
        cellSize = new Vector2(surfaceSize.x / gridWidth, surfaceSize.y / gridHeight);
        totalCells = gridWidth * gridHeight;
        cleanedCells = 0;
        WetCells = 0;
        completionPercentage = 0f;
        
        // Initialize grid data
        InitializeGrid();
        
        // Generate mesh
        GenerateMesh();
        
        // Setup collider
        SetupCollider();
        
        isInitialized = true;
    }
    
    private void InitializeGrid()
    {
        cells = new CleaningCell[gridWidth, gridHeight];
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                cells[x, y] = new CleaningCell
                {
                    State = CellState.Dirty,
                    WetAmount = 0f,
                    CleanAmount = 0f,
                    WetTimer = 0f
                };
            }
        }
    }
    
    private void GenerateMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        
        // Create new mesh
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "CleanableSurfaceMesh";
        }
        else
        {
            mesh.Clear();
        }
        
        // Calculate array sizes
        int vertexCount = totalCells * 4;
        int triangleCount = totalCells * 6;
        
        // Initialize arrays
        vertices = new Vector3[vertexCount];
        uvs = new Vector2[vertexCount];
        triangles = new int[triangleCount];
        vertexColors = new Color[vertexCount];
        targetColors = new Color[vertexCount];
        
        int vertIndex = 0;
        int triIndex = 0;
        
        // Generate geometry for each cell
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Calculate cell corners (with gap)
                float xMin = -surfaceSize.x / 2f + x * cellSize.x;
                float xMax = -surfaceSize.x / 2f + (x + 1) * cellSize.x;
                float yMin = -surfaceSize.y / 2f + y * cellSize.y;
                float yMax = -surfaceSize.y / 2f + (y + 1) * cellSize.y;
                
                // Vertices (counter-clockwise from bottom-left)
                vertices[vertIndex + 0] = new Vector3(xMin, yMin, 0);
                vertices[vertIndex + 1] = new Vector3(xMax, yMin, 0);
                vertices[vertIndex + 2] = new Vector3(xMax, yMax, 0);
                vertices[vertIndex + 3] = new Vector3(xMin, yMax, 0);
                
                // UVs - map each cell to full 0-1 range for potential texture tiling
                float uMin = (float)x / gridWidth;
                float uMax = (float)(x + 1) / gridWidth;
                float vMin = (float)y / gridHeight;
                float vMax = (float)(y + 1) / gridHeight;
                
                uvs[vertIndex + 0] = new Vector2(uMin, vMin);
                uvs[vertIndex + 1] = new Vector2(uMax, vMin);
                uvs[vertIndex + 2] = new Vector2(uMax, vMax);
                uvs[vertIndex + 3] = new Vector2(uMin, vMax);
                
                // Initial colors (dirty state)
                vertexColors[vertIndex + 0] = dirtyColor;
                vertexColors[vertIndex + 1] = dirtyColor;
                vertexColors[vertIndex + 2] = dirtyColor;
                vertexColors[vertIndex + 3] = dirtyColor;
                
                targetColors[vertIndex + 0] = dirtyColor;
                targetColors[vertIndex + 1] = dirtyColor;
                targetColors[vertIndex + 2] = dirtyColor;
                targetColors[vertIndex + 3] = dirtyColor;
                
                // Triangles (two triangles per cell)
                triangles[triIndex + 0] = vertIndex + 0;
                triangles[triIndex + 1] = vertIndex + 2;
                triangles[triIndex + 2] = vertIndex + 1;
                triangles[triIndex + 3] = vertIndex + 0;
                triangles[triIndex + 4] = vertIndex + 3;
                triangles[triIndex + 5] = vertIndex + 2;
                
                vertIndex += 4;
                triIndex += 6;
            }
        }
        
        // Apply to mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = vertexColors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        mesh.UploadMeshData(false);
        
        meshFilter.mesh = mesh;
    }
    
    private void SetupCollider()
    {
        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false;
    }
    
    public void ApplySpray(Vector3 worldPosition, float radius, float amount)
    {
        if (!isInitialized) return;
        
        var affectedCells = GetCellsInRadius(worldPosition, radius);
        bool anyAffected = false;
        
        foreach (var pos in affectedCells)
        {
            var cell = cells[pos.x, pos.y];
            
            // Can only wet dirty or already wet cells
            if (cell.State == CellState.Dirty || cell.State == CellState.Wet)
            {
                // Calculate distance-based falloff
                Vector3 cellWorldPos = GetCellWorldPosition(pos.x, pos.y);
                float distance = Vector3.Distance(worldPosition, cellWorldPos);
                float falloff = 1f - Mathf.Clamp01(distance / radius);
                float adjustedAmount = amount * falloff;
                
                // Apply wetness
                float previousWet = cell.WetAmount;
                cell.WetAmount = Mathf.Clamp01(cell.WetAmount + adjustedAmount);
                
                // Reset dry timer when sprayed
                cell.WetTimer = dryOutTime;
                
                // State transition: Dirty -> Wet
                if (cell.State == CellState.Dirty && cell.WetAmount >= wetThreshold)
                {
                    cell.State = CellState.Wet;
                    WetCells++;
                    OnCellStateChanged?.Invoke(pos, CellState.Wet);
                }
                
                // Update visual
                UpdateCellTargetColor(pos.x, pos.y);
                anyAffected = true;
            }
        }
        
        if (anyAffected)
        {
            meshNeedsUpdate = true;
        }
    }
    
    public bool ApplySponge(Vector3 worldPosition, float radius, float amount)
    {   
        if (!isInitialized) return false;
        
        var affectedCells = GetCellsInRadius(worldPosition, radius);
        bool anyCleaned = false;
        bool anyStateChanged = false;
        
        foreach (var pos in affectedCells)
        {
            // Random chance to miss this cell
            if (UnityEngine.Random.value < cleanMissProbability)
                continue;

            var cell = cells[pos.x, pos.y];
            
            // Can only clean wet cells with sufficient wetness
            if (cell.State == CellState.Wet && cell.WetAmount >= cleanableThreshold)
            {
                // Calculate distance-based falloff
                Vector3 cellWorldPos = GetCellWorldPosition(pos.x, pos.y);
                float distance = Vector3.Distance(worldPosition, cellWorldPos);
                float falloff = 1f - Mathf.Clamp01(distance / radius);
                float adjustedAmount = amount * falloff;
                
                // Apply cleaning
                cell.CleanAmount = Mathf.Clamp01(cell.CleanAmount + adjustedAmount);
                anyCleaned = true;
                
                // State transition: Wet -> Clean
                if (cell.CleanAmount >= cleanThreshold)
                {
                    cell.State = CellState.Clean;
                    cleanedCells++;
                    WetCells--;
                    anyStateChanged = true;
                    OnCellStateChanged?.Invoke(pos, CellState.Clean);
                }
                
                // Update visual
                UpdateCellTargetColor(pos.x, pos.y);
            }
        }
        
        if (anyCleaned)
        {
            meshNeedsUpdate = true;
            
            if (anyStateChanged)
            {
                UpdateCompletionProgress();
                PlaySound(cleanSound);
            }
        }
        
        return anyCleaned;
    }
    
    public bool IsPositionWet(Vector3 worldPosition)
    {
        if (!isInitialized) return false;
        
        Vector2Int? gridPos = WorldToGridPosition(worldPosition);
        if (!gridPos.HasValue) return false;
        
        var cell = cells[gridPos.Value.x, gridPos.Value.y];
        return cell.State == CellState.Wet && cell.WetAmount >= cleanableThreshold;
    }
    
    public Vector2Int? WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        
        // Convert to grid coordinates
        int x = Mathf.FloorToInt((localPos.x + surfaceSize.x / 2f) / cellSize.x);
        int y = Mathf.FloorToInt((localPos.y + surfaceSize.y / 2f) / cellSize.y);
        
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return new Vector2Int(x, y);
        }
        
        return null;
    }
    
    public Vector3 GetCellLocalPosition(int x, int y)
    {
        float xPos = -surfaceSize.x / 2f + (x + 0.5f) * cellSize.x;
        float yPos = -surfaceSize.y / 2f + (y + 0.5f) * cellSize.y;
        
        return new Vector3(xPos, yPos, 0f);
    }
    
    public Vector3 GetCellWorldPosition(int x, int y)
    {
        return transform.TransformPoint(GetCellLocalPosition(x, y));
    }
    
    private List<Vector2Int> GetCellsInRadius(Vector3 worldPosition, float radius)
    {
        cellResultPool.Clear();
        
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        
        // Convert to grid space
        float gridX = (localPos.x + surfaceSize.x / 2f) / cellSize.x;
        float gridY = (localPos.y + surfaceSize.y / 2f) / cellSize.y;
        
        // Calculate search radius in cells
        int cellRadiusX = Mathf.CeilToInt(radius / cellSize.x) + 1;
        int cellRadiusY = Mathf.CeilToInt(radius / cellSize.y) + 1;
        
        int centerX = Mathf.FloorToInt(gridX);
        int centerY = Mathf.FloorToInt(gridY);
        
        // Search within bounding box
        for (int x = centerX - cellRadiusX; x <= centerX + cellRadiusX; x++)
        {
            for (int y = centerY - cellRadiusY; y <= centerY + cellRadiusY; y++)
            {
                // Bounds check
                if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
                    continue;
                
                // Distance check
                Vector3 cellWorldPos = GetCellWorldPosition(x, y);
                float distance = Vector3.Distance(worldPosition, cellWorldPos);
                
                if (distance <= radius)
                {
                    cellResultPool.Add(new Vector2Int(x, y));
                }
            }
        }
        
        return cellResultPool;
    }
    
    private void UpdateCellTargetColor(int x, int y)
    {
        var cell = cells[x, y];
        int baseVertex = (y * gridWidth + x) * 4;
        
        Color targetColor;
        
        switch (cell.State)
        {
            case CellState.Dirty:
                // Interpolate dirty color based on any residual wetness
                targetColor = Color.Lerp(dirtyColor, wetColor, cell.WetAmount * 0.5f);
                break;
                
            case CellState.Wet:
                // Interpolate based on wetness and cleaning progress
                Color wetBase = Color.Lerp(dirtyColor, wetColor, cell.WetAmount);
                targetColor = Color.Lerp(wetBase, cleanColor, cell.CleanAmount * 0.3f);
                break;
                
            case CellState.Clean:
                targetColor = cleanColor;
                break;
                
            default:
                targetColor = dirtyColor;
                break;
        }
        
        // Set target for all 4 vertices
        targetColors[baseVertex + 0] = targetColor;
        targetColors[baseVertex + 1] = targetColor;
        targetColors[baseVertex + 2] = targetColor;
        targetColors[baseVertex + 3] = targetColor;
        
        // If not using smooth transitions, apply immediately
        if (!smoothColorTransitions)
        {
            vertexColors[baseVertex + 0] = targetColor;
            vertexColors[baseVertex + 1] = targetColor;
            vertexColors[baseVertex + 2] = targetColor;
            vertexColors[baseVertex + 3] = targetColor;
        }
    }
    
    private void UpdateColorTransitions()
    {
        bool anyChanged = false;
        float lerpFactor = colorTransitionSpeed * Time.deltaTime;
        
        for (int i = 0; i < vertexColors.Length; i++)
        {
            if (vertexColors[i] != targetColors[i])
            {
                vertexColors[i] = Color.Lerp(vertexColors[i], targetColors[i], lerpFactor);
                anyChanged = true;
            }
        }
        
        if (anyChanged)
        {
            meshNeedsUpdate = true;
        }
    }
    
    private void ApplyMeshColors()
    {
        if (mesh != null)
        {
            mesh.colors = vertexColors;
        }
    }
    
    private void ProcessDryOut()
    {
        float deltaTime = Time.deltaTime;
        bool anyChanged = false;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                var cell = cells[x, y];
                
                if (cell.State == CellState.Wet)
                {
                    // Decrease timer
                    cell.WetTimer -= deltaTime;
                    
                    if (cell.WetTimer <= 0)
                    {
                        // Start drying out
                        cell.WetAmount -= dryOutRate * deltaTime;
                        
                        // Transition back to dirty if fully dried
                        if (cell.WetAmount <= 0f)
                        {
                            cell.WetAmount = 0f;
                            cell.CleanAmount = 0f;
                            cell.State = CellState.Dirty;
                            WetCells--;
                            
                            OnCellStateChanged?.Invoke(new Vector2Int(x, y), CellState.Dirty);
                        }
                        
                        UpdateCellTargetColor(x, y);
                        anyChanged = true;
                    }
                }
            }
        }
        
        if (anyChanged)
        {
            meshNeedsUpdate = true;
        }
    }
    
    private void UpdateCompletionProgress()
    {
        float newPercentage = (float)cleanedCells / totalCells;
        
        if (!Mathf.Approximately(newPercentage, completionPercentage))
        {
            completionPercentage = newPercentage;
            OnProgressChanged?.Invoke(completionPercentage);
            
            if (cleanedCells >= totalCells)
            {
                PlaySound(completionSound);
                OnFullyCleaned?.Invoke();
            }
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, audioVolume);
        }
    }
}
