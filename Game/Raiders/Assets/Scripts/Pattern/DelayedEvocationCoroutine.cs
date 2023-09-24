using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayedEvocationCoroutine
{
    private Evocation Evocation;
    private bool HasExecuted = false;
    private IDelayedEvocationSummoner Executor;

    public DelayedEvocationCoroutine(IDelayedEvocationSummoner executor)
    {
        this.Executor = executor;
    }

    public void Run(Character caster, Block targetBlock, string id, int summonLevel, float timeDelay)
    {
        ((MonoBehaviour)Executor).StartCoroutine(ExecuteSummonDelayed(caster, targetBlock, id, summonLevel, timeDelay));
    }

    IEnumerator ExecuteSummonDelayed(Character caster, Block targetBlock, string id, int summonLevel, float timeDelay)
    {
        if (!HasExecuted)
        {
            yield return new WaitForSeconds(timeDelay);
            Evocation = Spell.ut_execute_summon(caster, targetBlock, id, summonLevel);
            HasExecuted = true;
            Executor.OnSummonExecuted(this);
        }
    }

    public Evocation GetEvocation() => HasExecuted ? Evocation : null;

}

public interface IDelayedEvocationSummoner
{
    public void OnSummonExecuted(DelayedEvocationCoroutine dec);
}
