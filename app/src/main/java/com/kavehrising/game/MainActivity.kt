package com.kavehrising.game

import android.app.Activity
import android.os.Bundle
import android.graphics.Color
import android.view.Gravity
import android.widget.LinearLayout
import android.widget.TextView

class MainActivity : Activity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        window.statusBarColor = Color.rgb(35, 18, 12)
        window.navigationBarColor = Color.rgb(35, 18, 12)

        val screen = LinearLayout(this).apply {
            orientation = LinearLayout.VERTICAL
            gravity = Gravity.CENTER
            setPadding(32, 32, 32, 32)
            setBackgroundColor(Color.rgb(35, 18, 12))
        }
        val title = TextView(this).apply {
            text = "قیام کاوه\nKAVEH RISING"
            textSize = 30f
            gravity = Gravity.CENTER
            setTextColor(Color.rgb(224, 166, 72))
        }
        val subtitle = TextView(this).apply {
            text = "از آتش آهنگری تا قیام مردم\n\nپروژه در حال توسعه است"
            textSize = 18f
            gravity = Gravity.CENTER
            setTextColor(Color.WHITE)
            setPadding(0, 28, 0, 0)
        }
        screen.addView(title)
        screen.addView(subtitle)
        setContentView(screen)
    }
}
