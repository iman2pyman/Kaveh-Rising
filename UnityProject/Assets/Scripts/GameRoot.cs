using System;
using UnityEngine;

namespace KavehRising
{
    [Serializable]
    public class ChapterData
    {
        public string title;
        public string objective;
        public string enemyName;
        public int enemyHealth;
        public Color sky;
        public Color ground;
    }

    public class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance { get; private set; }
        public readonly ChapterData[] Chapters =
        {
            new ChapterData { title="آتش آهنگری", objective="نگهبانان آهنگری را شکست بده", enemyName="نگهبان ضحاک", enemyHealth=3, sky=new Color(.20f,.12f,.12f), ground=new Color(.20f,.10f,.07f) },
            new ChapterData { title="فریاد ستم‌دیدگان", objective="راه مردم را از سربازان باز کن", enemyName="سرباز ضحاک", enemyHealth=4, sky=new Color(.18f,.16f,.20f), ground=new Color(.22f,.14f,.12f) },
            new ChapterData { title="گذرگاه کوهستان", objective="از گذرگاه دیوان عبور کن", enemyName="دیو کوهستان", enemyHealth=5, sky=new Color(.12f,.19f,.27f), ground=new Color(.18f,.22f,.25f) },
            new ChapterData { title="سیاه‌چال ضحاک", objective="زندانبان را شکست بده و زندانیان را آزاد کن", enemyName="زندانبان", enemyHealth=6, sky=new Color(.10f,.09f,.15f), ground=new Color(.13f,.11f,.16f) },
            new ChapterData { title="خیزش مردم", objective="فرمانده سپاه را از پا درآور", enemyName="فرمانده", enemyHealth=7, sky=new Color(.27f,.12f,.10f), ground=new Color(.20f,.10f,.08f) },
            new ChapterData { title="درفش کاویانی", objective="به دژ ضحاک برس", enemyName="سردار دژ", enemyHealth=8, sky=new Color(.16f,.12f,.21f), ground=new Color(.17f,.12f,.18f) },
            new ChapterData { title="بندهای آهنین", objective="زنجیرهای زندان را درهم بشکن", enemyName="شکنجه‌گر", enemyHealth=8, sky=new Color(.13f,.12f,.16f), ground=new Color(.14f,.12f,.15f) },
            new ChapterData { title="ماران تاریکی", objective="نگهبانان افسانه‌ای را شکست بده", enemyName="مارآیین", enemyHealth=9, sky=new Color(.08f,.15f,.14f), ground=new Color(.11f,.16f,.13f) },
            new ChapterData { title="راه البرز", objective="از آخرین گذرگاه کوهستانی بگذر", enemyName="دیو سپید", enemyHealth=10, sky=new Color(.17f,.20f,.25f), ground=new Color(.20f,.21f,.23f) },
            new ChapterData { title="نبرد با ضحاک", objective="ستم ضحاک را برای همیشه پایان بده", enemyName="ضحاک", enemyHealth=14, sky=new Color(.15f,.06f,.10f), ground=new Color(.16f,.08f,.10f) }
        };

        public int Chapter { get; private set; }
        public int Health { get; private set; } = 5;
        public int Score { get; private set; }
        public int EnemyHealth { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsGameOver { get; private set; }
        public bool IsVictory { get; private set; }
        public bool HasPotion { get; private set; } = true;
        public string Notice { get; private set; } = "برای حرکت از دکمه‌ها استفاده کن";
        public PlayerMotor Player { get; private set; }
        public EnemyBrain Enemy { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            LoadProgress();
            StartChapter(false);
        }

        public void RegisterActors(PlayerMotor player, EnemyBrain enemy)
        {
            Player = player; Enemy = enemy;
        }

        public ChapterData Current => Chapters[Mathf.Clamp(Chapter, 0, Chapters.Length - 1)];

        public void DamagePlayer(int amount)
        {
            if (IsPaused || IsGameOver || IsVictory) return;
            Health = Mathf.Max(0, Health - amount);
            Notice = "کاوه آسیب دید!";
            if (Health == 0) { IsGameOver = true; Notice = "شکست؛ دوباره تلاش کن"; }
            SaveProgress();
        }

        public void DamageEnemy(int amount)
        {
            if (IsPaused || IsGameOver || IsVictory || EnemyHealth <= 0) return;
            EnemyHealth = Mathf.Max(0, EnemyHealth - amount);
            Notice = "ضربه وارد شد!";
            if (EnemyHealth == 0)
            {
                Score += 100 + Chapter * 25;
                IsVictory = true;
                Notice = Chapter == Chapters.Length - 1 ? "ضحاک شکست خورد؛ ایران آزاد شد!" : "پیروزی! فصل بعد آماده است";
                SaveProgress();
            }
        }

        public void UsePotion()
        {
            if (!HasPotion || Health >= 5 || IsPaused || IsGameOver || IsVictory) return;
            Health = Mathf.Min(5, Health + 2); HasPotion = false;
            Notice = "دو واحد سلامتی بازیابی شد"; SaveProgress();
        }

        public void TogglePause() { if (!IsGameOver && !IsVictory) IsPaused = !IsPaused; }
        public void NextChapter()
        {
            if (!IsVictory) return;
            Chapter = (Chapter + 1) % Chapters.Length;
            if (Chapter == 0) Score = 0;
            StartChapter(true);
        }
        public void Retry() { StartChapter(false); }

        void StartChapter(bool resetHealth)
        {
            IsPaused = false; IsGameOver = false; IsVictory = false;
            if (resetHealth) Health = 5;
            HasPotion = true; EnemyHealth = Current.enemyHealth;
            if (Player != null) Destroy(Player.gameObject);
            if (Enemy != null) Destroy(Enemy.gameObject);
            RuntimeWorld.BuildEnvironment(Current);
            RuntimeWorld.SpawnActors(this);
            SaveProgress();
        }

        void SaveProgress()
        {
            PlayerPrefs.SetInt("kr_chapter", Chapter);
            PlayerPrefs.SetInt("kr_score", Score);
            PlayerPrefs.Save();
        }
        void LoadProgress()
        {
            Chapter = Mathf.Clamp(PlayerPrefs.GetInt("kr_chapter", 0), 0, Chapters.Length - 1);
            Score = PlayerPrefs.GetInt("kr_score", 0);
        }

        void OnGUI()
        {
            GUI.depth = -10;
            var old = GUI.color;
            GUI.color = new Color(.05f,.04f,.04f,.78f);
            GUI.Box(new Rect(12,10,Screen.width*.46f,74),"");
            GUI.color = Color.white;
            GUI.Label(new Rect(25,14,Screen.width*.44f,30), $"فصل {Chapter+1}/10  |  {Current.title}     امتیاز: {Score}");
            GUI.Label(new Rect(25,45,Screen.width*.44f,28), $"سلامتی: {Health}/5       دشمن: {EnemyHealth}/{Current.enemyHealth}");
            float bw=Screen.width*.115f, bh=Screen.height*.14f, y=Screen.height-bh-16;
            if (GUI.RepeatButton(new Rect(14,y,bw,bh),"◀")) { if(Player) Player.SetMove(-1); }
            if (GUI.RepeatButton(new Rect(22+bw,y,bw,bh),"▶")) { if(Player) Player.SetMove(1); }
            if (GUI.Button(new Rect(Screen.width-bw*3-42,y,bw,bh),"جاخالی") && Player) Player.Dodge();
            if (GUI.Button(new Rect(Screen.width-bw*2-28,y,bw,bh),"حمله") && Player) Player.Attack(false);
            if (GUI.Button(new Rect(Screen.width-bw-14,y,bw,bh),"ضربه سنگین") && Player) Player.Attack(true);
            if (GUI.Button(new Rect(Screen.width-110,12,96,42), IsPaused?"ادامه":"مکث")) TogglePause();
            if (GUI.Button(new Rect(Screen.width-220,12,100,42),"معجون")) UsePotion();
            GUI.color = new Color(1f,.82f,.48f,1f);
            GUI.Label(new Rect(Screen.width*.25f,Screen.height*.16f,Screen.width*.5f,42),Notice);
            GUI.color = old;
            if (IsPaused || IsGameOver || IsVictory)
            {
                GUI.color = new Color(.02f,.02f,.02f,.88f);
                GUI.Box(new Rect(Screen.width*.25f,Screen.height*.28f,Screen.width*.5f,Screen.height*.44f),"");
                GUI.color = Color.white;
                GUI.Label(new Rect(Screen.width*.30f,Screen.height*.34f,Screen.width*.4f,50),IsPaused?"بازی متوقف شد":IsGameOver?"کاوه شکست خورد":"پیروزی");
                if (IsPaused && GUI.Button(new Rect(Screen.width*.40f,Screen.height*.52f,Screen.width*.20f,48),"ادامه بازی")) TogglePause();
                if (IsGameOver && GUI.Button(new Rect(Screen.width*.40f,Screen.height*.52f,Screen.width*.20f,48),"تلاش دوباره")) Retry();
                if (IsVictory && GUI.Button(new Rect(Screen.width*.40f,Screen.height*.52f,Screen.width*.20f,48),Chapter==Chapters.Length-1?"شروع دوباره":"فصل بعد")) NextChapter();
            }
        }
    }
}
