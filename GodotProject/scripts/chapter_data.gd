extends RefCounted
class_name KavehChapterData

# Ten story chapters and seventeen combat waves. All narrative text is Persian.
const CHAPTERS := [
	{"title":"آتش آهنگری","subtitle":"جرقه نخستین قیام","mission":"از کارگاه آهنگری دفاع کن و نگهبانان را عقب بران","waves":1,"enemy":"guard","count":3,"arena":"forge","reward":25,"story":"سال‌هاست که سایه ضحاک بر ایران افتاده است. کاوه دیگر خاموش نمی‌ماند."},
	{"title":"فریاد ستم‌دیدگان","subtitle":"مردم را همراه کن","mission":"راه میدان را باز کن و سه دسته سرباز را شکست بده","waves":2,"enemy":"guard","count":3,"arena":"village","reward":35,"story":"فریاد دادخواهی از کوچه‌ها برخاسته و مردم آماده همراهی‌اند."},
	{"title":"گذرگاه کوهستان","subtitle":"عبور از کمینگاه","mission":"از گذرگاه سنگی و حمله جنگجویان جان سالم به در ببر","waves":2,"enemy":"beast","count":3,"arena":"mountain","reward":45,"story":"در راه البرز، سایه موجودات کهن و دیده‌بانان ضحاک بر سنگ‌ها افتاده است."},
	{"title":"سیاه‌چال ضحاک","subtitle":"شکستن زنجیرها","mission":"نگهبانان سیاه‌چال را شکست بده و زندانیان را آزاد کن","waves":2,"enemy":"guard","count":4,"arena":"dungeon","reward":55,"story":"در ژرفای دژ، بی‌گناهان در بندند. راه آزادی از میان نگهبانان می‌گذرد."},
	{"title":"آزادسازی زندانیان","subtitle":"هم‌پیمانان تازه","mission":"از زندانیان محافظت کن و فرمانده سیاه‌چال را شکست بده","waves":2,"enemy":"elite","count":3,"arena":"dungeon","reward":65,"story":"زنجیرها گسسته‌اند؛ آزادشدگان سوگند یاری می‌خورند."},
	{"title":"دشت خیزش","subtitle":"پرچم در باد","mission":"موج‌های پیاپی سپاه ضحاک را عقب بران","waves":2,"enemy":"guard","count":4,"arena":"plain","reward":75,"story":"درفش دادخواهی از دور دیده می‌شود و سپاه مردم گرد آن جمع می‌شود."},
	{"title":"جنگل نفرین‌شده","subtitle":"نگهبانان باستانی","mission":"از تله‌ها بگذر و جانوران افسانه‌ای را شکست بده","waves":2,"enemy":"beast","count":4,"arena":"forest","reward":85,"story":"نیروهای تاریکی جنگل کهن را به کمینگاه بدل کرده‌اند."},
	{"title":"دژ ماران","subtitle":"دروازه‌های آهنین","mission":"دژ را پاکسازی کن و فرماندهان ضحاک را از میان بردار","waves":1,"enemy":"elite","count":4,"arena":"fortress","reward":95,"story":"راه کاخ از دژی می‌گذرد که دیوارهایش با ترس و آهن ساخته شده‌اند."},
	{"title":"ساخت درفش کاویانی","subtitle":"امید یک سرزمین","mission":"از آهنگران و پرچم تازه در برابر مهاجمان دفاع کن","waves":1,"enemy":"elite","count":5,"arena":"forge","reward":110,"story":"پیش‌بند چرمی کاوه نشان آزادی می‌شود؛ درفش کاویانی برافراشته خواهد شد."},
	{"title":"نبرد واپسین","subtitle":"رویارویی با ضحاک","mission":"نخست سپاه تاریکی را شکست بده، سپس ضحاک را از تخت به زیر بکش","waves":2,"enemy":"boss","count":1,"arena":"throne","reward":250,"story":"مردم یک‌دل شده‌اند. زمان پایان دادن به فرمانروایی ضحاک فرا رسیده است."}
]

# Wave counts add up to seventeen. A chapter can contain one or two waves.
const WAVE_PLAN := [1, 2, 2, 2, 2, 2, 2, 1, 1, 2]

static func get_chapter(index: int) -> Dictionary:
	return CHAPTERS[clampi(index, 0, CHAPTERS.size() - 1)].duplicate(true)

static func chapter_count() -> int:
	return CHAPTERS.size()

static func total_waves() -> int:
	var total := 0
	for count in WAVE_PLAN:
		total += int(count)
	return total
