using UnityEngine;

public class PlayerRoomTracker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        RoomTriggerInfo roomInfo = other.GetComponent<RoomTriggerInfo>();
        if (roomInfo != null)
        {
            MapGenerator.instance.UpdateActiveRoom(roomInfo.roomId);
        }
    }
}