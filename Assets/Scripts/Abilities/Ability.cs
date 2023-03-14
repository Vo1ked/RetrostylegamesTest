using System.Collections.Generic;
using UnityEngine;

namespace RetroStyleGamesTest.Abillity
{
    public abstract class Ability : ScriptableObject
    {
        public virtual Specialization Specialization { get; protected set; }
        public virtual WorkType WorkType { get; protected set; }
        public abstract void Execute(GameObject user, params object[] parameters);

        public static bool AbilityCheck(List<Ability> allabilities, Specialization specialization, ref List<Ability> abilities)
        {
            if (allabilities.Count < 1)
                return false;

            abilities = allabilities.FindAll(abbility => abbility.Specialization == specialization);
            var overrideAbilities = abilities.FindAll(abbility => abbility.WorkType == WorkType.@override);
            if (overrideAbilities.Count > 1)
            {
                Debug.LogError($"override abiliries with specialization {overrideAbilities[0].Specialization} more that one! will work only one");
            }
            return abilities.Count > 0;
        }
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
}
