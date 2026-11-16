package com.kavehrising.game

data class GameChapter(
    val titleFa: String,
    val subtitleFa: String,
    val enemyNameFa: String,
    val enemyHealth: Int
)

object ChapterData {
    val chapters = listOf(
        GameChapter("آتش آهنگری", "کاوه در برابر نخستین نگهبان ضحاک", "نگهبان ضحاک", 5),
        GameChapter("فریاد ستم‌دیدگان", "مردم به ندای کاوه پاسخ می‌دهند", "سرباز ضحاک", 7),
        GameChapter("گذرگاه کوهستان", "راه دشوار از میان کوه‌های افسانه‌ای", "دیو کوهستان", 9),
        GameChapter("سیاه‌چال ضحاک", "آزادسازی زندانیان از بند ستم", "پاسدار سیاه‌چال", 11),
        GameChapter("خیزش مردم", "همراهی مردم برای نبرد نهایی", "فرمانده ضحاک", 13),
        GameChapter("درفش کاویانی", "برافراشتن درفش آزادی و پایان فصل نخست", "ضحاک", 15)
    )
}
