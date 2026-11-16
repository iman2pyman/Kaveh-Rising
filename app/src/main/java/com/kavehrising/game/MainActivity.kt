package com.kavehrising.game

import android.app.Activity
import android.os.Bundle
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.RectF
import android.view.MotionEvent
import android.view.View
import kotlin.math.min

class MainActivity : Activity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = Color.rgb(25, 13, 9)
        window.navigationBarColor = Color.rgb(25, 13, 9)
        setContentView(KavehGameView())
    }
}

private class KavehGameView : View {
    private val paint = Paint(Paint.ANTI_ALIAS_FLAG)
    private var chapterIndex = 0
    private var playerX = 0.22f
    private var enemyX = 0.78f
    private var health = 3
    private var enemyHealth = ChapterData.chapters[0].enemyHealth
    private var hitCooldown = 0L
    private var lastEnemyStep = 0L
    private var gameOver = false
    private var won = false
    private var message = "کاوه را به سوی دشمن هدایت کن"
    private var messageUntil = 0L

    constructor(activity: Activity) : super(activity)

    override fun onDraw(canvas: Canvas) {
        super.onDraw(canvas)
        val w = width.toFloat()
        val h = height.toFloat()
        canvas.drawColor(Color.rgb(29, 16, 12))

        // Mountain silhouettes and the forge-lit horizon
        paint.color = Color.rgb(58, 37, 28)
        canvas.drawOval(RectF(-w * .2f, h * .28f, w * .55f, h * .68f), paint)
        canvas.drawOval(RectF(w * .35f, h * .22f, w * 1.2f, h * .67f), paint)
        paint.color = Color.rgb(111, 58, 28)
        canvas.drawRect(0f, h * .69f, w, h * .82f, paint)
        paint.color = Color.rgb(45, 28, 20)
        canvas.drawRect(0f, h * .82f, w, h, paint)

        drawHud(canvas, w, h)
        drawCharacter(canvas, w * playerX, h * .69f, true)
        drawEnemy(canvas, w * enemyX, h * .69f)

        // Touch controls
        drawButton(canvas, RectF(w * .08f, h * .84f, w * .34f, h * .96f), "◀")
        drawButton(canvas, RectF(w * .38f, h * .84f, w * .64f, h * .96f), "▶")
        drawButton(canvas, RectF(w * .70f, h * .84f, w * .94f, h * .96f), "⚒ حمله")

        if (!gameOver && !won) {
            val now = System.currentTimeMillis()
            if (now - lastEnemyStep > 1800L) {
                lastEnemyStep = now
                if (kotlin.math.abs(enemyX - playerX) < .20f) {
                    health--
                    message = "دشمن ضربه زد! مراقب باش"
                    messageUntil = now + 1300
                    if (health <= 0) gameOver = true
                } else {
                    enemyX += if (playerX > enemyX) .035f else -.035f
                    enemyX = enemyX.coerceIn(.08f, .92f)
                }
            }
            postInvalidateDelayed(80)
        }
        if (gameOver) drawOverlay(canvas, w, h, "کاوه شکست خورد", "برای شروع دوباره لمس کن")
        if (won) drawOverlay(canvas, w, h, "پیروزی کاوه!", if (chapterIndex < ChapterData.chapters.lastIndex) "مرحله بعد: " + ChapterData.chapters[chapterIndex + 1].titleFa + "\nبرای ادامه لمس کن" else "درفش کاویانی برافراشته شد!\nبرای شروع دوباره لمس کن")
    }

    private fun drawHud(canvas: Canvas, w: Float, h: Float) {
        paint.color = Color.rgb(230, 174, 77)
        paint.textSize = min(w, h) * .055f
        paint.textAlign = Paint.Align.CENTER
        canvas.drawText("قیام کاوه | " + ChapterData.chapters[chapterIndex].titleFa, w / 2, h * .08f, paint)
        paint.color = Color.WHITE
        paint.textSize = min(w, h) * .04f
        canvas.drawText("سلامتی: $health     جان دشمن: $enemyHealth", w / 2, h * .14f, paint)
        if (System.currentTimeMillis() < messageUntil) {
            paint.color = Color.rgb(255, 210, 120)
            canvas.drawText(message, w / 2, h * .21f, paint)
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
        paint.color = Color.rgb(35, 10, 8)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = height * .035f
        canvas.drawText(ChapterData.chapters[chapterIndex].enemyNameFa, x, ground + height * .045f, paint)
    }

    private fun drawButton(canvas: Canvas, rect: RectF, label: String) {
        paint.color = Color.rgb(102, 54, 27)
        canvas.drawRoundRect(rect, 18f, 18f, paint)
        paint.color = Color.rgb(255, 221, 155)
        paint.textAlign = Paint.Align.CENTER
        paint.textSize = height * .045f
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
        subtitle.split("\n").forEachIndexed { i, line -> canvas.drawText(line, w / 2, h * (.51f + i * .06f), paint) }
    }

    override fun onTouchEvent(event: MotionEvent): Boolean {
        if (event.action != MotionEvent.ACTION_DOWN) return true
        val w = width.toFloat()
        val h = height.toFloat()
        if (gameOver || won) {
            playerX = .22f; enemyX = .78f; health = 3; enemyHealth = ChapterData.chapters[chapterIndex].enemyHealth
            gameOver = false; won = false
            message = "کاوه را به سوی دشمن هدایت کن"
            invalidate()
            return true
        }
        if (event.y > h * .82f) {
            when {
                event.x < w * .36f -> playerX = (playerX - .07f).coerceIn(.08f, .92f)
                event.x < w * .68f -> playerX = (playerX + .07f).coerceIn(.08f, .92f)
                else -> {
                    val now = System.currentTimeMillis()
                    if (now - hitCooldown > 650L) {
                        hitCooldown = now
                        if (kotlin.math.abs(enemyX - playerX) < .22f) {
                            enemyHealth--
                            message = "ضربه کاوه!"
                            messageUntil = now + 1000
                            if (enemyHealth <= 0) won = true
                        } else {
                            message = "به دشمن نزدیک شو"
                            messageUntil = now + 1000
                        }
                    }
                }
            }
        } else {
            playerX = (playerX + if (event.x > w / 2) .05f else -.05f).coerceIn(.08f, .92f)
        }
        invalidate()
        return true
    }
}
