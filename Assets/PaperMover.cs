using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct DoggoPhaseAttributes
{
    public float pullSpeedFactor;
    public float shakeDampingFactor;
}

enum DoggoPhase
{
    DISTRACTED, // can only go to playful
    PLAYFUL, // can go to either distracted or beast.
    BEAST // can only go to playful.
};

public class PaperMover : MonoBehaviour
{
    [SerializeField] float speed = 1.0f; //how fast it shakes
    [SerializeField] float amount = 1.0f; //how much it shakes
    Vector3 startPos;

    [SerializeField] float playerPull = -0.008f; // how much paper is pulled when player presses space.
    [SerializeField] float dogPull = 0.003f;

    [SerializeField] float dogExcitmentAddScale = 0.01f; // how much dog excitement is added everytime space is pressed.
    [SerializeField] float dogExcitmentReductionScale = -0.01f; // how much dog excitment drops everytime space is not pressed. 2 is to 1 means it drops twice as quickly as it builds up. 

    // TODO: Excitment can increase if player keeps space pressed for a long time.
    // Completed: Allow 50ms delay in beast mode before destroying heart.
    // Complete: once player enters phase, make it difficult to exit, by reducing threshold by random amount. so they can't play around it.

    struct LevelAttributes
    {
        public float dogExcitedThreshold; // hidden meter. Beyond this, the dog will enter beast mode.
    
        public DoggoPhaseAttributes[] dogPhaseAttributes;

        // every so often, a random number will be picked between range.
        // the range will dynamically lerp to that number. (NOT DOING THIS RN). We immediately update it.
        // when it reaches that number, it will stay there for some time.
        // then a new number will be picked.
        public (int, int) excitedThresholdRanges_forLevel;
        public float timeToChangeThreshold_seconds; // not being used
        public float timeToStayAtThreshold_seconds;

        public float gracePeriodBeastDamage;
    }

    struct StateAttributes
    {
        public int playerHearts;
        public float lastHealthHitWhen;
        public float dogExcitment; // hidden meter. how excited is the dog currently?
        public DoggoPhase dogPhase;
        public float nextThresholdChange; // at what time should we start changing the threshold;
        public float whenExitDistracted; // if distracted, when to exit the mode. Should be set when distracted is enabled.

        public float lastPhysicsTick; // instead of increasing dog excitment every frame, we should do it at fixed timestemps.
    }

    LevelAttributes lvl;
    StateAttributes state;
    System.Random rnd;

    void Lvl1()
    {
        lvl = new LevelAttributes
        {
            dogExcitedThreshold = 100,
            excitedThresholdRanges_forLevel = (0, 0),
            timeToChangeThreshold_seconds = 2,
            timeToStayAtThreshold_seconds = 2,
            dogPhaseAttributes = new DoggoPhaseAttributes[] {
                new DoggoPhaseAttributes { pullSpeedFactor = 0f, shakeDampingFactor = 0f }, // distracted
                new DoggoPhaseAttributes { pullSpeedFactor = 1.2f, shakeDampingFactor = 0.1f }, // playful
                new DoggoPhaseAttributes { pullSpeedFactor = 0.4f, shakeDampingFactor = 0.8f }, // beast
            },
            gracePeriodBeastDamage = 0.2f,
        };

        state = new StateAttributes
        {
            dogExcitment = 0,
            playerHearts = 5,
            lastHealthHitWhen = Time.time - 5,
            dogPhase = DoggoPhase.DISTRACTED,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            whenExitDistracted = Time.time + 100000,
        };
    }

    // TODO: we can add these for each level.
    void Lvl2()
    {
        lvl = new LevelAttributes
        {
            dogExcitedThreshold = 50,
            excitedThresholdRanges_forLevel = (60, 80),
            timeToChangeThreshold_seconds = 2,
            timeToStayAtThreshold_seconds = 2,
            dogPhaseAttributes = new DoggoPhaseAttributes[] {
                new DoggoPhaseAttributes { pullSpeedFactor = 0f, shakeDampingFactor = 0f }, // distracted
                new DoggoPhaseAttributes { pullSpeedFactor = 1.2f, shakeDampingFactor = 0.1f }, // playful
                new DoggoPhaseAttributes { pullSpeedFactor = 0.4f, shakeDampingFactor = 0.8f }, // beast
            },
            gracePeriodBeastDamage = 0.2f,
        };

        state = new StateAttributes
        {
            dogExcitment = 20,
            playerHearts = 5,
            lastHealthHitWhen = Time.time - 5,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            whenExitDistracted = 0, // set from external function call when we trigger a distracted state.
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.position;
        rnd = new System.Random();

        lvl = new LevelAttributes();
        state = new StateAttributes();

        Lvl2();
        //Lvl1();
    }

    float since(float when)
    {
        return Time.time - when;
    }

    int phaseIdx(DoggoPhase phase)
    {
        if (phase == DoggoPhase.DISTRACTED) return 0;
        if (phase == DoggoPhase.PLAYFUL) return 1;
        return 2;
    }

    // Update is called once per frame

    int iFrame = 0;
    void Update()
    {
        iFrame += 1;
        if (iFrame % 10 == 0)
        {
            Debug.Log("Health:" + state.playerHearts + " threshold:" + lvl.dogExcitedThreshold + " excitment:" + state.dogExcitment + " phase:" + state.dogPhase);
        }

        DoggoPhaseAttributes dogAttrs = lvl.dogPhaseAttributes[phaseIdx(state.dogPhase)];

        // have the dog pull the paper based on its phase.
        float currZ = transform.position.z;
        currZ += dogPull * dogAttrs.pullSpeedFactor;
        float xShake = startPos.x + Mathf.Sin(Time.time * speed) * dogAttrs.shakeDampingFactor;

        bool tick = since(state.lastPhysicsTick) > 0.01 ? true : false;
        if (tick)
        {
            state.lastPhysicsTick = Time.time;
        }

        bool spacePressed = Input.GetKey(KeyCode.Space);
        if (!spacePressed) {
            if (tick && state.dogPhase != DoggoPhase.DISTRACTED) {
                state.dogExcitment += dogExcitmentReductionScale;
                state.dogExcitment = Mathf.Clamp(state.dogExcitment, 0, 200);
            }

            // player successfully escapes the mode.
            if (state.dogPhase == DoggoPhase.BEAST && state.dogExcitment < lvl.dogExcitedThreshold)
            {
                state.dogPhase = DoggoPhase.PLAYFUL;

                // give players breathing room.
                int min = 4;
                int max = 14;
                state.dogExcitment -= (float)rnd.NextDouble() * (max - min) + min;
            }

            transform.position = new Vector3(xShake, 0, currZ);
            return;
        }

        // space Pressed!
        currZ += playerPull;
        if (state.dogPhase == DoggoPhase.BEAST && since(state.lastHealthHitWhen) > lvl.gracePeriodBeastDamage)
        {
            state.playerHearts -= 1;
            state.lastHealthHitWhen = Time.time;
            Debug.Log("Ouch! Health decreased. Hearts: " + state.playerHearts);
        }

        //end game  
        if (state.playerHearts == 0)
        {
            FindObjectOfType<GameManager>().EndGame();
            return;
        }
        transform.position = new Vector3(xShake,0,currZ);

        if (tick && state.dogPhase != DoggoPhase.DISTRACTED)
        {
            state.dogExcitment += dogExcitmentAddScale;
            state.dogExcitment = Mathf.Clamp(state.dogExcitment, 0, 200);
        }

        if (state.dogPhase == DoggoPhase.PLAYFUL && state.dogExcitment > lvl.dogExcitedThreshold)
        {
            state.dogPhase = DoggoPhase.BEAST;

            // immediately increase dogExcitment by some amount, so player has to suffer for few frames before getting it back.
            int min = 80;
            int max = 130;
            state.dogExcitment += (float)rnd.NextDouble() * (max - min) + min;
            state.dogExcitment = Mathf.Clamp(state.dogExcitment, 0, 200);

            state.lastHealthHitWhen = Time.time; // give them 0.5s to react.
        }

        if (state.dogPhase == DoggoPhase.PLAYFUL && state.dogExcitment * 1.2 > lvl.dogExcitedThreshold)
        {
            Debug.Log("About to enter beast mode!");
        }

        if (state.dogPhase == DoggoPhase.DISTRACTED && since(state.whenExitDistracted) > 0)
        {
            state.dogPhase = DoggoPhase.PLAYFUL;
        }

        // perhaps unnecessary?
        if (state.nextThresholdChange - Time.time < 0)
        {
            int min = lvl.excitedThresholdRanges_forLevel.Item1;
            int max = lvl.excitedThresholdRanges_forLevel.Item2;
            lvl.dogExcitedThreshold = (float)rnd.NextDouble() * (max - min) + min;
            state.nextThresholdChange = Time.time + lvl.timeToStayAtThreshold_seconds;
        }
    }
}
