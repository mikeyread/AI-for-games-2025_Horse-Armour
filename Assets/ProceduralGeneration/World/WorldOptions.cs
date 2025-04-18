using UnityEditor;


[InitializeOnLoad]
static class WorldOptions {
    // Chunk Parameters
    public const int CHUNK_QUAD_AMOUNT = 32;
    public const float CHUNK_QUAD_SCALAR = 0.5f;

    // World Generation Parameters
    public const uint RENDER_DISTANCE = 16;
}