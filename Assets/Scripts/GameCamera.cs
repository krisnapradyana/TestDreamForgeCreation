using DG.Tweening;
using System;
using UnityEngine;

public class GameCamera : MonoBehaviour
{    
    public Action<Player, float> RepositionCamera;

    private void OnDestroy()
    {
        RepositionCamera -= RepositioningCamera;
    }

    private void Awake()
    {
        RepositionCamera += RepositioningCamera;
    }

    void RepositioningCamera(Player currentPlayer, float offset)
    {
        transform.DOMoveX(currentPlayer.transform.position.x + offset, .5f);
    }
}
