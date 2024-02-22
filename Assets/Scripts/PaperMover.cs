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
    BEAST, // can only go to playful.
    INDICATING
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

    float playerPull = -0.0065f; // how much paper is pulled when player presses space.
    float dogPull = 0.002f;

    const float PHYSICS_TICK = 0.01f;
    const float DEFAULT_EXCITMENT_ADD = 0.7f;
    const float DEFAULT_EXCITMENT_REDUCE = -1f;


    struct LevelAttributes
    {
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
        public bool modeEnabledForThisScene;
        public bool ongoing;
        public int ticksLeft;
        public float perTickDelta;
    }

    private ThresholdChangingState constructFalseThresholdChangingState(bool enabledForScene)
    {
        return new ThresholdChangingState {
            ongoing = false,
            modeEnabledForThisScene = enabledForScene,
        };
    }

    private ThresholdChangingState constructThresholdChangingState(float totalChange, float overTimeSec)
    {
        int nTicks = (int) (overTimeSec / PHYSICS_TICK);
        return new ThresholdChangingState
        {
            modeEnabledForThisScene = true,
            ongoing = true,
            ticksLeft = nTicks,
            perTickDelta = totalChange / nTicks
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
        public ThresholdChangingState thresholdChangingState;

        public float whenExitDistracted; // if distracted, when to exit the mode. Should be set when distracted is enabled.

        public float lastPhysicsTick; // instead of increasing dog excitment every frame, we should do it at fixed timestemps.
    }

    LevelAttributes lvl;
    StateAttributes state;
    System.Random rnd;

    private float calculateConstantHealthLossGivenPlayTime(float playTimeInSeconds, float maxHealth)
    {
        float totalTicks = playTimeInSeconds / PHYSICS_TICK;
        return maxHealth / totalTicks;
    }

    void Lvl1()
    {
        // sleepy doggo. doesn't do anything. baby tutorial.
        Debug.Log("level 1! sleepy doggo. doesn't do anything. baby tutorial.");
        lvl = new LevelAttributes
        {
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
            thresholdChangingState = constructFalseThresholdChangingState(false),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl2()
    {
        // show that pulling increasing doggo excitment.

        Debug.Log("level 2! show that pulling increasing doggo excitment.");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE,

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
            thresholdChangingState = constructFalseThresholdChangingState(false),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl3()
    {
        // play w/ playful dog w/ decreasing health.
        Debug.Log("level 3! play w/ playful dog w/ decreasing health.");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE,

            excitedThresholdRanges_forLevel = (100, 100),
            timeToChangeThreshold_seconds = 10000,
            timeToStayAtThreshold_seconds = (10000, 10000),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 10000,
            beastModeFaultDamage = 0,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(12, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 101f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            thresholdChangingState = constructFalseThresholdChangingState(false),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl4()
    {
        // play w/ dog w/ static beast threshold and generous hit beast penalty.
        Debug.Log("level 4! play w/ dog w/ static beast threshold and generous hit beast penalty..");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD*.6f,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE,

            excitedThresholdRanges_forLevel = (40, 80),
            timeToChangeThreshold_seconds = 10000,
            timeToStayAtThreshold_seconds = (10000, 10000),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 0.4f,
            beastModeFaultDamage = 25,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(35, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 70f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + lvl.timeToChangeThreshold_seconds,
            thresholdChangingState = constructFalseThresholdChangingState(false),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl5()
    {
        // play w/ dog w/ changing beast threshold and generous hit beast penalty.
        Debug.Log("level 5! play w/ dog w/ changing beast threshold and generous hit beast penalty..");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD * .9f,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE*1.2f,

            excitedThresholdRanges_forLevel = (40, 80),
            timeToChangeThreshold_seconds = 1,
            timeToStayAtThreshold_seconds = (2, 4),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 0.3f,
            beastModeFaultDamage = 25,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(25, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 60f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + 2,
            thresholdChangingState = constructFalseThresholdChangingState(true),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl6()
    {
        // HARDER!
        Debug.Log("level 6! HARDER!");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD * .9f,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE * 1.2f,

            excitedThresholdRanges_forLevel = (40, 60),
            timeToChangeThreshold_seconds = 0.6f,
            timeToStayAtThreshold_seconds = (1, 3),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 0.3f,
            beastModeFaultDamage = 25,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(35, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 60f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + 2,
            thresholdChangingState = constructFalseThresholdChangingState(true),
            whenExitDistracted = Time.time + 100000,
        };
    }


    void Lvl7()
    {
        // HARDERRR!
        Debug.Log("level 7! HARDERRR!");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD * .9f,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE * 1.2f,

            excitedThresholdRanges_forLevel = (40, 60),
            timeToChangeThreshold_seconds = 0.6f,
            timeToStayAtThreshold_seconds = (1, 3),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 0.1f,
            beastModeFaultDamage = 25,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(20, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 60f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + 2,
            thresholdChangingState = constructFalseThresholdChangingState(true),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl8()
    {
        // Asshole! Constantly changing threshold
        Debug.Log("level 8! Asshole! Constantly changing threshold");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD * .9f,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE,

            excitedThresholdRanges_forLevel = (30, 60),
            timeToChangeThreshold_seconds = 0.3f,
            timeToStayAtThreshold_seconds = (0, 2),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 0.2f,
            beastModeFaultDamage = 20,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(30, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 60f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + 2,
            thresholdChangingState = constructFalseThresholdChangingState(true),
            whenExitDistracted = Time.time + 100000,
        };
    }

    void Lvl9()
    {
        // Impossible asshold
        Debug.Log("Level 9. Impossible asshole.");
        lvl = new LevelAttributes
        {
            dogExcitmentAddScale = DEFAULT_EXCITMENT_ADD * 1.7f,
            dogExcitmentReductionScale = DEFAULT_EXCITMENT_REDUCE*0.8f,

            excitedThresholdRanges_forLevel = (20, 80),
            timeToChangeThreshold_seconds = 0.1f,
            timeToStayAtThreshold_seconds = (0, 1),

            dogPhaseAttributes = constructDefaultDoggoPhaseAttributes(),

            maxHealth = 100,
            gracePeriodBeforeBeastDamage = 0.05f,
            beastModeFaultDamage = 80,
            constantHealthLoss = calculateConstantHealthLossGivenPlayTime(20, 100),
        };

        state = new StateAttributes
        {
            paperHealth = 100,
            lastHealthHitWhen = Time.time - 5,
            dogExcitment = 0,
            dogExcitmentThreshold = 20f,
            dogPhase = DoggoPhase.PLAYFUL,
            nextThresholdChange = Time.time + 2,
            thresholdChangingState = constructFalseThresholdChangingState(true),
            whenExitDistracted = Time.time + 100000,
        };
    }


    void setDoggoPhaseUI (DoggoPhase currPhase) {
        if(currPhase == DoggoPhase.BEAST) {
            phaseState.setPhase("BEAST!!!"); 
            FindObjectOfType<GameManager>().setBeast();     
        }
        else if(currPhase == DoggoPhase.PLAYFUL) {
            phaseState.setPhase("playful!");
            FindObjectOfType<GameManager>().setNormal(); 
        }
        else if(currPhase == DoggoPhase.INDICATING) {
            phaseState.setPhase("CAREFUL!");
            FindObjectOfType<GameManager>().setIndicator(); 
        }
        else {
            phaseState.setPhase("distracted!");
            FindObjectOfType<GameManager>().setNormal(); 
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = -1;
        startPos = this.transform.position;
        rnd = new System.Random();

        lvl = new LevelAttributes();
        state = new StateAttributes();

        Scene currentScene = SceneManager.GetActiveScene();

        switch (currentScene.name)
        {
            case "Level01": Lvl1(); break;
            case "Level02": Lvl2(); break;
            case "Level03": Lvl3(); break;
            case "Level04": Lvl4(); break;
            case "Level05": Lvl5(); break;
            case "Level06": Lvl6(); break;
            case "Level07": Lvl7(); break;
            case "Level08": Lvl8(); break;
            case "Level09": Lvl9(); break;
            default:
                Debug.LogError("Unable to find scene:" + currentScene.name);
                Lvl2();
            break;
        }

        //FindObjectOfType<GameManager>().EndGame(); uncomment to see death animation on third scene

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
        if (phase == DoggoPhase.INDICATING) return 1;
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

    //int iFrame = 0;
    void Update()
    {
        //iFrame += 1;
        //if (iFrame % 10 == 0)
        //{
        //    //Debug.Log("Health:" + state.paperHealth + " threshold:" + state.dogExcitmentThreshold + " excitment:" + state.dogExcitment + " phase:" + state.dogPhase);
        //}

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

        // check if threshold needs to change.
        if (tick && state.thresholdChangingState.modeEnabledForThisScene)
        {
            if (!state.thresholdChangingState.ongoing)
            {
                if (state.nextThresholdChange - Time.time < 0)
                {
                    //Debug.Log("AUTO CHANGING THRESHOLD!");
                    int min = lvl.excitedThresholdRanges_forLevel.Item1;
                    int max = lvl.excitedThresholdRanges_forLevel.Item2;
                    int next =  (int)(rnd.NextDouble() * (max - min) + min);
                    float delta = next - state.dogExcitmentThreshold;

                    state.thresholdChangingState = constructThresholdChangingState(delta, lvl.timeToChangeThreshold_seconds);

                    min = lvl.timeToStayAtThreshold_seconds.Item1;
                    max = lvl.timeToStayAtThreshold_seconds.Item2;
                    float secStayAtThreshold = (float)rnd.NextDouble() * (max - min) + min;
                    state.nextThresholdChange = Time.time + secStayAtThreshold + lvl.timeToChangeThreshold_seconds;
                    state.thresholdChangingState.ongoing = true;
                }
            }

            // important to check again because it will be set to true only if it enters
            // the second if condition above.
            if (state.thresholdChangingState.ongoing)
            {
                state.thresholdChangingState.ticksLeft -= 1;
                state.dogExcitmentThreshold += state.thresholdChangingState.perTickDelta;

                if (state.thresholdChangingState.ticksLeft == 0)
                {
                    //Debug.Log("Completing threshold!");
                    state.thresholdChangingState.ongoing = false;
                }
            }

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
                int min = 15;
                int max = 20;
                state.dogExcitmentThreshold += (float)rnd.NextDouble() * (max - min) + min;
                state.dogExcitmentThreshold = Mathf.Clamp(state.dogExcitmentThreshold, lvl.excitedThresholdRanges_forLevel.Item1, lvl.excitedThresholdRanges_forLevel.Item2);
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
            //Debug.Log("Ouch! Health decreased. Hearts: " + state.paperHealth);
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

        if ((state.dogPhase == DoggoPhase.PLAYFUL || state.dogPhase == DoggoPhase.INDICATING) && state.dogExcitment >= state.dogExcitmentThreshold)
        {
            state.dogPhase = DoggoPhase.BEAST;
            FindObjectOfType<GameManager>().playGrowls();

            // immediately increase dogExcitment by some amount, so player has to suffer for few frames before getting it back.
            int min = 15;
            int max = 20;
            state.dogExcitmentThreshold -= (float)rnd.NextDouble() * (max - min) + min;
            state.dogExcitmentThreshold = Mathf.Clamp(state.dogExcitmentThreshold, lvl.excitedThresholdRanges_forLevel.Item1, lvl.excitedThresholdRanges_forLevel.Item2);

            state.lastHealthHitWhen = Time.time; // give them 0.5s to react.
        }

        if (state.dogPhase == DoggoPhase.PLAYFUL && state.dogExcitment * 1.2 >= state.dogExcitmentThreshold)
        {
            //Debug.Log("About to enter beast mode!");
            //add eye animation here!
            state.dogPhase = DoggoPhase.INDICATING;
        }

        if (state.dogPhase == DoggoPhase.DISTRACTED && since(state.whenExitDistracted) > 0)
        {
            state.dogPhase = DoggoPhase.PLAYFUL;
            setDoggoPhaseUI(state.dogPhase);
        }

        UpdateUI();
    }
}
