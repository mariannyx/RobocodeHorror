using Unity.Cinemachine;
using UnityEngine;

public interface IGadget
{
    void Initialize(Camera camera, CinemachineCamera cameraCinema);

    int GadgetNum{ get; set; }
}
