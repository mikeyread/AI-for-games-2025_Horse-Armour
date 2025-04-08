using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Parent Class of a Map Object
public class MapObject {
    public Vector3 position;
    public Vector3 bounds;
    public bool hasCollision;
}

public class TreeObject : MapObject {

    public TreeObject()
    {
        
    }
}