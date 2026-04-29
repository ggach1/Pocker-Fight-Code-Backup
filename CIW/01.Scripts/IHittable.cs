using UnityEngine;

public interface IHittable
{
    public void GetDamage(float dam);
    public void SetMaxHealth(float heal);
}
