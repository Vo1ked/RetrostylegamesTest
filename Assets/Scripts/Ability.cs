using UnityEngine;

public abstract class Ability : ScriptableObject
{
    public Specialization Specialization { get; protected set; }
    public WorkType WorkType { get; protected set; }
    public abstract void Execute(GameObject user, params object[] parameters);
}

public enum Specialization
{
    Spawn,
    Move,
    Attack,
    Damage,
    OnDestroy
}
public enum WorkType
{
    addition,
    @override
}
