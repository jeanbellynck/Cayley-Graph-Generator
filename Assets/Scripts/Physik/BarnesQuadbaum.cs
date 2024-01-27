using UnityEngine;

public class BarnesQuadbaum {

    // Konstruktorvariablen
    private Vector3 position;
    private float radius;

    // Baumvariablen
    private Vector3 punktItem;
    private BarnesQuadbaum not;
    private BarnesQuadbaum nwt;
    private BarnesQuadbaum swt;
    private BarnesQuadbaum sot;
    private BarnesQuadbaum nob;
    private BarnesQuadbaum nwb;
    private BarnesQuadbaum swb;
    private BarnesQuadbaum sob;
    public Vector3 schwerpunkt;
    public int masse;
    private float präzision;

    public BarnesQuadbaum(Vector3 position, float radius, float präzision) {
        this.position = position;
        this.radius = radius;
        this.präzision = präzision;
        this.masse = 0;
    }

    public void Add(Vector3 punkt) {
        if(punktInBounds(punkt)) {
            if (masse == 0) {
                punktItem = punkt;
            } else {
                if(masse == 1) {
                    Teile();
                    not.Add(punktItem);
                    nwt.Add(punktItem);
                    swt.Add(punktItem);
                    sot.Add(punktItem);
                    nob.Add(punktItem);
                    nwb.Add(punktItem);
                    swb.Add(punktItem);
                    sob.Add(punktItem);
                }
                not.Add(punkt);
                nwt.Add(punkt);
                swt.Add(punkt);
                sot.Add(punkt);
                nob.Add(punkt);
                nwb.Add(punkt);
                swb.Add(punkt);
                sob.Add(punkt);
            }
            masse++;
        } 
    }

    public void Teile() {
        not = new BarnesQuadbaum(position + radius/2*(Vector3.up+Vector3.right+Vector3.forward), radius/2, präzision);
        nwt = new BarnesQuadbaum(position + radius/2*(Vector3.up+Vector3.left+Vector3.forward), radius/2, präzision);
        swt = new BarnesQuadbaum(position + radius/2*(Vector3.down+Vector3.left+Vector3.forward), radius/2, präzision);
        sot = new BarnesQuadbaum(position + radius/2*(Vector3.down+Vector3.right+Vector3.forward), radius/2, präzision);
        nob = new BarnesQuadbaum(position + radius/2*(Vector3.up+Vector3.right+Vector3.back), radius/2, präzision);
        nwb = new BarnesQuadbaum(position + radius/2*(Vector3.up+Vector3.left+Vector3.back), radius/2, präzision);
        swb = new BarnesQuadbaum(position + radius/2*(Vector3.down+Vector3.left+Vector3.back), radius/2, präzision);
        sob = new BarnesQuadbaum(position + radius/2*(Vector3.down+Vector3.right+Vector3.back), radius/2, präzision);
    }

    private bool punktInBounds(Vector3 punkt) {
        return punkt.x >= position.x-radius &&
        punkt.x < position.x+radius &&
        punkt.y >= position.y-radius &&
        punkt.y < position.y + radius && 
        punkt.z >= position.z-radius &&
        punkt.z < position.z+radius;
    }

    public void BerechneSchwerpunkt() {
        if(masse == 0) {
        } else if(masse == 1) {
            schwerpunkt = punktItem;
        } else{
            not.BerechneSchwerpunkt();
            nwt.BerechneSchwerpunkt();
            swt.BerechneSchwerpunkt();
            sot.BerechneSchwerpunkt();
            nob.BerechneSchwerpunkt();
            nwb.BerechneSchwerpunkt();
            swb.BerechneSchwerpunkt();
            sob.BerechneSchwerpunkt();
            schwerpunkt = (not.schwerpunkt*not.masse +
                nwt.schwerpunkt*nwt.masse +
                sot.schwerpunkt*sot.masse +
                swt.schwerpunkt*swt.masse + 
                nob.schwerpunkt*nob.masse +
                nwb.schwerpunkt*nwb.masse +
                sob.schwerpunkt*sob.masse +
                swb.schwerpunkt*swb.masse)/masse;
        }
    }

    public Vector3 calculateRepulsionForceOnVertex(Vector3 punkt, float repulsionDistance) {
        if(masse == 0) {
            return Vector3.zero;
        } else if(masse == 1) {
            if(punktInBounds(punkt)) {
                return Vector3.zero;
            } else {
                return BerechneKraft(punkt, punktItem);
            }
        } else {
            // Ignore points that are far away.
            if(repulsionDistance != 0 && Vector3.Distance(punkt, schwerpunkt) > repulsionDistance) {
                return Vector2.zero;
            }
            if(2*radius/Vector3.Distance(punkt, schwerpunkt) < präzision && !punktInBounds(punkt)) {
                return masse * BerechneKraft(punkt, schwerpunkt);
            } else {
                return not.calculateRepulsionForceOnVertex(punkt, repulsionDistance) + nwt.calculateRepulsionForceOnVertex(punkt, repulsionDistance)
                    + sot.calculateRepulsionForceOnVertex(punkt, repulsionDistance) + swt.calculateRepulsionForceOnVertex(punkt, repulsionDistance)
                    + nob.calculateRepulsionForceOnVertex(punkt, repulsionDistance) + nwb.calculateRepulsionForceOnVertex(punkt, repulsionDistance)
                    + sob.calculateRepulsionForceOnVertex(punkt, repulsionDistance) + swb.calculateRepulsionForceOnVertex(punkt, repulsionDistance);
            }
        }
    }

    private Vector3 BerechneKraft(Vector3 bewirkter, Vector3 wirkender) {
        Vector3 diff = wirkender - bewirkter;
        return -diff.normalized*Mathf.Pow(diff.magnitude, -2);
    }
}