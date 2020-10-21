using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class BaseUnitStatus
{
    public virtual string Key { get; protected set; }

    protected Unit unit;
    protected int duration;
    protected virtual int DefaultDuration { get; set; }

    protected bool endless;

    public BaseUnitStatus() => Init(null, DefaultDuration);

    public BaseUnitStatus(Unit _unit) => Init(_unit, DefaultDuration);

    public BaseUnitStatus(int _duration) => Init(null, _duration);

    public BaseUnitStatus(Unit _unit, int _duration) => Init(_unit, _duration);

    protected void Init(Unit _unit, int _duration) => (unit, duration) = (_unit, _duration);

    public virtual Stats StatsEffects() => new Stats();
}
