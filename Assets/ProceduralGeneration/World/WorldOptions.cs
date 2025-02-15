using UnityEditor;


// List of pre-set options for World Generation.
[InitializeOnLoad]
static class WorldOptions {
    public const int CHUNK_QUAD_AMOUNT = 128;
    public const float CHUNK_QUAD_SCALAR = 1f;

    public const int RENDER_DISTANCE = 16;
}