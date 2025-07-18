using UnityEngine;
using System.Collections; // For coroutines
using System.Collections.Generic; // For List

public class BossEnemy : Enemy
{
    [Header("Boss Components")]
    private BossPhaseAttacker phaseAttacker;
    private BossSpecialAbilities specialAbilities;
    
    [Header("Boss Specifics")]
    public float entryMoveDuration = 3f;
    public Vector3 entryTargetPosition = new Vector3(0, 3, 0); // Where the boss stops
    public float phase1HealthThreshold = 0.7f; // E.g., at 70% health, transition to phase 2
    public float phase2HealthThreshold = 0.3f; // E.g., at 30% health, transition to phase 3

    [Header("Attack Phases")]
    public List<MonoBehaviour> phase1Attacks; // Assign specific attack components here (e.g., BasicAttacker)
    public List<MonoBehaviour> phase2Attacks; // (e.g., BurstAttacker, SpreadAttacker)
    public List<MonoBehaviour> phase3Attacks; // (e.g., all active, more intense)

    private bool enteredArena = false;
    private int currentPhase = 0; // 0 = entry, 1, 2, 3...

    protected override void Start()
    {
        base.Start(); // Call the base Enemy Start method to initialize health and visuals
        
        // Get boss-specific components
        phaseAttacker = GetComponent<BossPhaseAttacker>();
        specialAbilities = GetComponent<BossSpecialAbilities>();
        
        currentPhase = 0;
        StartCoroutine(EntryMovement());
    }

    void Update()
    {
        if (enteredArena)
        {
            CheckPhaseTransitions();
        }
    }

    IEnumerator EntryMovement()
    {
        Debug.Log("Boss: Entering arena...");
        Vector3 startPos = transform.position;
        float timer = 0f;

        while (timer < entryMoveDuration)
        {
            transform.position = Vector3.Lerp(startPos, entryTargetPosition, timer / entryMoveDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = entryTargetPosition; // Ensure it's exactly at the target
        enteredArena = true;
        Debug.Log("Boss: Entered arena. Activating Phase 1 attacks.");
        ActivatePhase(1); // Start Phase 1
    }

    void CheckPhaseTransitions()
    {
        float healthRatio = (float)CurrentHealth / enemyData.maxHealth;

        if (currentPhase == 1 && healthRatio <= phase1HealthThreshold)
        {
            ActivatePhase(2);
        }
        else if (currentPhase == 2 && healthRatio <= phase2HealthThreshold)
        {
            ActivatePhase(3);
        }
        // You can add more phases as needed
    }

    void ActivatePhase(int phaseNumber)
    {
        Debug.Log($"Boss: Activating Phase {phaseNumber}");

        // Deactivate all existing attack components first to ensure clean transition
        DeactivateAllAttackComponents();

        // Update the phase attacker if available
        if (phaseAttacker != null)
        {
            phaseAttacker.UpdatePhase(phaseNumber);
        }

        List<MonoBehaviour> attacksToEnable = new List<MonoBehaviour>();
        if (phaseNumber == 1)
        {
            attacksToEnable.AddRange(phase1Attacks);
        }
        else if (phaseNumber == 2)
        {
            attacksToEnable.AddRange(phase2Attacks);
        }
        else if (phaseNumber == 3)
        {
            attacksToEnable.AddRange(phase3Attacks);
        }
        // Add more phases here

        foreach (MonoBehaviour attackComponent in attacksToEnable)
        {
            if (attackComponent != null)
            {
                attackComponent.enabled = true; // Enable the specific attack script
            }
        }
        currentPhase = phaseNumber; // Update current phase
    }

    void DeactivateAllAttackComponents()
    {
        // Get all MonoBehaviour components that are children of Enemy or are attack types
        // A more robust way might be to tag them or use an interface if you have many
        MonoBehaviour[] allAttackComponents = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour comp in allAttackComponents)
        {
            // Only disable if it's an attack script you manage, not BossEnemy itself or Enemy script
            if (comp is BasicAttacker || comp is BurstAttacker || comp is SpreadAttacker /* Add other attack types here */)
            {
                comp.enabled = false;
            }
        }
    }

    protected override void Die()
    {
        Debug.Log("Boss Defeated! Game Over!");
        
        // Play boss explosion sound effect
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossExplosion();
        }
        
        // Spawn death effect if available
        if (enemyData is BossEnemyData bossData && bossData.deathEffect != null)
        {
            Instantiate(bossData.deathEffect, transform.position, transform.rotation);
        }
        
        // Implement end-game logic, victory screen, etc.
        base.Die(); // Call base Enemy Die method to destroy object
    }
    
    // Override TakeDamage to handle shield
    public override void TakeDamage(int amount)
    {
        // Check if shield is active
        if (specialAbilities != null && specialAbilities.ShouldBlockDamage())
        {
            Debug.Log("Boss: Damage blocked by shield!");
            return;
        }
        
        // Temporarily store the current health to avoid calling enemy hit sound
        int healthBefore = CurrentHealth;
        
        // Call base TakeDamage which handles health calculation and death
        base.TakeDamage(amount);
        
        // Only play boss hit sound if damage was actually taken and boss is still alive
        if (CurrentHealth < healthBefore && CurrentHealth > 0)
        {
            // Play boss hit sound effect instead of the enemy hit sound from base class
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossHit();
            }
        }
    }
}
