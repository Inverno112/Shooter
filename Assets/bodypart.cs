using UnityEngine;

public class bodypart : MonoBehaviour, IDamagable
{
    [SerializeField]Transform parentDamagable;
    //max damage range: 100
    [SerializeField] byte maxDmg;
    [SerializeField] byte minDmg;
    IDamagable a;
    void Start(){
        a=parentDamagable.GetComponent<IDamagable>();
    }
    public void Dmg(byte Dmg, Transform source)
    {
        a.Dmg((byte)Random.Range(minDmg, maxDmg + 1), source);
    }
}
