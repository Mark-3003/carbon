using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    PlayerMovement player;
    void Awake()
    {
        player = GameObject.Find("Player").GetComponent<PlayerMovement>();
    }
    void OnMouseDown()
    {
        player.SetNewTarget(this);
    }
}
