package com.kavehrising.game

import android.app.Activity
import android.os.Bundle
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.RectF
import android.view.MotionEvent
import android.view.View
import kotlin.math.abs
import kotlin.math.min

class MainActivity : Activity() {
    private lateinit var gameView: KavehGameView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = Color.rgb(25, 13, 9)
        window.navigationBarColor = Color.rgb(25, 13, 9)
        gameView = KavehGameView(this)
        setContentView(gameView)
    }

    override fun onPause() {
        if (::gameView.isInitialized) gameView.saveGameState()
        super.onPause()
    }

    @Deprecated("Deprecated in Android API 33; retained for compatibility with older devices")
    override fun onBackPressed() {
        if (::gameView.isInitialized && gameView.pauseFromBack()) return
        super.onBackPressed()
    }
}

private class KavehGameView(private val host: Activity) : View(host) {
    private val paint = Paint(Paint.ANTI_ALIAS_FLAG)
    private val prefs = host.getPreferences(Activity.MODE_PRIVATE)
    private var chapterIndex = prefs.getInt("chapter", 0).coerceIn(0, ChapterData.chapters.lastIndex)
    private var score = prefs.getInt("score", 0).coerceAtLeast(0)
    private var bestScore = prefs.getInt("best_score", 0).coerceAtLeast(0)
    private var playerX = prefs.getFloat("player_x", 0.22f).coerceIn(.08f, .92f)
    private var enemyX = prefs.getFloat("enemy_x", 0.78f).coerceIn(.08f, .92f)
    private var health = prefs.getInt("health", 3).coerceIn(0, 3)
    private var enemyHealth = prefs.getInt("enemy_health", ChapterData.chapters[chapterIndex].enemyHealth)
        .coerceIn(0, ChapterData.chapters[chapterIndex].enemyHealth)
    private var enemyTurnCount = prefs.getInt("enemy_turn_count", 0).coerceAtLeast(0)
    private var hitCooldown = 0L
    private var heavyAttackReadyAt = prefs.getLong("heavy_attack_ready_at", 0L)
    private var heavyAttackEffectUntil = 0L
    private var heavyAttackEffectHit = false
    private var attackEffectUntil = 0L
    private var attackEffectHit = false
    private var dodgeCooldownUntil = 0L
    private var dodgeActiveUntil = 0L
    private var playerDamageFlashUntil = 0L
    private var lastEnemyStep = System.currentTimeMillis()
    private var storyIntro = prefs.getBoolean("story_intro", true)
    private var paused = false
    private var potionUsed = prefs.getBoolean("potion_used", false)
    private var gameOver = prefs.getBoolean("game_over", false)
    private var won = prefs.getBoolean("won", false)
    private var message = "کاوه را حرکت بده؛ نزدیک دشمن شو و حمله کن"
    private var messageUntil = System.currentTimeMillis() + 4000L

    fun pauseFromBack(): Boolean {
        if (storyIntro || gameOver || won || paused) return false
        paused = true
        lastEnemyStep = System.currentTimeMillis()
        invalidate()
        return true
    }

    fun saveGameState() {
        prefs.edit()
            .putInt("chapter", chapterIndex)
            .putInt("score", score)
            .putInt("best_score", bestScore)
            .putFloat("player_x", playerX)
            .putFloat("enemy_x", enemyX)
            .putInt("health", health)
            .putInt("enemy_health", enemyHealth)
            .putInt("enemy_turn_count", enemyTurnCount)
            .putLong("heavy_attack_ready_at", heavyAttackReadyAt)
            .putBoolean("story_intro", storyIntro)
            .putBoolean("potion_used", potionUsed)
            .putBoolean("game_over", gameOver)
            .putBoolean("won", won)
            .apply()
    }

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)
        val w = width.toFloat()
        val h = height.toFloat()
        drawWorld(canvas, w, h)
        drawHud(canvas, w, h)
        drawCharacter(canvas, w * playerX, h * .69f, true)
        drawEnemy(canvas, w * enemyX, h * .69f)
        drawAttackEffect(canvas, w, h)
        drawHeavyAttackEffect(canvas, w, h)
        drawDamageFlash(canvas, w, h)
        drawControls(canvas, w, h)

        if (!gameOver && !won && !paused) {
            updateEnemy()
            // A modest refresh rate keeps the simple Canvas game responsive while reducing CPU use.
            postInvalidateDelayed(100)
        }
        if (paused) drawOverlay(canvas, w, h, "بازی متوقف شد", "برای ادامه، صفحه را لمس کن")
        if (gameOver) drawOverlay(canvas, w, h, "کاوه شکست خورد", "برای تلاش دوباره لمس کن")
        if (won) {
            val next = if (chapterIndex < ChapterData.chapters.lastIndex)
                "مرحله بعد: " + ChapterData.chapters[chapterIndex + 1].titleFa + "\nبرای ادامه لمس کن"
            else "درفش کاویانی برافراشته شد!\nلمس کن تا از ابتدا بازی کنی"
            drawOverlay(canvas, w, h, "پیروزی کاوه!", next)
        }
        if (storyIntro && !gameOver && !won) drawStoryOverlay(canvas, w, h)
    }

    private fun drawWorld(canvas: Canvas, w: Float, h: Float) {
        // Each chapter gets a distinct palette and a simple landmark silhouette.
        val sky = when (chapterIndex) {
            0 -> Color.rgb(39, 17, 10)       // forge
            1 -> Color.rgb(32, 22, 19)       // crowded streets
            2 -> Color.rgb(18, 29, 39)       // mountain pass
            3 -> Color.rgb(20, 15, 25)       // dungeon
            4 -> Color.rgb(38, 20, 17)       // uprising
            else -> Color.rgb(24, 18, 35)     // palace
        }
        val ridge = when (chapterIndex) {
            2 -> Color.rgb(54, 73, 88)
            3 -> Color.rgb(45, 35, 55)
            5 -> Color.rgb(67, 42, 58)
            else -> Color.rgb(58, 37, 28)
        }
        val groundColor = when (chapterIndex) {
            2 -> Color.rgb(76, 83, 85)
            3 -> Color.rgb(58, 48, 62)
            4 -> Color.rgb(105, 48, 29)
            5 -> Color.rgb(82, 42, 35)
            else -> Color.rgb(111, 58, 28)
        }
        canvas.drawColor(sky)
        paint.color = ridge
        canvas.drawOval(RectF(-w * .2f, h * .28f, w * .55f, h * .68f), paint)
        canvas.drawOval(RectF(w * .35f, h * .22f, w * 1.2f, h * .67f), paint)
        paint.color = groundColor
        canvas.drawRect(0f, h * .69f, w, h * .82f, paint)
        paint.color = Color.rgb(45, 28, 20)
        canvas.drawRect(0f, h * .82f, w, h, paint)
        paint.color = Color.argb(if (chapterIndex == 0) 90 else 45, 255, if (chapterIndex == 2) 190 else 130, 30)
        canvas.drawCircle(w * .5f, h * .66f, min(w, h) * .16f, paint)

        when (chapterIndex) {
            2 -> { // jagged mountain ridge
                paint.color = Color.rgb(35, 48, 61)
                val peaks = android.graphics.Path().apply {
                    moveTo(0f, h * .55f)
                    lineTo(w * .18f, h * .30f)
                    lineTo(w * .32f, h * .48f)
                    lineTo(w * .52f, h * .24f)
                    lineTo(w * .72f, h * .46f)
                    lineTo(w * .88f, h * .32f)
                    lineTo(w, h * .53f)
                    lineTo(w, h * .69f)
                    lineTo(0f, h * .69f)
                    close()
                }
                canvas.drawPath(peaks, paint)
            }
            3 -> { // dungeon bars
                paint.color = Color.rgb(95, 82, 103)
                paint.strokeWidth = w * .012f
                for (i in 1..7) canvas.drawLine(w * i / 8f, h * .25f, w * i / 8f, h * .69f, paint)
                canvas.drawLine(w * .08f, h * .42f, w * .92f, h * .42f, paint)
            }
            4 -> { // distant city silhouettes
                paint.color = Color.rgb(55, 30, 25)
                for (i in 0..6) {
                    val x = w * i / 7f
                    val buildingWidth = w * .12f
                    val top = h * (.48f + (i % 3) * .035f)
                    canvas.drawRect(x, top, x + buildingWidth, h * .69f, paint)
                }
            }
            5 -> { // palace towers
                paint.color = Color.rgb(65, 35, 48)
                canvas.drawRect(w * .12f, h * .35f, w * .25f, h * .69f, paint)
                canvas.drawRect(w * .75f, h * .35f, w * .88f, h * .69f, paint)
                canvas.drawRect(w * .30f, h * .45f, w * .70f, h * .69f, paint)
                canvas.drawCircle(w * .185f, h * .35f, w * .065f, paint)
                canvas.drawCircle(w * .815f, h * .35f, w * .065f, paint)
            }
        }
    }

    private fun enemyStepInterval(): Long {
        // Later chapters steadily increase pressure, with a fair lower bound.
        return (1650L - chapterIndex * 90L).coerceAtLeast(1200L)
    }

    private fun updateEnemy() {
        val now = System.currentTimeMillis()
        if (now - lastEnemyStep < enemyStepInterval()) return
        lastEnemyStep = now
        enemyTurnCount++
        val specialStrike = chapterIndex >= 2 && enemyTurnCount % 3 == 0
        if (abs(enemyX - playerX) < .19f) {
            if (now < dodgeActiveUntil) {
                message = if (specialStrike) "جاخالی عالی! حمله ویژه دشمن خنثی شد" else "جاخالی موفق! ضربه دشمن بی‌اثر شد"
                messageUntil = now + 1300L
            } else {
                val damage = if (specialStrike) 2 else 1
                val previousHealth = health
                health = (health - damage).coerceAtLeast(0)
                playerDamageFlashUntil = now + 260L
                message = if (specialStrike) "حمله ویژه دشمن! دو واحد سلامتی از دست دادی" else "نگهبان ضربه زد؛ زمان‌بندی جاخالی را بهتر کن"
                messageUntil = now + 1500L
                if (health <= 0) {
                    gameOver = true
                    message = "کاوه از پا افتاد؛ برای تلاش دوباره لمس کن"
                    messageUntil = now + 4000L
                } else if (health == 1 && previousHealth > 1) {
                    message = "هشدار: سلامتی کاوه بحرانی است! از معجون یا جاخالی استفاده کن"
                    messageUntil = now + 2200L
                }
            }
        } else {
            enemyX += if (playerX > enemyX) .045f else -.045f
            enemyX = enemyX.coerceIn(.08f, .92f)
        }
        // Save each completed enemy turn, including movement and incoming damage.
        saveGameState()
    }

    private fun drawHud(canvas: Canvas, w: Float, h: Float) {
        val chapter = ChapterData.chapters[chapterIndex]
        paint.textAlign = Paint.Align.CENTER
        paint.color = Color.rgb(230, 174, 77)
        paint.textSize = min(w, h) * .052f
        canvas.drawText("قیام کاوه | " + chapter.titleFa, w / 2, h * .075f, paint)

        paint.color = Color.rgb(245, 220, 195)
        paint.textSize = min(w, h) * .032f
        canvas.drawText(chapter.subtitleFa, w / 2, h * .12f, paint)
        paint.textSize = min(w, h) * .029f
        canvas.drawText("فصل ${chapterIndex + 1} از ${ChapterData.chapters.size}", w / 2, h * .16f, paint)
        drawButton(canvas, RectF(w * .03f, h * .035f, w * .19f, h * .105f), if (potionUsed) "معجون×" else "معجون")
        drawButton(canvas, RectF(w * .83f, h * .035f, w * .97f, h * .105f), "Ⅱ")

        // Compact health bars make combat status readable at a glance.
        val barWidth = w * .30f
        val barHeight = h * .018f
        val left = w * .10f
        val right = w * .60f
        val top = h * .19f
        paint.color = Color.rgb(65, 35, 27)
        canvas.drawRoundRect(RectF(left, top, left + barWidth, top + barHeight), 8f, 8f, paint)
        canvas.drawRoundRect(RectF(right, top, right + barWidth, top + barHeight), 8f, 8f, paint)
        paint.color = Color.rgb(190, 55, 45)
        canvas.drawRoundRect(RectF(left, top, left + barWidth * (health / 3f).coerceIn(0f, 1f), top + barHeight), 8f, 8f, paint)
        paint.color = Color.rgb(225, 165, 55)
        val enemyMax = chapter.enemyHealth.toFloat()
        canvas.drawRoundRect(RectF(right, top, right + barWidth * (enemyHealth / enemyMax).coerceIn(0f, 1f), top + barHeight), 8f, 8f, paint)

        paint.color = Color.WHITE
        paint.textSize = min(w, h) * .029f
        canvas.drawText("سلامتی کاوه: $health / 3", left + barWidth / 2, top + h * .045f, paint)
        canvas.drawText("${chapter.enemyNameFa}: $enemyHealth / ${chapter.enemyHealth}", right + barWidth / 2, top + h * .045f, paint)
        val now = System.currentTimeMillis()
        paint.color = Color.rgb(255, 220, 145)
        paint.textSize = min(w, h) * .028f
        canvas.drawText("امتیاز: $score   |   رکورد: $bestScore", w / 2, h * .255f, paint)
        paint.textSize = min(w, h) * .023f
        paint.color = if (now < heavyAttackReadyAt) Color.LTGRAY else Color.rgb(255, 205, 105)
        canvas.drawText(
            if (now < heavyAttackReadyAt) "ضربه سنگین: ${((heavyAttackReadyAt - now + 999L) / 1000L)} ثانیه" else "ضربه سنگین آماده است؛ دکمه سنگین پایین صفحه را بزن",
            w / 2, h * .285f, paint
        )
        if (now < messageUntil) {
            paint.color = Color.rgb(255, 210, 120)
            paint.textSize = min(w, h) * .033f
            canvas.drawText(message, w / 2, h * .29f, paint)
        }

        // Telegraph an incoming enemy strike so players can react with dodge.
        val enemyIsClose = abs(enemyX - playerX) < .19f
        val enemyStepProgress = now - lastEnemyStep
        val enemyInterval = enemyStepInterval()
        if (!paused && !storyIntro && !gameOver && !won &&
            enemyIsClose && enemyStepProgress in (enemyInterval - 600L)..(enemyInterval - 1L)) {
            paint.color = Color.rgb(255, 105, 75)
            paint.textSize = min(w, h) * .032f
            val nextStrikeIsSpecial = chapterIndex >= 2 && (enemyTurnCount + 1) % 3 == 0
            canvas.drawText(
                if (nextStrikeIsSpecial) "هشدار: حمله ویژه دشمن! جاخالی بده!" else "هشدار: دشمن آماده ضربه است؛ جاخالی بده!",
                w / 2, h * .33f, paint
            )
        }
    }

    private fun drawCharacter(canvas: Canvas, x: Float, ground: Float, hero: Boolean) {
        paint.color = if (hero) Color.rgb(218, 165, 65) else Color.rgb(125, 45, 35)
        canvas.drawCircle(x, ground - height * .12f, height * .045f, paint)
        paint.strokeWidth = height * .018f
        canvas.drawLine(x, ground - height * .075f, x, ground - height * .015f, paint)
        canvas.drawLine(x, ground - height * .055f, x - width * .045f, ground - height * .025f, paint)
        canvas.drawLine(x, ground - height * .055f, x + width * .045f, ground - height * .025f, paint)
        canvas.drawLine(x, ground - height * .015f, x - width * .03f, ground, paint)
        canvas.drawLine(x, ground - height * .015f, x + width * .03f, ground, paint)
        if (hero) {
            paint.color = Color.rgb(245, 190, 75)
            canvas.drawLine(x + width * .045f, ground - height * .03f, x + width * .09f, ground - height * .10f, paint)
        }
    }

    private fun drawAttackEffect(canvas: Canvas, w: Float, h: Float) {
        val now = System.currentTimeMillis()
        if (now >= attackEffectUntil) return
        val progress = 1f - ((attackEffectUntil - now) / 240f).coerceIn(0f, 1f)
        val centerX = w * playerX + if (enemyX >= playerX) w * .055f else -w * .055f
        val centerY = h * .69f - h * .085f
        val radius = min(w, h) * (.10f + progress * .035f)
        paint.style = Paint.Style.STROKE
        paint.strokeWidth = min(w, h) * .012f
        paint.color = if (attackEffectHit) Color.rgb(255, 220, 95) else Color.rgb(210, 170, 115)
        canvas.drawArc(
            RectF(centerX - radius, centerY - radius, centerX + radius, centerY + radius),
            if (enemyX >= playerX) 205f else -25f,
            if (enemyX >= playerX) 125f else -125f,
            false,
            paint
        )
        paint.style = Paint.Style.FILL
    }

    private fun drawHeavyAttackEffect(canvas: Canvas, w: Float, h: Float) {
        val now = System.currentTimeMillis()
        if (now >= heavyAttackEffectUntil) return
        val progress = 1f - ((heavyAttackEffectUntil - now) / 420f).coerceIn(0f, 1f)
        val centerX = w * playerX + if (enemyX >= playerX) w * .07f else -w * .07f
        val centerY = h * .69f - h * .08f
        val radius = min(w, h) * (.13f + progress * .08f)
        paint.style = Paint.Style.STROKE
        paint.strokeWidth = min(w, h) * .018f
        paint.color = if (heavyAttackEffectHit) Color.rgb(255, 225, 105) else Color.rgb(185, 145, 105)
        canvas.drawCircle(centerX, centerY, radius, paint)
        paint.style = Paint.Style.FILL
    }

    private fun drawDamageFlash(canvas: Canvas, w: Float, h: Float) {
        val remaining = playerDamageFlashUntil - System.currentTimeMillis()
        if (remaining <= 0L) return
        val alpha = (remaining * 105L / 260L).toInt().coerceIn(0, 105)
        paint.style = Paint.Style.STROKE
        paint.strokeWidth = min(w, h) * .035f
        paint.color = Color.argb(alpha, 225, 35, 25)
        canvas.drawRect(
            paint.strokeWidth / 2f,
            paint.strokeWidth / 2f,
            w - paint.strokeWidth / 2f,
            h - paint.strokeWidth / 2f,
            paint
        )
        paint.style = Paint.Style.FILL
    }

    private fun drawEnemy(canvas: Canvas, x: Float, ground: Float) {
        drawCharacter(canvas, x, ground, false)
        val headY = ground - height * .12f
        val scale = height * .01f

        // Give every chapter's opponent a recognizable silhouette.
        when (chapterIndex) {
            0, 1 -> { // soldiers: a simple metal helmet and shield
                paint.color = Color.rgb(190, 175, 145)
                canvas.drawRect(x - scale * 3.5f, headY - scale * 3f, x + scale * 3.5f, headY - scale * 1.5f, paint)
                paint.color = Color.rgb(135, 85, 45)
                canvas.drawOval(RectF(x + width * .035f, ground - height * .09f, x + width * .085f, ground - height * .015f), paint)
            }
            2 -> { // mountain demon: horned head and broad shoulders
                paint.color = Color.rgb(75, 105, 91)
                canvas.drawCircle(x, headY, height * .052f, paint)
                paint.strokeWidth = scale * 5f
                canvas.drawLine(x - scale * 3f, headY - scale * 2f, x - scale * 8f, headY - scale * 10f, paint)
                canvas.drawLine(x + scale * 3f, headY - scale * 2f, x + scale * 8f, headY - scale * 10f, paint)
            }
            3 -> { // dungeon keeper: iron mask and heavy chains
                paint.color = Color.rgb(165, 155, 145)
                canvas.drawRect(x - scale * 4f, headY - scale * 2f, x + scale * 4f, headY + scale * 2f, paint)
                paint.strokeWidth = scale * 2f
                canvas.drawLine(x - width * .045f, ground - height * .02f, x - width * .075f, ground + height * .025f, paint)
                canvas.drawLine(x + width * .045f, ground - height * .02f, x + width * .075f, ground + height * .025f, paint)
            }
            4 -> { // commander: raised crest and dark cloak
                paint.color = Color.rgb(75, 25, 22)
                canvas.drawOval(RectF(x - width * .065f, ground - height * .09f, x + width * .065f, ground + height * .015f), paint)
                paint.color = Color.rgb(220, 170, 65)
                canvas.drawPath(android.graphics.Path().apply {
                    moveTo(x, headY - scale * 9f)
                    lineTo(x - scale * 4f, headY)
                    lineTo(x + scale * 4f, headY)
                    close()
                }, paint)
            }
            else -> { // Zahhak: royal crown and two serpent-like necks
                paint.color = Color.rgb(225, 175, 55)
                canvas.drawRect(x - scale * 5f, headY - scale * 7f, x + scale * 5f, headY - scale * 3f, paint)
                canvas.drawLine(x - width * .025f, ground - height * .06f, x - width * .065f, ground - height * .12f, paint)
                canvas.drawLine(x + width * .025f, ground - height * .06f, x + width * .065f, ground - height * .12f, paint)
                paint.color = Color.rgb(65, 125, 75)
                canvas.drawCircle(x - width * .065f, ground - height * .12f, scale * 3f, paint)
                canvas.drawCircle(x + width * .065f, ground - height * .12f, scale * 3f, paint)
            }
        }
        paint.color = Color.rgb(245, 220, 195)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = height * .032f
        canvas.drawText(ChapterData.chapters[chapterIndex].enemyNameFa, x, ground + height * .045f, paint)
    }

    private fun drawControls(canvas: Canvas, w: Float, h: Float) {
        drawButton(canvas, RectF(w * .02f, h * .84f, w * .19f, h * .96f), "◀")
        drawButton(canvas, RectF(w * .21f, h * .84f, w * .38f, h * .96f), "▶")
        val now = System.currentTimeMillis()
        val dodgeLabel = if (now < dodgeCooldownUntil) "جاخالی…" else "جاخالی"
        val heavyLabel = if (now < heavyAttackReadyAt) "سنگین…" else "سنگین"
        drawButton(canvas, RectF(w * .40f, h * .84f, w * .58f, h * .96f), dodgeLabel)
        drawButton(canvas, RectF(w * .60f, h * .84f, w * .78f, h * .96f), "حمله")
        drawButton(canvas, RectF(w * .80f, h * .84f, w * .98f, h * .96f), heavyLabel)
    }

    private fun drawButton(canvas: Canvas, rect: RectF, label: String) {
        paint.color = Color.rgb(102, 54, 27)
        canvas.drawRoundRect(rect, 18f, 18f, paint)
        paint.color = Color.rgb(255, 221, 155)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = height * .043f
        canvas.drawText(label, rect.centerX(), rect.centerY() + height * .015f, paint)
    }

    private fun drawStoryOverlay(canvas: Canvas, w: Float, h: Float) {
        val chapter = ChapterData.chapters[chapterIndex]
        paint.color = Color.argb(232, 12, 7, 5)
        canvas.drawRect(0f, 0f, w, h, paint)
        val panel = RectF(w * .08f, h * .28f, w * .92f, h * .70f)
        paint.color = Color.rgb(72, 38, 20)
        canvas.drawRoundRect(panel, 24f, 24f, paint)
        paint.color = Color.rgb(235, 178, 72)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = min(w, h) * .052f
        canvas.drawText("فصل ${chapterIndex + 1}: ${chapter.titleFa}", w / 2, h * .37f, paint)
        paint.color = Color.rgb(255, 235, 205)
        paint.textSize = min(w, h) * .038f
        val maxTextWidth = w * .76f
        val lines = mutableListOf<String>()
        var line = ""
        chapter.storyFa.split(" ").forEach { word ->
            val candidate = if (line.isEmpty()) word else "$line $word"
            if (paint.measureText(candidate) > maxTextWidth && line.isNotEmpty()) {
                lines.add(line)
                line = word
            } else line = candidate
        }
        if (line.isNotEmpty()) lines.add(line)
        lines.take(4).forEachIndexed { index, text ->
            canvas.drawText(text, w / 2, h * (.46f + index * .055f), paint)
        }
        paint.color = Color.rgb(255, 210, 120)
        paint.textSize = min(w, h) * .035f
        canvas.drawText("برای آغاز این فصل لمس کن", w / 2, h * .64f, paint)
    }

    private fun drawOverlay(canvas: Canvas, w: Float, h: Float, title: String, subtitle: String) {
        paint.color = Color.argb(220, 12, 7, 5)
        canvas.drawRect(0f, 0f, w, h, paint)
        paint.color = Color.rgb(235, 178, 72)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = h * .065f
        canvas.drawText(title, w / 2, h * .43f, paint)
        paint.color = Color.WHITE
        paint.textSize = h * .04f
        subtitle.split("\n").forEachIndexed { i, line ->
            canvas.drawText(line, w / 2, h * (.51f + i * .06f), paint)
        }
    }

    override fun onTouchEvent(event: MotionEvent): Boolean {
        if (event.action != MotionEvent.ACTION_DOWN) return true
        val w = width.toFloat()
        val h = height.toFloat()
        if (paused) {
            paused = false
            // Restart the enemy cadence so resuming never causes an immediate hit.
            lastEnemyStep = System.currentTimeMillis()
            invalidate()
            return true
        }
        if (event.y < h * .13f && event.x < w * .20f && !storyIntro && !gameOver && !won) {
            usePotion()
            invalidate()
            return true
        }
        if (event.y < h * .13f && event.x > w * .80f && !storyIntro && !gameOver && !won) {
            paused = true
            // Keep combat timing consistent while the game is paused.
            lastEnemyStep = System.currentTimeMillis()
            invalidate()
            return true
        }
        if (storyIntro && !gameOver && !won) {
            storyIntro = false
            saveGameState()
            invalidate()
            return true
        }
        if (gameOver || won) {
            if (won) {
                if (chapterIndex < ChapterData.chapters.lastIndex) {
                    chapterIndex++
                } else {
                    chapterIndex = 0
                    score = 0
                    prefs.edit().putInt("score", score).apply()
                }
                prefs.edit().putInt("chapter", chapterIndex).apply()
                storyIntro = true
            }
            resetCombat()
            invalidate()
            return true
        }
        if (event.y > h * .82f) {
            when {
                event.x < w * .20f -> playerX = (playerX - .075f).coerceIn(.08f, .92f)
                event.x < w * .39f -> playerX = (playerX + .075f).coerceIn(.08f, .92f)
                event.x < w * .59f -> dodge()
                event.x < w * .79f -> attack()
                else -> heavyAttack()
            }
        } else {
            playerX = (playerX + if (event.x > w / 2) .05f else -.05f).coerceIn(.08f, .92f)
        }
        // Preserve the latest player position even if Android stops the app
        // before the next enemy update.
        saveGameState()
        invalidate()
        return true
    }

    private fun usePotion() {
        val now = System.currentTimeMillis()
        if (gameOver || won || paused || storyIntro) return
        if (potionUsed) {
            message = "معجون این فصل مصرف شده است"
        } else if (health >= 3) {
            message = "سلامتی کاوه کامل است"
        } else {
            health = (health + 1).coerceAtMost(3)
            potionUsed = true
            message = "معجون درمانی مصرف شد؛ یک واحد سلامتی بازیابی شد"
        }
        messageUntil = now + 1400L
        saveGameState()
    }

    private fun dodge() {
        val now = System.currentTimeMillis()
        if (now < dodgeCooldownUntil) {
            message = "جاخالی هنوز آماده نیست"
            messageUntil = now + 900L
            return
        }
        // A short invulnerability window rewards timing without making combat trivial.
        dodgeActiveUntil = now + 650L
        dodgeCooldownUntil = now + 1800L
        playerX = (playerX + if (enemyX >= playerX) -.10f else .10f).coerceIn(.08f, .92f)
        message = "کاوه جاخالی داد؛ حالا ضدحمله کن!"
        messageUntil = now + 1200L
        saveGameState()
    }

    private fun heavyAttack() {
        val now = System.currentTimeMillis()
        if (won || gameOver || paused || storyIntro) return
        if (now < heavyAttackReadyAt) {
            message = "ضربه سنگین هنوز آماده نیست"
            messageUntil = now + 1000L
            return
        }
        heavyAttackReadyAt = now + 8000L
        saveGameState()
        heavyAttackEffectUntil = now + 420L
        heavyAttackEffectHit = abs(enemyX - playerX) < .30f
        if (heavyAttackEffectHit) {
            enemyHealth = (enemyHealth - 3).coerceAtLeast(0)
            message = "ضربه سنگین کاوه! سه واحد آسیب"
            messageUntil = now + 1400L
            if (enemyHealth == 0) finishEnemy(now)
            saveGameState()
        } else {
            message = "ضربه سنگین به هدف نرسید؛ به دشمن نزدیک‌تر شو"
            messageUntil = now + 1300L
        }
    }

    private fun finishEnemy(now: Long) {
        val chapterReward = ChapterData.chapters[chapterIndex].enemyHealth * 100
        score += chapterReward
        if (score > bestScore) bestScore = score
        prefs.edit().putInt("score", score).putInt("best_score", bestScore).apply()
        message = "پیروزی! $chapterReward امتیاز دریافت کردی"
        messageUntil = now + 1800L
        won = true
    }

    private fun attack() {
        val now = System.currentTimeMillis()
        if (won || gameOver || paused || storyIntro) return
        if (now - hitCooldown < 600L) return
        hitCooldown = now
        attackEffectUntil = now + 240L
        attackEffectHit = abs(enemyX - playerX) < .23f
        if (attackEffectHit) {
            enemyHealth--
            message = "ضربه کاوه!"
            messageUntil = now + 1100L
            if (enemyHealth == 0) finishEnemy(now)
            saveGameState()
        } else {
            message = "به دشمن نزدیک‌تر شو"
            messageUntil = now + 1100L
        }
    }

    private fun resetCombat() {
        playerX = .22f
        enemyX = .78f
        health = 3
        enemyHealth = ChapterData.chapters[chapterIndex].enemyHealth
        enemyTurnCount = 0
        hitCooldown = 0L
        attackEffectUntil = 0L
        attackEffectHit = false
        dodgeCooldownUntil = 0L
        dodgeActiveUntil = 0L
        // Each fresh attempt/chapter starts with its healing potion available.
        potionUsed = false
        prefs.edit().putBoolean("potion_used", false).apply()
        lastEnemyStep = System.currentTimeMillis()
        gameOver = false
        won = false
        paused = false
        message = "کاوه را حرکت بده؛ نزدیک دشمن شو و حمله کن"
        messageUntil = System.currentTimeMillis() + 3500L
        // Persist the full reset immediately so a process interruption cannot
        // restore stale combat health or enemy progress on the next launch.
        saveGameState()
    }
}
