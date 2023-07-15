using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    PlayerMovement player;
    void OnMouseDown()
    {
        player.SetNewTarget(transform, gameObject.layer);
    }
    public void SetPlayer(PlayerMovement _player){
        player = _player;
    }
}
