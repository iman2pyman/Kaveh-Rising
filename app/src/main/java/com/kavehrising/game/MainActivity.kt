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
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = Color.rgb(25, 13, 9)
        window.navigationBarColor = Color.rgb(25, 13, 9)
        setContentView(KavehGameView(this))
    }
}

private class KavehGameView(private val host: Activity) : View(host) {
    private val paint = Paint(Paint.ANTI_ALIAS_FLAG)
    private val prefs = host.getPreferences(Activity.MODE_PRIVATE)
    private var chapterIndex = prefs.getInt("chapter", 0).coerceIn(0, ChapterData.chapters.lastIndex)
    private var playerX = 0.22f
    private var enemyX = 0.78f
    private var health = 3
    private var enemyHealth = ChapterData.chapters[chapterIndex].enemyHealth
    private var hitCooldown = 0L
    private var dodgeCooldownUntil = 0L
    private var dodgeActiveUntil = 0L
    private var lastEnemyStep = System.currentTimeMillis()
    private var gameOver = false
    private var won = false
    private var message = "کاوه را حرکت بده؛ نزدیک دشمن شو و حمله کن"
    private var messageUntil = System.currentTimeMillis() + 4000L

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)
        val w = width.toFloat()
        val h = height.toFloat()
        drawWorld(canvas, w, h)
        drawHud(canvas, w, h)
        drawCharacter(canvas, w * playerX, h * .69f, true)
        drawEnemy(canvas, w * enemyX, h * .69f)
        drawControls(canvas, w, h)

        if (!gameOver && !won) {
            updateEnemy()
            postInvalidateDelayed(70)
        }
        if (gameOver) drawOverlay(canvas, w, h, "کاوه شکست خورد", "برای تلاش دوباره لمس کن")
        if (won) {
            val next = if (chapterIndex < ChapterData.chapters.lastIndex)
                "مرحله بعد: " + ChapterData.chapters[chapterIndex + 1].titleFa + "\nبرای ادامه لمس کن"
            else "درفش کاویانی برافراشته شد!\nلمس کن تا از ابتدا بازی کنی"
            drawOverlay(canvas, w, h, "پیروزی کاوه!", next)
        }
    }

    private fun drawWorld(canvas: Canvas, w: Float, h: Float) {
        canvas.drawColor(Color.rgb(29, 16, 12))
        paint.color = Color.rgb(58, 37, 28)
        canvas.drawOval(RectF(-w * .2f, h * .28f, w * .55f, h * .68f), paint)
        canvas.drawOval(RectF(w * .35f, h * .22f, w * 1.2f, h * .67f), paint)
        paint.color = Color.rgb(111, 58, 28)
        canvas.drawRect(0f, h * .69f, w, h * .82f, paint)
        paint.color = Color.rgb(45, 28, 20)
        canvas.drawRect(0f, h * .82f, w, h, paint)
        paint.color = Color.argb(55, 255, 130, 30)
        canvas.drawCircle(w * .5f, h * .66f, min(w, h) * .16f, paint)
    }

    private fun updateEnemy() {
        val now = System.currentTimeMillis()
        if (now - lastEnemyStep < 1650L) return
        lastEnemyStep = now
        if (abs(enemyX - playerX) < .19f) {
            health--
            message = "نگهبان ضربه زد؛ جاخالی بده!"
            messageUntil = now + 1400L
            if (health <= 0) gameOver = true
        } else {
            enemyX += if (playerX > enemyX) .045f else -.045f
            enemyX = enemyX.coerceIn(.08f, .92f)
        }
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
        if (System.currentTimeMillis() < messageUntil) {
            paint.color = Color.rgb(255, 210, 120)
            paint.textSize = min(w, h) * .033f
            canvas.drawText(message, w / 2, h * .29f, paint)
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

    private fun drawEnemy(canvas: Canvas, x: Float, ground: Float) {
        drawCharacter(canvas, x, ground, false)
        paint.color = Color.rgb(245, 220, 195)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = height * .032f
        canvas.drawText(ChapterData.chapters[chapterIndex].enemyNameFa, x, ground + height * .045f, paint)
    }

    private fun drawControls(canvas: Canvas, w: Float, h: Float) {
        drawButton(canvas, RectF(w * .06f, h * .84f, w * .32f, h * .96f), "◀")
        drawButton(canvas, RectF(w * .36f, h * .84f, w * .62f, h * .96f), "▶")
        drawButton(canvas, RectF(w * .68f, h * .84f, w * .94f, h * .96f), "⚒ حمله")
    }

    private fun drawButton(canvas: Canvas, rect: RectF, label: String) {
        paint.color = Color.rgb(102, 54, 27)
        canvas.drawRoundRect(rect, 18f, 18f, paint)
        paint.color = Color.rgb(255, 221, 155)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = height * .043f
        canvas.drawText(label, rect.centerX(), rect.centerY() + height * .015f, paint)
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
        if (gameOver || won) {
            if (won) {
                chapterIndex = if (chapterIndex < ChapterData.chapters.lastIndex) chapterIndex + 1 else 0
                prefs.edit().putInt("chapter", chapterIndex).apply()
            }
            resetCombat()
            invalidate()
            return true
        }
        if (event.y > h * .82f) {
            when {
                event.x < w * .34f -> playerX = (playerX - .075f).coerceIn(.08f, .92f)
                event.x < w * .65f -> playerX = (playerX + .075f).coerceIn(.08f, .92f)
                else -> attack()
            }
        } else {
            playerX = (playerX + if (event.x > w / 2) .05f else -.05f).coerceIn(.08f, .92f)
        }
        invalidate()
        return true
    }

    private fun dodge() {\n        val now = System.currentTimeMillis()\n        if (now < dodgeCooldownUntil) {\n            message = "جاخالی هنوز آماده نیست"\n            messageUntil = now + 900L\n            return\n        }\n        // A short invulnerability window rewards timing without making combat trivial.\n        dodgeActiveUntil = now + 650L\n        dodgeCooldownUntil = now + 1800L\n        playerX = (playerX + if (enemyX >= playerX) -.10f else .10f).coerceIn(.08f, .92f)\n        message = "کاوه جاخالی داد؛ حالا ضدحمله کن!"\n        messageUntil = now + 1200L\n    }\n\n    private fun attack() {
        val now = System.currentTimeMillis()
        if (now - hitCooldown < 600L) return
        hitCooldown = now
        if (abs(enemyX - playerX) < .23f) {
            enemyHealth--
            message = "ضربه کاوه!"
            messageUntil = now + 1100L
            if (enemyHealth <= 0) won = true
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
        hitCooldown = 0L
        lastEnemyStep = System.currentTimeMillis()
        gameOver = false
        won = false
        message = "کاوه را حرکت بده؛ نزدیک دشمن شو و حمله کن"
        messageUntil = System.currentTimeMillis() + 3500L
    }
}
