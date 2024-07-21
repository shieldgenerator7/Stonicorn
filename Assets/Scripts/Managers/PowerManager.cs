using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PowerManager : MonoBehaviour
{
    private Dictionary<IPowerConduit, HashSet<IPowerConduit>> connectionMap
        = new Dictionary<IPowerConduit, HashSet<IPowerConduit>>();
    private List<IPowerConduit> powerConduits = new List<IPowerConduit>();

    private struct PowerPath
    {
        public IPowerable powerable;
        public List<IPowerTransferer> path;
        public PowerPath(IPowerable powerable, List<IPowerTransferer> path)
        {
            this.powerable = powerable;
            this.path = path;
        }
    }
    List<KeyValuePair<IPowerer, List<PowerPath>>> powerPaths = new List<KeyValuePair<IPowerer, List<PowerPath>>>();
    List<IPowerable> noPowerPowerables = new List<IPowerable>();

    private void Start()
    {
        generateConnectionMap();
    }

    private void FixedUpdate()
    {
        //Reset all power wires
        powerConduits
              .FindAll(ipc => ipc is PowerWire)
              .ConvertAll(ipc => (PowerWire)ipc)
              .ForEach(pw => pw.reset());
        //Process powerables with no power
        noPowerPowerables.ForEach(pwr => pwr.acceptPower(0));
        //Have powerers dish out their power
        powerPaths.ForEach(
            entry =>
            {
                IPowerer ipr = entry.Key;
                float powerToEach = ipr.ThroughPut * Time.fixedDeltaTime / entry.Value.Count;
                entry.Value.ForEach(path =>
                {
                    IPowerable pwr = path.powerable;
                    ipr.givePower(
                        -pwr.acceptPower(ipr.givePower(powerToEach))
                        );
                    path.path.ForEach(wire => wire.transferPower(powerToEach));
                });
            });
    }

    private List<PowerPath> getPowerPaths(IPowerer source)
    {
        List<PowerPath> paths = new List<PowerPath>();
        getPowerables(source).ForEach(pwr =>
        {
            paths.Add(new PowerPath(
                pwr,
                getTransferPath(source, pwr)
                ));
        });
        return paths;
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
                    if (ipc is IPowerable && ipc.GameObject != source.GameObject)
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

    /// <summary>
    /// Tells the wires between these two conduits that power is transferring through them
    /// </summary>
    /// <param name="ipr"></param>
    /// <param name="pwr"></param>
    List<IPowerTransferer> getTransferPath(IPowerer source, IPowerable usage)
    {
        Stack<IPowerTransferer> path = new Stack<IPowerTransferer>();
        Stack<IPowerTransferer> stack = new Stack<IPowerTransferer>(
            connectionMap[source].ToList()
            .FindAll(ipc => ipc is IPowerTransferer)
            .ConvertAll(ipc => (IPowerTransferer)ipc)
            );
        HashSet<IPowerTransferer> tried = new HashSet<IPowerTransferer>();
        bool foundPath = false;
        IPowerTransferer currentNode;
        while (!foundPath)
        {
            currentNode = stack.Pop();
            path.Push(currentNode);
            tried.Add(currentNode);
            bool foundBranch = false;
            connectionMap[currentNode].ToList().ForEach(
                ipc =>
                {
                    if (ipc is IPowerable && ipc == usage)
                    {
                        foundPath = true;
                    }
                    if (ipc is IPowerTransferer)
                    {
                        if (!tried.Contains(ipc))
                        {
                            foundBranch = true;
                            stack.Push((IPowerTransferer)ipc);
                        }
                    }
                }
                );
            if (!foundBranch && !foundPath)
            {
                path.Pop();
            }
        }
        //Return the found wires
        return path.ToList();
    }

    private void generatePowerConduitList()
    {
        powerConduits.Clear();
        powerConduits.AddRange(
            FindObjectsByType<GameObject>(FindObjectsSortMode.None).ToList()
           .ConvertAll(go => go.GetComponent<IPowerConduit>())
           .FindAll(ipc=>ipc!= null)
        );
    }

    public void generateConnectionMap()
    {
        generatePowerConduitList();
        connectionMap.Clear();
        powerPaths.Clear();
        List<IPowerer> powerers = powerConduits
           .FindAll(ipc => ipc is IPowerer)
           .ConvertAll(ipc => (IPowerer)ipc);
        noPowerPowerables = powerConduits
           .FindAll(ipc => ipc is IPowerable)
           .ConvertAll(ipc => (IPowerable)ipc);
        Dictionary<IPowerer, List<PowerPath>> dictPaths = new Dictionary<IPowerer, List<PowerPath>>();
        powerers.ForEach(ipr =>
        {
            generateConnections(ipr);
            List<PowerPath> paths = new List<PowerPath>();
            paths.AddRange(getPowerPaths(ipr));
            dictPaths.Add(ipr, paths);
            //Remove its powerable from the non powered list
            paths.ForEach(path => noPowerPowerables.Remove(path.powerable));
        });
        powerPaths = dictPaths.ToList();

    }
    private void generateConnections(IPowerConduit ipc)
    {
        //Don't process a conduit twice
        if (connectionMap.ContainsKey(ipc))
        {
            return;
        }
        else
        {
            connectionMap.Add(ipc, new HashSet<IPowerConduit>());
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
        Collider2D[] colls = new Collider2D[Utility.MAX_HIT_COUNT];
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        int count = coll2d.Overlap(filter, colls);
        Utility.checkMaxReturnedList("getConnectingConduits", count);
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
