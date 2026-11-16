# مسیر تکمیل شخصیت، انیمیشن و صدا — KAVEH RISING

این پروژه عمداً موسیقی متن ندارد؛ موسیقی در مرحله‌ای جداگانه و با فایل‌های انتخابی کاربر اضافه می‌شود.

## دارایی‌های سه‌بعدی و مجوز
برای شخصیت‌های Humanoid و انیمیشن‌های قابل Retarget می‌توان از بسته‌های CC0 کواتِرنیوس استفاده کرد:
- Universal Base Characters: https://quaternius.com/packs/universalbasecharacters.html
- Universal Animation Library: https://quaternius.com/packs/universalanimationlibrary.html
- مجوز اعلام‌شده هر دو بسته: CC0 1.0؛ نسخه Standard رایگان، نسخه‌های Source ممکن است پولی باشند.

فایل‌های دانلودشده را پس از بررسی مجوز و ساختار در مسیرهای زیر قرار بده:
- `Assets/Art/Characters/` — FBX شخصیت‌ها و لباس‌ها
- `Assets/Art/Animations/` — FBX کلیپ‌های انیمیشن
- `Assets/Art/Materials/` و `Assets/Art/Textures/` — متریال و تکسچرها

اسکریپت `HumanoidAssetImporter` تنظیم Import مدل‌های این دو مسیر را روی Humanoid می‌گذارد. برای هر شخصیت، Avatar معتبر و Animator Controller لازم است. کنترلر باید پارامترهای زیر را داشته باشد:
`Speed(float)`, `Crouch(bool)`, `Grounded(bool)`, `Attack(trigger)`, `HeavyAttack(trigger)`, `Dodge(trigger)`, `Hit(trigger)`, `Die(trigger)`.

اسکریپت `HumanoidAnimationDriver` این پارامترها را هدایت می‌کند. کلیپ‌های Idle/Walk/Run/Crouch/Attack/HeavyAttack/Dodge/Hit/Death باید در Animator Controller به Stateها و Transitionهای مناسب متصل شوند. انیمیشن‌های ریشه‌دار برای کنترلر بازیکن باید با Root Motion خاموش و حرکت بازی با CharacterController اجرا شوند.

## صداپیشگی فارسی
`VoiceDialogueDirector` زیرنویس فارسی و پخش فایل‌های ضبط‌شده را پشتیبانی می‌کند. برای هر خط دیالوگ یک AudioClip ضبط‌شده به فیلد `voice` بده؛ اگر خالی باشد زیرنویس همچنان نمایش داده می‌شود. فایل‌های WAV/OGG در `Assets/Audio/Voice/` قرار بگیرند و در Inspector به DialogueLineها متصل شوند.

نقش‌های پیشنهادی: راوی، کاوه، ضحاک، زندانی، فرمانده سپاه و شخصیت‌های فرعی. دیالوگ باید جداگانه برای هر فصل ضبط، تدوین و در بازی زمان‌بندی شود. هیچ صدای ضبط‌شده‌ای در این commit جعل یا به‌عنوان صدای حرفه‌ای معرفی نشده است.

## کنترل کیفیت قبل از تحویل
- تمام FBXها در Unity با Avatar سبز و بدون خطای Rig وارد شوند.
- همه کلیپ‌ها روی مدل مقصد Retarget و در حالت Play بررسی شوند.
- Attack/HeavyAttack/Dodge/Hit/Death از نظر زمان‌بندی با hitbox و حرکت بازی هماهنگ شوند.
- دیالوگ‌ها روی بلندگو و هدفون، با زیرنویس فارسی و بدون قطع شدن آزمایش شوند.
- APK روی حداقل یک گوشی اندرویدی واقعی تست شود.
- موسیقی متن در این مرحله عمداً خارج از محدوده است.

## وضعیت فعلی
این commit زیرساخت اتصال Animator و پخش دیالوگ/زیرنویس را اضافه می‌کند؛ به‌تنهایی به معنی وارد شدن مدل‌ها، ضبط صداپیشگی، ساخت Animator Controller یا تست نهایی نیست. این موارد باید با فایل‌های واقعی دارایی و تست Unity تکمیل شوند.
