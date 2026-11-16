# دارایی‌های هنری و صوتی — KAVEH RISING

## شخصیت‌ها و انیمیشن
- مدل کاوه: KayKit Adventurers / Barbarian — rig متوسط، بافت اصلی و چکش متصل به دست.
- مدل ضحاک: KayKit Adventurers / Mage — rig متوسط، بافت سبز تیره و تاج شاخ‌دار.
- پنج مجموعه GLB از KayKit Character Animations برای Rig_Medium: Movement Basic، Movement Advanced، General، Combat Melee و Special.
- Unity glTFast کلیپ‌ها را وارد می‌کند؛ `CharacterPrefabBuilder` آن‌ها را به AnimationClipهای Legacy تبدیل می‌کند، Prefabهای شخصیت را می‌سازد و در مسیر Resources ذخیره می‌کند.
- `RiggedCharacterAnimation` کلیپ‌های واقعی را برای Idle، Walk، Run، Crouch، Dodge، Hit، Death، Attack و HeavyAttack به‌کار می‌گیرد و در صورت نبود کلیپ مناسب fallback رویه‌ای دارد.

## صداهای بازی
فایل‌های OGG از بسته‌های Kenney با مجوز CC0 در `Assets/Resources/Audio/SFX/` قرار دارند: ضربه سبک و سنگین، زره، صدای چکش، صدای فلز، باز شدن در، حرکت سلاح و قدم‌ها. مجوزهای هر دو بسته کنار فایل‌ها ذخیره شده‌اند.

## محدوده‌ای که عمداً کنار گذاشته شده
موسیقی متن و صداپیشگی فارسی بنا به درخواست کاربر در این مرحله وجود ندارند. زیرنویس داستانی فارسی و مسیر اتصال AudioClip برای صداپیشگی آینده باقی است.

## ساخت
در Unity 2022.3.62f1 پروژه را باز کن. منوی `Kaveh Rising > Build Android APK` ابتدا Prefabها و کلیپ‌ها را از دارایی‌های موجود می‌سازد و سپس APK اندروید را تولید می‌کند. ساخت ابری GitHub Actions به Unity license secrets نیاز دارد.

## اعتبارسنجی
مدل‌ها، فایل‌های انیمیشن و SFX در مخزن ثبت شده‌اند. با این حال، Unity Editor در این محیط اجرا نشده و هیچ APK یا تست روی گوشی واقعی تأیید نشده است. اولین build باید خطاهای Import/Animator را بررسی کند؛ نسخه نهایی تنها پس از build موفق و تست کنترل، انیمیشن، صدا و نرخ فریم قابل اعلام است.
