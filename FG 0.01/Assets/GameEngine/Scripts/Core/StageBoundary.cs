using UnityEngine;
using System.Collections;
public class StageBoundary : MonoBehaviour {

    void OnTriggerExit2D(Collider2D col)
    {
        GameObject exitObject = col.gameObject;
        PlayerOriginPoint playerOriginPoint = exitObject.GetComponent<PlayerOriginPoint>();

        if (playerOriginPoint != null)
        {
            playerOriginPoint.player.Die();
        }
    }
}
