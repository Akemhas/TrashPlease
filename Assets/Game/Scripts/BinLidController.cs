using System.Collections.Generic;
using UnityEngine;

public class BinLidController : MonoBehaviour
{
    [SerializeField] private List<PlayerBin> _playerBins;
    [SerializeField] private float _range = 1.5f;

    private Camera _mainCam;

    private void Start()
    {
        _mainCam = Camera.main;
    }

    private void Update()
    {
        var mouseWorldPos = GetMouseWorldPos();

        foreach (var playerBin in _playerBins)
        {
            var dist = Vector3.Distance(playerBin.transform.position, mouseWorldPos);

            if (dist <= _range && InputManager.HasTrash)
            {
                playerBin.OpenLid();
            }
            else
            {
                playerBin.CloseLid();
            }
        }
    }

    private Vector3 GetMouseWorldPos()
    {
        var mousePos = InputManager.MousePosition;
        Vector3 mouseWorldPos = _mainCam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -_mainCam.transform.position.z));
        return mouseWorldPos;
    }
}