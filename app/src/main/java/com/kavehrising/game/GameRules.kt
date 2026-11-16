package com.kavehrising.game

import kotlin.math.abs

/**
 * Small, deterministic rules shared by the game and its JVM unit tests.
 * Keeping combat arithmetic here makes balancing safer without adding game-engine dependencies.
 */
object GameRules {
    const val PLAYER_MAX_HEALTH = 3
    const val NORMAL_ATTACK_DAMAGE = 1
    const val HEAVY_ATTACK_DAMAGE = 3
    const val NORMAL_ATTACK_RANGE = 0.23f
    const val HEAVY_ATTACK_RANGE = 0.30f
    const val ENEMY_CONTACT_RANGE = 0.19f

    const val NORMAL_ATTACK_COOLDOWN_MS = 600L
    const val HEAVY_ATTACK_COOLDOWN_MS = 8_000L
    const val DODGE_COOLDOWN_MS = 1_800L
    const val DODGE_INVULNERABILITY_MS = 650L

    fun isInRange(first: Float, second: Float, range: Float): Boolean =
        abs(first - second) < range.coerceAtLeast(0f)

    fun applyDamage(currentHealth: Int, damage: Int): Int =
        (currentHealth.coerceAtLeast(0) - damage.coerceAtLeast(0)).coerceAtLeast(0)

    fun enemyStepInterval(chapterIndex: Int): Long =
        (1_650L - chapterIndex.coerceAtLeast(0) * 90L).coerceAtLeast(1_200L)

    fun isSpecialEnemyStrike(chapterIndex: Int, turnCount: Int): Boolean =
        chapterIndex >= 2 && turnCount > 0 && turnCount % 3 == 0

    fun chapterReward(enemyMaxHealth: Int): Int = enemyMaxHealth.coerceAtLeast(0) * 100

    fun clampPosition(position: Float): Float = position.coerceIn(0.08f, 0.92f)
}
