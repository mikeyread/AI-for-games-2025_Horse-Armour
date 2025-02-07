using UnityEditor;


// List of pre-set options for World Generation.
[InitializeOnLoad]
static class WorldOptions {
    public const int CHUNK_QUAD_AMOUNT = 16;
    public const float CHUNK_QUAD_SCALAR = 0.5f;

    public const int RENDER_DISTANCE = 5;   // Not used ATM
}