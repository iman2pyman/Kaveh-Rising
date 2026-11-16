package com.kavehrising.game

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertTrue
import org.junit.Test

class GameRulesTest {
    @Test fun damageNeverDropsBelowZeroOrBecomesNegative() {
        assertEquals(2, GameRules.applyDamage(3, 1))
        assertEquals(0, GameRules.applyDamage(1, 3))
        assertEquals(0, GameRules.applyDamage(0, 2))
        assertEquals(3, GameRules.applyDamage(3, -5))
    }

    @Test fun heavyAttackDealsMoreDamageThanNormalAttack() {
        assertTrue(GameRules.HEAVY_ATTACK_DAMAGE > GameRules.NORMAL_ATTACK_DAMAGE)
        assertEquals(0, GameRules.applyDamage(3, GameRules.HEAVY_ATTACK_DAMAGE))
    }

    @Test fun laterChaptersIncreasePressureButKeepFairIntervalFloor() {
        assertTrue(GameRules.enemyStepInterval(5) < GameRules.enemyStepInterval(0))
        assertEquals(1_200L, GameRules.enemyStepInterval(20))
        assertEquals(1_650L, GameRules.enemyStepInterval(0))
    }

    @Test fun specialStrikeHappensEveryThirdTurnFromMountainChapter() {
        assertFalse(GameRules.isSpecialEnemyStrike(1, 3))
        assertFalse(GameRules.isSpecialEnemyStrike(2, 2))
        assertTrue(GameRules.isSpecialEnemyStrike(2, 3))
        assertTrue(GameRules.isSpecialEnemyStrike(5, 6))
    }

    @Test fun combatRangeAndPositionBoundsAreConsistent() {
        assertTrue(GameRules.isInRange(0.2f, 0.4f, GameRules.HEAVY_ATTACK_RANGE))
        assertFalse(GameRules.isInRange(0.2f, 0.5f, GameRules.NORMAL_ATTACK_RANGE))
        assertEquals(0.08f, GameRules.clampPosition(-2f), 0.0001f)
        assertEquals(0.92f, GameRules.clampPosition(2f), 0.0001f)
    }

    @Test fun chapterRewardsScaleWithEnemyStrength() {
        assertEquals(500, GameRules.chapterReward(5))
        assertEquals(1_500, GameRules.chapterReward(15))
        assertEquals(0, GameRules.chapterReward(-2))
    }

    @Test fun chapterDataContainsSixProgressivelyHarderChapters() {
        assertEquals(6, ChapterData.chapters.size)
        assertTrue(ChapterData.chapters.zipWithNext().all { (a, b) -> b.enemyHealth > a.enemyHealth })
        assertTrue(ChapterData.chapters.all { it.titleFa.isNotBlank() && it.storyFa.isNotBlank() })
    }
}
