using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public ExcitementBar excitementBar;
    public HealthBar healthBar;
    public PhaseUI phaseState;
    public ExcitementIndicator excitementIndicator;
    [SerializeField] float speed = 1.0f; //how fast it shakes
    [SerializeField] float amount = 1.0f; //how much it shakes
    Vector3 startPos;

    [SerializeField] float playerPull = -0.008f; // how much paper is pulled when player presses space.
    [SerializeField] float dogPull = 0.003f;

    [SerializeField] float dogExcitmentAddScale_ = 0.01f;
    [SerializeField] float dogExcitmentReductionScale_ = -0.01f;

    const float PHYSICS_TICK = 0.01f;


    struct LevelAttributes
    {
        public float dogExcitedThresholdStart; // vertical line. Beyond this, the dog will enter beast mode.
        public float dogExcitmentAddScale; // how much dog excitement is added everytime space is pressed.
        public float dogExcitmentReductionScale; // how much dog excitment drops everytime space is not pressed. 2 is to 1 means it drops twice as quickly as it builds up

        // TODO: Excitment can increase if player keeps space pressed for a long time.
        // Complete: once player enters phase, make it difficult to exit, by reducing threshold by random amount. so they can't play around it.

        // every so often, a random number will be picked between range.
        // the range will dynamically lerp to that number. (NOT DOING THIS RN). We immediately update it.
        // when it reaches that number, it will stay there for some time.
        // then a new number will be picked.
        public (int, int) excitedThresholdRanges_forLevel;
        public float timeToChangeThreshold_seconds; // not being used
        public (int, int) timeToStayAtThreshold_seconds;


        public DoggoPhaseAttributes[] dogPhaseAttributes;

        public float maxHealth;
        public float gracePeriodBeforeBeastDamage; // Completed: Allow 50ms delay in beast mode before destroying heart.
        public float beastModeFaultDamage; 
        public float constantHealthLoss;
    }

    struct ThresholdChangingState
    {
        public bool ongoing;
        public float whenStarted;
        public float expectedEndTime;
        public float from;
        public float to;
    }

    private ThresholdChangingState constructFalseThresholdChangingState()
    {
        return new ThresholdChangingState{
            ongoing = false,
        };
    }

    private DoggoPhaseAttributes[] constructDefaultDoggoPhaseAttributes()
    {
        return new DoggoPhaseAttributes[] {
                new DoggoPhaseAttributes { pullSpeedFactor = 0f, shakeDampingFactor = 0f }, // distracted
                new DoggoPhaseAttributes { pullSpeedFactor = 1.2f, shakeDampingFactor = 0.1f }, // playful
                new DoggoPhaseAttributes { pullSpeedFactor = 0.4f, shakeDampingFactor = 0.8f }, // beast
            };
    }

    struct StateAttributes
    {
        public float paperHealth;
        public float lastHealthHitWhen;
        public float dogExcitment; // meter. how excited is the dog currently?
        public float dogExcitmentThreshold;
        public DoggoPhase dogPhase;

        public float nextThresholdChange; // at what time should we start changing the threshold;
        ThresholdChangingState thresholdChangingState;

        public float whenExitDistracted; // if distracted, when to exit the mode. Should be set when distracted is enabled.

        public float lastPhysicsTick; // instead of increasing dog excitment every frame, we should do it at fixed timestemps.
    }

    LevelAttributes lvl;
    StateAttributes state;
    System.Random rnd;

    private float calculateConstantHealthLossGivenPlayTime(float playTimeInSeconds, int maxHealth)
    {
        float totalTicks = playTimeInSeconds / PHYSICS_TICK;
        return 0;
    }

    void Lvl1()
    {
        // sleepy doggo. doesn't do anything. baby tutorial.
        Debug.Log("level 1! sleepy doggo. doesn't do anything. baby tutorial.");
        lvl = new LevelAttributes
        {
            dogExcitedThresholdStart = 100,
            dogExcitmentAddScale = 0f,
            dogExcitmentReductionScale = 0f,

            excitedThresholdRanges_forLevel = (100, 100),
            timeToChangeThreshold_seconds = 10000,
            timeToStayAtThreshold_seconds = (10000, 10000),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 10000,
            beastModeFaultDamage = 0,
            constantHealthLoss = 0,
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 100f,
            dogPhase = DoggoPhase.DISTRACTED,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl2()
    {
        // show that pulling increasing doggo excitment.

        Debug.Log("level 2! show that pulling increasing doggo excitment.");
        lvl = new LevelAttributes
        {
            dogExcitedThresholdStart = 100f,
            dogExcitmentAddScale = 1f,
            dogExcitmentReductionScale = -1.5f,

            excitedThresholdRanges_forLevel = (100, 100),
            timeToChangeThreshold_seconds = 10000,
            timeToStayAtThreshold_seconds = (10000, 10000),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 10000,
            beastModeFaultDamage = 0,
            constantHealthLoss = 0,
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 101f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl3()
    {
        // play w/ playful dog w/ decreasing health.
        Debug.Log("level 3! play w/ playful dog w/ decreasing health.");
        lvl = new LevelAttributes
        {
            dogExcitedThresholdStart = 100f,
            dogExcitmentAddScale = 1f,
            dogExcitmentReductionScale = -1.5f,

            excitedThresholdRanges_forLevel = (100, 100),
            timeToChangeThreshold_seconds = 10000,
            timeToStayAtThreshold_seconds = (10000, 10000),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 10000,
            beastModeFaultDamage = 0,
            constantHealthLoss = 0.1f,
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 101f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            whenExitDistracted = Time.time + 100000,
        };
    }



        //lvl = new LevelAttributes
        //{
        //    dogExcitedThreshold = 50,
        //    excitedThresholdRanges_forLevel = (60, 80),
        //    timeToChangeThreshold_seconds = 2,
        //    timeToStayAtThreshold_seconds = 2,
        //    dogPhaseAttributes = new DoggoPhaseAttributes[] {
        //        new DoggoPhaseAttributes { pullSpeedFactor = 0f, shakeDampingFactor = 0f }, // distracted
        //        new DoggoPhaseAttributes { pullSpeedFactor = 1.2f, shakeDampingFactor = 0.1f }, // playful
        //        new DoggoPhaseAttributes { pullSpeedFactor = 0.4f, shakeDampingFactor = 0.8f }, // beast
        //    },
        //    gracePeriodBeastDamage = 0.2f,
        //    constantHealthLoss = 0.1f,
        //};

        //state = new StateAttributes
        //{
        //    dogExcitment = 20,
        //    playerHearts = 1000,
        //    lastHealthHitWhen = Time.time - 5,
        //    dogPhase = DoggoPhase.PLAYFUL,
        //    nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
        //    whenExitDistracted = 0, // set from external function call when we trigger a distracted state.
        //};

        //excitementBar.SetMaxExcitement(lvl.dogExcitedThreshold);
        //excitementBar.SetExcitement(state.dogExcitment);
        //healthBar.SetMaxHealth(state.playerHearts);
        //setDoggoPhaseUI(state.dogPhase);
        //excitementIndicator.updateVerticalLinePosition(0.8f);
    //}

    void setDoggoPhaseUI (DoggoPhase currPhase) {
        if(currPhase == DoggoPhase.BEAST) {
            phaseState.setPhase("BEAST!!!");        
        }
        else if(currPhase == DoggoPhase.PLAYFUL) {
            phaseState.setPhase("playful!");
        }
        else {
            phaseState.setPhase("distracted!");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.position;
        rnd = new System.Random();

        lvl = new LevelAttributes();
        state = new StateAttributes();

        Scene currentScene = SceneManager.GetActiveScene();

        if(currentScene.name == "Level01") {
            Lvl1();
        }
        else if(currentScene.name == "Level02") {
            Lvl2();
        } else if (currentScene.name == "Level03")
        {
            Lvl3();
        }
        else {
            Debug.Log(currentScene.name);
            Lvl2();
            //FindObjectOfType<GameManager>().EndGame(); uncomment to see death animation on third scene
        }

        FindObjectOfType<GameManager>().playMusic();
        UpdateUI();
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

    public void UpdateUI()
    {
        excitementBar.SetMaxExcitement(100f);
        excitementBar.SetExcitement(state.dogExcitment);
        healthBar.SetMaxHealth((int)lvl.maxHealth);
        healthBar.SetHealth((int)state.paperHealth);
        setDoggoPhaseUI(state.dogPhase);
        excitementIndicator.updateVerticalLinePosition(state.dogExcitmentThreshold / 100.0f);
    }

    // Update is called once per frame

    int iFrame = 0;
    void Update()
    {
        iFrame += 1;
        if (iFrame % 10 == 0)
        {
            Debug.Log("Health:" + state.paperHealth + " threshold:" + state.dogExcitmentThreshold + " excitment:" + state.dogExcitment + " phase:" + state.dogPhase);
        }

        DoggoPhaseAttributes dogAttrs = lvl.dogPhaseAttributes[phaseIdx(state.dogPhase)];

        // have the dog pull the paper based on its phase.
        float currZ = transform.position.z;
        currZ += dogPull * dogAttrs.pullSpeedFactor;
        float xShake = startPos.x + Mathf.Sin(Time.time * speed) * dogAttrs.shakeDampingFactor;

        bool tick = since(state.lastPhysicsTick) > PHYSICS_TICK ? true : false;
        if (tick)
        {
            state.lastPhysicsTick = Time.time;
            state.paperHealth -= lvl.constantHealthLoss;
        }

        bool spacePressed = Input.GetKey(KeyCode.Space);
        if (!spacePressed) {
            if (tick && state.dogPhase != DoggoPhase.DISTRACTED) {
                state.dogExcitment += lvl.dogExcitmentReductionScale;
                state.dogExcitment = Mathf.Clamp(state.dogExcitment, 0, 99);
            }

            // player successfully escapes the mode.
            if (state.dogPhase == DoggoPhase.BEAST && state.dogExcitment < state.dogExcitmentThreshold)
            {
                state.dogPhase = DoggoPhase.PLAYFUL;

                // give players breathing room.
                int min = 4;
                int max = 14;
                state.dogExcitmentThreshold += (float)rnd.NextDouble() * (max - min) + min;
            }

            transform.position = new Vector3(xShake, 0, currZ);
            UpdateUI();
            return;
        }

        // space Pressed!
        currZ += playerPull;

        
        if (state.dogPhase == DoggoPhase.BEAST && since(state.lastHealthHitWhen) > lvl.gracePeriodBeforeBeastDamage)
        {
            state.paperHealth -= lvl.beastModeFaultDamage;
            state.lastHealthHitWhen = Time.time;
            Debug.Log("Ouch! Health decreased. Hearts: " + state.paperHealth);
        }

        //end game  
        if (state.paperHealth <= 0)
        {
            FindObjectOfType<GameManager>().EndGame();
            return;
        }
        transform.position = new Vector3(xShake,0,currZ);

        if (tick && state.dogPhase != DoggoPhase.DISTRACTED)
        {
            state.dogExcitment += lvl.dogExcitmentAddScale;
            state.dogExcitment = Mathf.Clamp(state.dogExcitment, 0, 99);
        }

        if (state.dogPhase == DoggoPhase.PLAYFUL && state.dogExcitment >= state.dogExcitmentThreshold)
        {
            state.dogPhase = DoggoPhase.BEAST;
            FindObjectOfType<GameManager>().playGrowls();

            // immediately increase dogExcitment by some amount, so player has to suffer for few frames before getting it back.
            int min = 5;
            int max = 13;
            state.dogExcitmentThreshold -= (float)rnd.NextDouble() * (max - min) + min;
            state.dogExcitmentThreshold = Mathf.Clamp(state.dogExcitment, 30, 100);

            state.lastHealthHitWhen = Time.time; // give them 0.5s to react.
        }

        if (state.dogPhase == DoggoPhase.PLAYFUL && state.dogExcitment * 1.2 > state.dogExcitmentThreshold)
        {
            Debug.Log("About to enter beast mode!");
            //add eye animation here!
        }

        if (state.dogPhase == DoggoPhase.DISTRACTED && since(state.whenExitDistracted) > 0)
        {
            state.dogPhase = DoggoPhase.PLAYFUL;
            setDoggoPhaseUI(state.dogPhase);
        }

        

        // perhaps unnecessary?
        if (state.nextThresholdChange - Time.time < 0)
        {
            Debug.Log("AUTO CHANGING THRESHOLD!");
            int min = lvl.excitedThresholdRanges_forLevel.Item1;
            int max = lvl.excitedThresholdRanges_forLevel.Item2;
            state.dogExcitmentThreshold = (float)rnd.NextDouble() * (max - min) + min;

            min = lvl.timeToStayAtThreshold_seconds.Item1;
            max = lvl.timeToStayAtThreshold_seconds.Item2;
            float secStayAtThreshold = (float)rnd.NextDouble() * (max - min) + min;
            state.nextThresholdChange = Time.time + secStayAtThreshold;
        }

        UpdateUI();
    }
}
