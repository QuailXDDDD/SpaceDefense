using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossEnemy : Enemy
{
    [Header("Boss Components")]
    private BossPhaseAttacker phaseAttacker;
    private BossSpecialAbilities specialAbilities;
    
    [Header("Boss Specifics")]
    public float entryMoveDuration = 3f;
    public Vector3 entryTargetPosition = new Vector3(0, 3, 0);
    public float phase1HealthThreshold = 0.7f;
    public float phase2HealthThreshold = 0.3f;

    [Header("Attack Phases")]
    public List<MonoBehaviour> phase1Attacks;
    public List<MonoBehaviour> phase2Attacks;
    public List<MonoBehaviour> phase3Attacks;

    private bool enteredArena = false;
    private int currentPhase = 0;

    protected override void Start()
    {
        base.Start();
        
        phaseAttacker = GetComponent<BossPhaseAttacker>();
        specialAbilities = GetComponent<BossSpecialAbilities>();
        
        currentPhase = 0;
        
        Debug.Log($"Boss: Started with {CurrentHealth}/{enemyData.maxHealth} health. Spawn protection should be active.");
        
        StartCoroutine(EntryMovement());
    }
    
    void OnEnable()
{
        Debug.Log($"Boss: OnEnable called. EnteredArena: {enteredArena}, HasSpawnProtection: {hasSpawnProtection}");
        
        EnsureSpriteVisibility();
        
        if (hasSpawnProtection && !enteredArena)
        {
            RemoveSpawnProtection();
            StartSpawnProtection();
            Debug.Log("Boss: Spawn protection restarted after being re-enabled");
        }
    }
    
    private void EnsureSpriteVisibility()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Debug.Log($"Boss: Checking sprite visibility - Enabled: {sr.enabled}, Sprite: {sr.sprite != null}, Color: {sr.color}, SortingOrder: {sr.sortingOrder}, Material: {sr.material != null}");
            
            sr.enabled = true;
            
            sr.color = Color.white;
            Debug.Log("Boss: Set color to bright white");
            
            sr.sortingOrder = 15;
            Debug.Log("Boss: Set very high sorting order for visibility");
            
            if (sr.sprite == null)
            {
                Debug.LogWarning("Boss: No sprite assigned! Checking enemy data...");
                if (enemyData != null && enemyData.enemySprite != null)
                {
                    sr.sprite = enemyData.enemySprite;
                    Debug.Log("Boss: Assigned sprite from enemy data");
                }
                else
                {
                    Debug.LogError("Boss: No sprite in enemy data either! Creating fallback sprite...");
                    CreateFallbackBossSprite(sr);
                }
            }
            
            if (sr.material == null || sr.material.name.Contains("Lit"))
            {
                sr.material = new Material(Shader.Find("Sprites/Default"));
                Debug.Log($"Boss: Changed material from {(sr.material?.name ?? "null")} to Sprites/Default");
            }
            
            if (transform.localScale.magnitude < 0.5f)
            {
                transform.localScale = Vector3.one;
                Debug.Log("Boss: Fixed too-small scale");
            }
            
            Debug.Log($"Boss: After visibility fixes - Enabled: {sr.enabled}, Sprite: {sr.sprite != null}, Color: {sr.color}, SortingOrder: {sr.sortingOrder}, Scale: {transform.localScale}");
        }
        else
        {
            Debug.LogError("Boss: No SpriteRenderer component found!");
        }
    }
    
    private void CreateFallbackBossSprite(SpriteRenderer sr)
    {
        Texture2D texture = new Texture2D(64, 64);
        
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                Vector2 center = new Vector2(32, 32);
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance < 28)
                {
                    texture.SetPixel(x, y, Color.red);
                }
                else if (distance < 30)
                {
                    texture.SetPixel(x, y, Color.yellow);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        
        texture.Apply();
        Sprite fallbackSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        sr.sprite = fallbackSprite;
        Debug.Log("Boss: Created bright red fallback sprite");
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
        transform.position = entryTargetPosition;
        enteredArena = true;
        Debug.Log("Boss: Entered arena. Activating Phase 1 attacks.");
        ActivatePhase(1);
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
    }

    void ActivatePhase(int phaseNumber)
    {
        Debug.Log($"Boss: Activating Phase {phaseNumber}");

        DeactivateAllAttackComponents();

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

        foreach (MonoBehaviour attackComponent in attacksToEnable)
        {
            if (attackComponent != null)
            {
                attackComponent.enabled = true;
            }
        }
        currentPhase = phaseNumber;
    }

    void DeactivateAllAttackComponents()
    {
        MonoBehaviour[] allAttackComponents = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour comp in allAttackComponents)
        {
            if (comp is BasicAttacker || comp is BurstAttacker || comp is SpreadAttacker)
            {
                comp.enabled = false;
            }
        }
    }

    protected override void Die()
    {
        Debug.Log($"Boss: Die() called! Health: {CurrentHealth}, EnteredArena: {enteredArena}, SpawnProtection: {isSpawnInvincible}");
        Debug.Log("Boss Defeated! Game Over!");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossExplosion();
        }
        
        if (enemyData is BossEnemyData bossData && bossData.deathEffect != null)
        {
            Instantiate(bossData.deathEffect, transform.position, transform.rotation);
        }
        
        base.Die();
    }
    
    void OnDestroy()
    {
        Debug.Log($"Boss: OnDestroy called! Health: {CurrentHealth}, EnteredArena: {enteredArena}");
    }
    
    public override void TakeDamage(int amount)
    {
        Debug.Log($"Boss: TakeDamage called with {amount} damage. Current health: {CurrentHealth}");
        
        if (specialAbilities != null && specialAbilities.ShouldBlockDamage())
        {
            Debug.Log("Boss: Damage blocked by shield!");
            return;
        }
        
        int healthBefore = CurrentHealth;
        
        base.TakeDamage(amount);
        
        if (CurrentHealth < healthBefore && CurrentHealth > 0)
        {
            Debug.Log($"Boss: Took damage! Health: {CurrentHealth}/{enemyData.maxHealth}");
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBossHit();
            }
        }
        else if (CurrentHealth == healthBefore)
        {
            Debug.Log("Boss: No damage taken (likely spawn protection active)");
        }
    }
}
