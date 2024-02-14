using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Doggo
{

    Dictionary<DoggoPhase, DoggoPhaseAttributes> mPhases;
    DoggoPhaseAttributes currPhase;

    float phaseStartedAt;
    float currPhaseLimit;

    System.Random rnd;

    public Doggo(Dictionary<DoggoPhase, DoggoPhaseAttributes> phases, DoggoPhase init, float time)
    {
        mPhases = phases;
        rnd = new System.Random();

        phaseStartedAt = time;
        currPhase = mPhases[init];

        float min = currPhase.stayRange.Item1;
        float max = currPhase.stayRange.Item2;
        currPhaseLimit = (float)rnd.NextDouble() * (max - min) + min;
        
    }

    public enum DoggoPhase
    {
        DISTRACTED, // can only go to playful
        PLAYFUL, // can go to either distracted or beast.
        BEAST // can only go to playful.
    };

    public struct DoggoPhaseAttributes
    {
        public DoggoPhase phase;
        public float pEnter;
        public int damage;
        public (float, float) stayRange;

        public float pullSpeedFactor;
        public float shakeDampingFactor;
    }

    public void step(float timeNow)
    {
        float elapsed = timeNow - phaseStartedAt;
        if (elapsed < currPhaseLimit) return;

        if (currPhase.phase == DoggoPhase.DISTRACTED)
        {
            currPhase = mPhases[DoggoPhase.PLAYFUL];
        } else if (currPhase.phase == DoggoPhase.BEAST)
        {
            currPhase = mPhases[DoggoPhase.PLAYFUL];
        }
        else
        {
            // toss coin.
            float pBeast = mPhases[DoggoPhase.BEAST].pEnter;
            float coin = (float) rnd.NextDouble();
            if (coin < pBeast)
            {
                currPhase = mPhases[DoggoPhase.BEAST];
            } else
            {
                currPhase = mPhases[DoggoPhase.DISTRACTED];
            }
        }


        phaseStartedAt = timeNow;
        float min = currPhase.stayRange.Item1;
        float max = currPhase.stayRange.Item2;
        currPhaseLimit = (float)rnd.NextDouble() * (max - min) + min;
    }

    public int getDamage()
    {
        return currPhase.damage;
    }

    public DoggoPhaseAttributes getCurrPhase()
    {
        return currPhase;
    }
}

public class PaperMover : MonoBehaviour
{
    [SerializeField] float speed = 1.0f; //how fast it shakes
    [SerializeField] float amount = 1.0f; //how much it shakes
    Vector3 startPos;

    int paperDurability = 100;
    Doggo dog;

    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.position;

        Dictionary<Doggo.DoggoPhase, Doggo.DoggoPhaseAttributes> phases = new Dictionary<Doggo.DoggoPhase, Doggo.DoggoPhaseAttributes>();
        phases[Doggo.DoggoPhase.DISTRACTED] = new Doggo.DoggoPhaseAttributes { phase = Doggo.DoggoPhase.DISTRACTED, damage = 0, pEnter = 0.5f, stayRange = (0.5f, 1.5f), pullSpeedFactor = 0f, shakeDampingFactor = 0f, };
        phases[Doggo.DoggoPhase.PLAYFUL] = new Doggo.DoggoPhaseAttributes { phase = Doggo.DoggoPhase.PLAYFUL, damage = 3, pEnter = 1f, stayRange = (1, 3), pullSpeedFactor = 1f, shakeDampingFactor=0.5f,};
        phases[Doggo.DoggoPhase.BEAST] = new Doggo.DoggoPhaseAttributes { phase = Doggo.DoggoPhase.BEAST, damage = 15, pEnter = 0.5f, stayRange = (1, 2), pullSpeedFactor = 0.5f, shakeDampingFactor=1f,};

        dog = new Doggo(phases, Doggo.DoggoPhase.DISTRACTED, Time.time);
        lastHealthHitWhen = Time.time;
    }

    float lastHealthHitWhen;
    const float healthHitOftenSeconds = 0.3f;

    // Update is called once per frame
    void Update()
    {
        dog.step(Time.time);
        Doggo.DoggoPhaseAttributes currAttrs = dog.getCurrPhase();

        float xShake = startPos.x + Mathf.Sin(Time.time * speed) * currAttrs.shakeDampingFactor;
        float dogPull = 0.005f * currAttrs.pullSpeedFactor;
        float playerPull = -0.01f;

        if (Time.time - lastHealthHitWhen > healthHitOftenSeconds)
        {
            lastHealthHitWhen = Time.time;
            paperDurability -= dog.getDamage();
            Debug.Log("Paper health: " + paperDurability);
        }

        //end game  
        if(paperDurability <= 0) {
            FindObjectOfType<GameManager>().EndGame();
        }


        float currZ = transform.position.z;
        currZ += dogPull;

        if(Input.GetKey(KeyCode.Space)) {
            currZ += playerPull;
        }

        transform.position = new Vector3(xShake,0,currZ);
        
    }
}
