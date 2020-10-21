using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmobilizedUnitStatus : BaseUnitStatus {

    public override string Key {
        get { return "immobilized"; }
    }

    protected override int DefaultDuration {
        get { return 2; }
    }

    public override Stats StatsEffects() {
        return new Stats(0, -1);
    }
}
