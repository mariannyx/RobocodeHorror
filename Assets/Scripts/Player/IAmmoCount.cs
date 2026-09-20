using System;

public interface IAmmoCount
{
    event Action<int, int, string> OnAmmoChanged;

    void RefreshUI();
}