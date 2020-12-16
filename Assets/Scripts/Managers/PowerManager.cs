using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    private Dictionary<IPowerConduit, HashSet<IPowerConduit>> connectionMap
        = new Dictionary<IPowerConduit, HashSet<IPowerConduit>>();
    private List<IPowerConduit> powerConduits = new List<IPowerConduit>();

    private void Start()
    {
        Managers.Scene.onSceneLoaded += (s) => generateConnectionMap();
        Managers.Scene.onSceneUnloaded += (s) => generateConnectionMap();
        generateConnectionMap();
    }

    private void FixedUpdate()
    {
        List<IPowerer> powerers = powerConduits
           .FindAll(ipc => ipc is IPowerer)
           .ConvertAll(ipc => (IPowerer)ipc);
        powerers.ForEach(ipr =>
        {
            List<IPowerable> powerables = getPowerables(ipr);
            float powerToEach = ipr.ThroughPut * Time.fixedDeltaTime / powerables.Count;
            powerables.ForEach(pwr => ipr.givePower(
                -pwr.acceptPower(ipr.givePower(powerToEach))
                ));
        }
            );
    }

    private List<IPowerable> getPowerables(IPowerer source)
    {
        HashSet<IPowerable> powerables = new HashSet<IPowerable>();
        List<IPowerTransferer> wires = connectionMap[source].ToList()
            .FindAll(ipc => ipc is IPowerTransferer)
            .ConvertAll(ipc => (IPowerTransferer)ipc);
        int i = 0;
        while (i < wires.Count)
        {
            connectionMap[wires[i]].ToList().ForEach(
                ipc =>
                {
                    if (ipc is IPowerable)
                    {
                        powerables.Add((IPowerable)ipc);
                    }
                    if (ipc is IPowerTransferer)
                    {
                        if (!wires.Contains(ipc))
                        {
                            wires.Add((IPowerTransferer)ipc);
                        }
                    }
                }
                );
            i++;
        }
        return powerables.ToList();
    }

    private void generatePowerConduitList()
    {
        powerConduits.Clear();
        powerConduits.AddRange(
            FindObjectsOfType<GameObject>().ToList()
           .FindAll(go => go.GetComponent<IPowerConduit>() != null)
           .ConvertAll(go => go.GetComponent<IPowerConduit>())
        );
    }

    public void generateConnectionMap()
    {
        Debug.Log("=== Generating Connection Map ===");
        generatePowerConduitList();
        connectionMap.Clear();
        List<IPowerer> powerers = powerConduits
           .FindAll(ipc => ipc is IPowerer)
           .ConvertAll(ipc => (IPowerer)ipc);
        //List<IPowerConduit> conduits = FindObjectsOfType<GameObject>().ToList()
        //   .FindAll(go => go.GetComponent<IPowerConduit>() != null)
        //   .ConvertAll(go => go.GetComponent<IPowerConduit>());
        Debug.Log("Found powerers: " + powerers.Count);
        powerers.ForEach(ipr =>
        {
            generateConnections(ipr);//, conduits);
        });
    }
    private void generateConnections(IPowerConduit ipc)//, List<IPowerConduit> allConduits)
    {
        //Don't process a conduit twice
        if (connectionMap.ContainsKey(ipc))
        {
            return;
        }
        //Get list of connecting conduits
        List<IPowerConduit> connectingConduits = getConnectingConduits(ipc);
        //Connect this conduit with a connecting one
        connectingConduits.ForEach(cc => addConnection(ipc, cc));
        //Connect further
        connectingConduits.ForEach(cc => generateConnections(cc));
    }
    private void addConnection(IPowerConduit node, IPowerConduit neighbor)
    {
        if (!connectionMap.ContainsKey(node))
        {
            connectionMap.Add(node, new HashSet<IPowerConduit>());
        }
        connectionMap[node].Add(neighbor);
    }



    public List<IPowerConduit> getConnectingConduits(IPowerConduit ipc)
    {
        Collider2D coll2d = ipc.GameObject.GetComponent<Collider2D>();
        Collider2D[] colls = new Collider2D[70];//[Utility.MAX_HIT_COUNT];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        int count = coll2d.OverlapCollider(filter, colls);
        //Utility.checkMaxReturnedList("getConnectingConduits", count);
        List<IPowerConduit> conduits = new List<IPowerConduit>();
        for (int i = 0; i < count; i++)
        {
            IPowerConduit conduit = colls[i].gameObject.GetComponent<IPowerConduit>();
            if (conduit != null)
            {
                conduits.Add(conduit);
            }
        }
        return conduits;
    }
}
