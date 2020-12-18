using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeltaTimeTest : MonoBehaviour
{

    class Analyzer
    {
        string name;
        List<float> values = new List<float>();

        public Analyzer(string name)
        {
            this.name = name;
        }

        public void record(float v)
        {
            values.Add(v);
        }

        public float Min => values.Min();

        public float Max => values.Max();

        public float Avg => values.Average();

        /// <summary>
        /// Returns the average without the smallest or largest values
        /// </summary>
        public float AvgCentered
        {
            get
            {
                List<float> vs = values.OrderBy(v => v).ToList();
                vs.RemoveAt(0);
                vs.RemoveAt(vs.Count - 1);
                return vs.Average();
            }
        }

        public void print()
        {
            Debug.Log("==== Analyzer: " + name + " ====");
            Debug.Log("- Min: " + Min);
            Debug.Log("- Max: " + Max);
            Debug.Log("- Avg: " + Avg);
            Debug.Log("- AvC: " + AvgCentered);
        }
    }

    Analyzer deltaAnalyzer = new Analyzer("deltaTime in Update");
    Analyzer fixedAnalyzer = new Analyzer("fixedDeltaTime in FixedUpdate");
    Analyzer deltaAnalyzerReverse = new Analyzer("fixedDeltaTime in Update");
    Analyzer fixedAnalyzerReverse = new Analyzer("deltaTime in FixedUpdate");

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        deltaAnalyzer.record(Time.deltaTime);
        deltaAnalyzerReverse.record(Time.fixedDeltaTime);
    }

    void FixedUpdate()
    {
        fixedAnalyzer.record(Time.fixedDeltaTime);
        fixedAnalyzerReverse.record(Time.deltaTime);
    }

    void OnDisable()
    {
        deltaAnalyzer.print();
        fixedAnalyzer.print();
        deltaAnalyzerReverse.print();
        fixedAnalyzerReverse.print();
    }
}
