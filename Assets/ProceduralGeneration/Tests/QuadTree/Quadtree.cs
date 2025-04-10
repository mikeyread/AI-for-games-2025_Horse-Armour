using System;
using System.Collections.Generic;
using UnityEngine;


// Global Parameters for the quad tree
static class QuadTreeParameters {
    public static uint maxDepth = 11;
    public static float GridSphereCheck = 3.33f;
}

public class QuadTree {
    private List<Quad> ActiveGrids;
    private Quad qt_Root;

    private WorldGenerationSettings settings;

    public QuadTree(float scale, Vector3 position, WorldGenerationSettings defaultSettings)
    {
        settings = defaultSettings;
        qt_Root = new(0, position, new Vector3(1, 0, 1) * scale, settings);
    }

    // Updates the Grid every frame by detecting Player position and parsing it into the root node.
    // The Root node, and subsiquently the entire rest of the Quadtree perform the recursion generation until a maximum depth is reached.
    public void UpdateGrid(Vector3 currentPosition)
    {
        Vector3 playerpos = Camera.main.transform.position;

        qt_Root.RefreshPosition(currentPosition);
        qt_Root.UpdateQuadtree(playerpos);
    }

    public List<Quad> GetActive()
    {
        ActiveGrids = new();
        if (qt_Root != null)
        {
            RecurseChild(qt_Root);

            if (ActiveGrids == null) return null;
        }

        return ActiveGrids;
    }

    // Recurses down the Quadtree to find all children of the root.
    private void RecurseChild(Quad parent)
    {
        // Parents are parsed immediatelly.
        ActiveGrids.Add(parent);

        if (parent.branch.bottomLeft == null) return;

        if (parent.branch.bottomLeft != null) RecurseChild(parent.branch.bottomLeft);
        else ActiveGrids.Add(parent.branch.bottomLeft);

        if (parent.branch.bottomLeft != null) RecurseChild(parent.branch.bottomRight);
        else ActiveGrids.Add(parent.branch.bottomRight);

        if (parent.branch.bottomLeft != null) RecurseChild(parent.branch.topLeft);
        else ActiveGrids.Add(parent.branch.topLeft);

        if (parent.branch.bottomLeft != null) RecurseChild(parent.branch.topRight);
        else ActiveGrids.Add(parent.branch.topRight);
    }
}



// A Grid node inside the Quadtree
public class Quad {

    public uint n_depth;

    public Vector3 g_Position;
    public Vector3 n_Bounds;

    public struct Node
    {
        // The Corners of the Branch, if one is empty then the parent node is a leaf.
        public Quad bottomRight;
        public Quad bottomLeft;
        public Quad topRight;
        public Quad topLeft;
    }

    public Node branch;

    public QuadTreeChunk chunk;
    public WorldGenerationSettings settings;


    public Quad(uint depth, Vector3 position, Vector3 bounds, WorldGenerationSettings defaultSettings)
    {
        settings = defaultSettings;
        n_depth = depth;
        g_Position = position;
        n_Bounds = bounds;
    }

    // Refreshes Grid Positions
    public void RefreshPosition(Vector3 newPosition) { g_Position = newPosition; }


    public void UpdateQuadtree(Vector3 playerPosition) {

        if (WithinDistance(playerPosition)) Subdivide();
        else CollapseQuadtree();

        if (!IsLeaf()) UpdateChildren(playerPosition);
        else
        {
            GenerateChunk();
            chunk.UpdateChunk();
        }
    }

    private void UpdateChildren(Vector3 playerPosition) {
        branch.bottomLeft.UpdateQuadtree(playerPosition);
        branch.bottomRight.UpdateQuadtree(playerPosition);
        branch.topLeft.UpdateQuadtree(playerPosition);
        branch.topRight.UpdateQuadtree(playerPosition);

        DestroyChunk();
    }

    // Collapses the Quadtree due to not being within local distance to retain it's LOD.
    private void CollapseQuadtree() {
        if (!IsLeaf())
        {
            branch.bottomLeft.CollapseQuadtree();
            branch.bottomRight.CollapseQuadtree();
            branch.topLeft.CollapseQuadtree();
            branch.topRight.CollapseQuadtree();

            if (branch.bottomLeft.IsLeaf())
            {
                branch.bottomLeft.DestroyChunk();
                branch.bottomLeft = null;

                branch.bottomRight.DestroyChunk();
                branch.bottomRight = null;

                branch.topLeft.DestroyChunk();
                branch.topLeft = null;

                branch.topRight.DestroyChunk();
                branch.topRight = null;
            }
        }
    }


    private void Subdivide() {
        if (!IsLeaf()) return;

        // First determines if the maximum depth has been reached, and if not, whether the Node has present branches as to skip new branch generation.
        if (n_depth < settings.Quadtree_maxDepth) {
            if (branch.bottomLeft != null) return;
            else branch.bottomLeft = new(n_depth + 1, g_Position - n_Bounds / 4, n_Bounds / 2, settings);

            if (branch.bottomRight != null) return;
            else branch.bottomRight = new(n_depth + 1, g_Position - new Vector3(-n_Bounds.x / 4, 0, n_Bounds.z / 4), n_Bounds / 2, settings);

            if (branch.topLeft != null) return;
            else branch.topLeft = new(n_depth + 1, g_Position + new Vector3(-n_Bounds.x / 4, 0, n_Bounds.z / 4), n_Bounds / 2, settings);

            if (branch.topRight != null) return;
            else branch.topRight = new(n_depth + 1, g_Position + n_Bounds / 4, n_Bounds / 2, settings);

            DestroyChunk();
        }
        else
        {
            GenerateChunk();
        }
    }

    public bool IsLeaf()
    {
        if (branch.bottomLeft != null) return false;
        if (branch.bottomRight != null) return false;
        if (branch.topRight != null) return false;
        if (branch.topLeft != null) return false;

        return true;
    }

    public bool InBounds(Vector3 playerPosition)
    {
        if (playerPosition.x < BLCorner_Position().x || playerPosition.x > TRCorner_Position().x) return false;
        if (playerPosition.z < BLCorner_Position().z || playerPosition.z > TRCorner_Position().z) return false;

        return true;
    }


    // Calculates if the quad is within a reasonable distance from the player to subdivide, rather than calculating purely within a bounding area.
    // This is overall more seamless than bounding grid detection and allows for cross Neighbor detection. The only "caveat" is that it increases LOD detail from outside grid bounds.
    public bool WithinDistance(Vector3 playerPosition) {
        float SphereOfInfluence = (n_Bounds * settings.Quadtree_GridSphericalCheck).sqrMagnitude;
        Vector3 snappedPlayerPosition = new(playerPosition.x, 0, playerPosition.z);

        if ((snappedPlayerPosition - g_Position).sqrMagnitude <= SphereOfInfluence) return true;
        else return false;
    }

    public Vector3 BLCorner_Position() { return g_Position - new Vector3(n_Bounds.x / 2, 0, n_Bounds.z / 2); }
    public Vector3 TRCorner_Position() { return g_Position - new Vector3(-n_Bounds.x / 2, 0, -n_Bounds.z / 2); }


    // Cleans up the Mesh and Game Object before assigning null to the chunk object itself.
    public void DestroyChunk() {
        if (chunk == null) return;

        Mesh.Destroy(this.chunk.chunkMesh);
        GameObject.Destroy(this.chunk.Chunk);
        chunk = null;
    }

    private void GenerateChunk()
    {
        if (chunk != null) return;
        chunk = new QuadTreeChunk(this);
    }
}